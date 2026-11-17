using System.Collections.ObjectModel;
using QuanLyCuaHang.Models;
using QuanLyCuaHang.Services;

namespace QuanLyVatLieuXayDung.Views
{
    public partial class InvoicesView : ContentView
    {
        private readonly KhachHangService _khachHangService;
        private readonly SanPhamService _sanPhamService;
        private readonly HoaDonService _hoaDonService;

        private List<KhachHang> _customers = new();
        private List<SanPham> _products = new();
        private ObservableCollection<ChiTietHoaDon> _cartItems = new();
        private List<HoaDon> _invoicesHistory = new();

        private decimal _totalAmount = 0;
        private decimal _discountAmount = 0;

        public InvoicesView()
        {
            InitializeComponent();

            _khachHangService = new KhachHangService();
            _sanPhamService = new SanPhamService();
            _hoaDonService = new HoaDonService();

            CartItemsCollection.ItemsSource = _cartItems;

            LoadData();
        }

        private async void LoadData()
        {
            // Tải danh sách khách hàng
            _customers = await _khachHangService.GetAll();
            CustomerPicker.ItemsSource = _customers;

            // Tải danh sách sản phẩm
            _products = await _sanPhamService.GetAll();
            ProductPicker.ItemsSource = _products;

            // Tải lịch sử hóa đơn
            await LoadInvoicesHistory();
        }

        private async Task LoadInvoicesHistory()
        {
            _invoicesHistory = await _hoaDonService.GetAll();
            InvoicesHistoryCollection.ItemsSource = null;
            InvoicesHistoryCollection.ItemsSource = _invoicesHistory;
        }

        private void OnProductPickerChanged(object sender, EventArgs e)
        {
            if (ProductPicker.SelectedItem is SanPham sp)
            {
                StockLabel.Text = $"Tồn kho khả dụng: {sp.SoLuongTon} {sp.DonViTinh}";
                StockLabel.TextColor = sp.SoLuongTon > 0 ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
                PriceLabel.Text = $"Đơn giá: {sp.GiaBan:N0} đ";
            }
            else
            {
                StockLabel.Text = "Tồn kho: --";
                PriceLabel.Text = "Đơn giá: --";
            }
        }

        // CHỨC NĂNG 3 & 5: Thêm sản phẩm vào giỏ & Kiểm tra tồn kho trước khi thêm
        private async void OnAddProductToCartClicked(object sender, EventArgs e)
        {
            if (ProductPicker.SelectedItem is not SanPham sp)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Thông báo", "Vui lòng chọn một sản phẩm!", "OK");
                return;
            }

            if (!int.TryParse(QuantityEntry.Text?.Trim(), out int qty) || qty <= 0)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Lỗi", "Số lượng mua phải là số nguyên dương lớn hơn 0!", "OK");
                QuantityEntry.Focus();
                return;
            }

            // Kiểm tra số lượng tồn kho (Chức năng 5)
            var existingInCart = _cartItems.FirstOrDefault(c => c.MaSanPham == sp.MaSanPham);
            int totalDesiredQty = qty + (existingInCart?.SoLuong ?? 0);

            if (totalDesiredQty > sp.SoLuongTon)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Cảnh báo tồn kho",
                    $"Sản phẩm '{sp.TenSanPham}' chỉ còn tồn kho {sp.SoLuongTon} {sp.DonViTinh}. Bạn không thể bán quá số lượng tồn!",
                    "Đã hiểu");
                return;
            }

            if (existingInCart != null)
            {
                // Cập nhật số lượng
                existingInCart.SoLuong += qty;
                // Làm mới danh sách hiển thị
                int idx = _cartItems.IndexOf(existingInCart);
                _cartItems.RemoveAt(idx);
                _cartItems.Insert(idx, existingInCart);
            }
            else
            {
                _cartItems.Add(new ChiTietHoaDon
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    DonViTinh = sp.DonViTinh,
                    DonGiaBan = sp.GiaBan,
                    SoLuong = qty
                });
            }

            QuantityEntry.Text = "1";
            RecalculateTotals();
        }

        private void OnRemoveCartItemClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ChiTietHoaDon item)
            {
                _cartItems.Remove(item);
                RecalculateTotals();
            }
        }

        // CHỨC NĂNG 4: Tự động tính tổng tiền hóa đơn & số tiền thanh toán
        private void RecalculateTotals()
        {
            _totalAmount = _hoaDonService.TinhTongTien(_cartItems);

            decimal finalAmount = _totalAmount - _discountAmount;
            if (finalAmount < 0) finalAmount = 0;

            TotalAmountLabel.Text = $"{_totalAmount:N0} đ";
            FinalPaymentLabel.Text = $"{finalAmount:N0} đ";
        }

        private void OnDiscountChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(e.NewTextValue?.Trim(), out decimal discount) && discount >= 0)
            {
                _discountAmount = discount;
            }
            else
            {
                _discountAmount = 0;
            }
            RecalculateTotals();
        }

        // CHỨC NĂNG 2 & 5: Lập hóa đơn, kiểm tra tồn kho & TỰ ĐỘNG TRỪ TỒN KHO
        private async void OnSubmitInvoiceClicked(object sender, EventArgs e)
        {
            if (CustomerPicker.SelectedItem is not KhachHang kh)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Thông báo", "Vui lòng chọn khách hàng!", "OK");
                CustomerPicker.Focus();
                return;
            }

            if (_cartItems.Count == 0)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Thông báo", "Hóa đơn chưa có sản phẩm nào trong giỏ!", "OK");
                return;
            }

            var phuongThuc = PaymentMethodPicker.SelectedItem?.ToString() ?? "Tiền mặt";

            var hoaDon = new HoaDon
            {
                MaKhachHang = kh.MaKhachHang,
                TenKhachHang = kh.TenKhachHang,
                SoDienThoai = kh.SoDienThoai,
                PhuongThucThanhToan = phuongThuc,
                GiamGia = _discountAmount
            };

            // Gọi service xử lý kiểm tra kho và tự động trừ tồn kho (Chức năng 5)
            var result = await _hoaDonService.TaoHoaDon(hoaDon, _cartItems.ToList(), _products);

            if (!result.Success)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Không thể thanh toán", result.Message, "OK");
                return;
            }

            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Thành công",
                $"Đã lập Hóa đơn #{result.ResultHoaDon!.MaHoaDon} thành công!\nSố tiền: {result.ResultHoaDon.ThanhToan:N0} đ\nĐã tự động cập nhật trừ số lượng tồn kho sản phẩm.",
                "Đóng");

            // Xóa trắng giỏ hàng
            _cartItems.Clear();
            DiscountEntry.Text = "0";
            RecalculateTotals();

            // Cập nhật lại nhãn tồn kho sản phẩm đang chọn
            OnProductPickerChanged(this, EventArgs.Empty);

            // Tải lại lịch sử hóa đơn
            await LoadInvoicesHistory();
        }

        private async void OnViewInvoiceDetailClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is HoaDon hd)
            {
                var details = await _hoaDonService.GetChiTietByHoaDonId(hd.MaHoaDon);
                string detailText = $"HÓA ĐƠN #{hd.MaHoaDon}\n" +
                                   $"Khách hàng: {hd.TenKhachHang}\n" +
                                   $"Ngày lập: {hd.NgayLap:dd/MM/yyyy HH:mm}\n" +
                                   $"Phương thức: {hd.PhuongThucThanhToan}\n" +
                                   $"---------------------------------\n";

                if (details.Count > 0)
                {
                    detailText += "Chi tiết sản phẩm:\n";
                    foreach (var item in details)
                    {
                        detailText += $"- {item.TenSanPham}: {item.SoLuong} {item.DonViTinh} x {item.DonGiaBan:N0} = {item.ThanhTien:N0} đ\n";
                    }
                }
                else
                {
                    detailText += "Chi tiết: Không có bản ghi chi tiết.\n";
                }

                detailText += $"---------------------------------\n" +
                              $"Tổng tiền: {hd.TongTien:N0} đ\n" +
                              $"Giảm giá: {hd.GiamGia:N0} đ\n" +
                              $"THANH TOÁN: {hd.ThanhToan:N0} đ";

                await Application.Current!.Windows[0].Page!.DisplayAlert("Chi tiết hóa đơn", detailText, "Đóng");
            }
        }
    }
}
