using QuanLyCuaHang.Models;
using QuanLyCuaHang.Services;

namespace QuanLyVatLieuXayDung.Views
{
    public partial class CustomersView : ContentView
    {
        private readonly KhachHangService _khachHangService;
        private List<KhachHang> _customersList = new();
        private KhachHang? _selectedCustomer = null;

        public CustomersView()
        {
            InitializeComponent();
            _khachHangService = new KhachHangService();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            _customersList = await _khachHangService.GetAll();
            CustomersCollection.ItemsSource = _customersList;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            var keyword = SearchEntry.Text?.Trim() ?? string.Empty;
            CustomersCollection.ItemsSource = await _khachHangService.Search(keyword);
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = e.NewTextValue?.Trim() ?? string.Empty;
            CustomersCollection.ItemsSource = await _khachHangService.Search(keyword);
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = string.Empty;
            ClearForm();
            LoadCustomers();
        }

        private void OnEditCustomerClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is KhachHang kh)
            {
                _selectedCustomer = kh;
                FormTitleLabel.Text = $"Sửa khách hàng #{kh.MaKhachHang}";
                MaKhachHangEntry.Text = kh.MaKhachHang.ToString();
                TenKhachHangEntry.Text = kh.TenKhachHang;
                SoDienThoaiEntry.Text = kh.SoDienThoai;
                DiaChiEntry.Text = kh.DiaChi;
                SaveButton.Text = "Cập nhật";
                SaveButton.BackgroundColor = Color.FromArgb("#0284C7");
            }
        }

        private async void OnDeleteCustomerClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is KhachHang kh)
            {
                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Xác nhận xóa",
                    $"Bạn có chắc chắn muốn xóa khách hàng '{kh.TenKhachHang}' không?",
                    "Xóa",
                    "Hủy");

                if (confirm)
                {
                    bool result = await _khachHangService.Delete(kh.MaKhachHang);
                    if (result)
                    {
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Thành công", "Đã xóa khách hàng!", "OK");
                        if (_selectedCustomer?.MaKhachHang == kh.MaKhachHang)
                        {
                            ClearForm();
                        }
                        LoadCustomers();
                    }
                    else
                    {
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Lỗi", "Không thể xóa khách hàng!", "OK");
                    }
                }
            }
        }

        private async void OnSaveCustomerClicked(object sender, EventArgs e)
        {
            string ten = TenKhachHangEntry.Text?.Trim() ?? string.Empty;
            string sdt = SoDienThoaiEntry.Text?.Trim() ?? string.Empty;
            string diaChi = DiaChiEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ten))
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Lỗi nhập liệu", "Vui lòng nhập tên khách hàng!", "OK");
                TenKhachHangEntry.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(sdt) || sdt.Length < 10 || sdt.Length > 11)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Lỗi nhập liệu", "Số điện thoại phải từ 10 đến 11 chữ số!", "OK");
                SoDienThoaiEntry.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(diaChi))
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Lỗi nhập liệu", "Vui lòng nhập địa chỉ khách hàng!", "OK");
                DiaChiEntry.Focus();
                return;
            }

            if (_selectedCustomer != null)
            {
                // Cập nhật
                _selectedCustomer.TenKhachHang = ten;
                _selectedCustomer.SoDienThoai = sdt;
                _selectedCustomer.DiaChi = diaChi;

                bool updated = await _khachHangService.Update(_selectedCustomer);
                if (updated)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Thành công", "Cập nhật thông tin khách hàng thành công!", "OK");
                    ClearForm();
                    LoadCustomers();
                }
            }
            else
            {
                // Thêm mới
                var newCustomer = new KhachHang
                {
                    TenKhachHang = ten,
                    SoDienThoai = sdt,
                    DiaChi = diaChi
                };

                bool added = await _khachHangService.Add(newCustomer);
                if (added)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Thành công", "Thêm khách hàng mới thành công!", "OK");
                    ClearForm();
                    LoadCustomers();
                }
            }
        }

        private void OnClearFormClicked(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedCustomer = null;
            FormTitleLabel.Text = "Thêm khách hàng mới";
            MaKhachHangEntry.Text = string.Empty;
            TenKhachHangEntry.Text = string.Empty;
            SoDienThoaiEntry.Text = string.Empty;
            DiaChiEntry.Text = string.Empty;
            SaveButton.Text = "Lưu lại";
            SaveButton.BackgroundColor = Color.FromArgb("#10B981");
        }
    }
}
