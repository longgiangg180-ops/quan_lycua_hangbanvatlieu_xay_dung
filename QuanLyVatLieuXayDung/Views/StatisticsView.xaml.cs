using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using QuanLyCuaHang.Models;
using QuanLyCuaHang.Services;
using System.Globalization;

namespace QuanLyVatLieuXayDung.Views;

public partial class StatisticsView : ContentView
{
    private readonly HoaDonService _hoaDonService = new();

    private List<HoaDon> _allInvoices = new();
    private List<DateTime> _availableMonths = new();

    private bool _isLoading;

    public StatisticsView()
    {
        InitializeComponent();

        LoadStatisticsAsync();
    }

    // Tải toàn bộ hóa đơn từ SQL Server
    private async void LoadStatisticsAsync()
    {
        try
        {
            _isLoading = true;

            MonthlyRevenueLabel.Text = "Đang tải...";
            InvoiceCountLabel.Text = "...";

            _allInvoices = await _hoaDonService.GetAll();

            // Lấy danh sách tháng có hóa đơn
            _availableMonths = _allInvoices
                .Select(hd => new DateTime(
                    hd.NgayLap.Year,
                    hd.NgayLap.Month,
                    1))
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            // Luôn bổ sung tháng hiện tại
            DateTime currentMonth = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            if (!_availableMonths.Contains(currentMonth))
            {
                _availableMonths.Add(currentMonth);
            }

            _availableMonths = _availableMonths
                .OrderByDescending(x => x)
                .ToList();

            // Hiển thị danh sách tháng/năm
            MonthPicker.ItemsSource = _availableMonths
                .Select(x => x.ToString(
                    "MM/yyyy",
                    CultureInfo.GetCultureInfo("vi-VN")))
                .ToList();

            // Mặc định chọn tháng hiện tại
            int currentIndex = _availableMonths.IndexOf(currentMonth);

            MonthPicker.SelectedIndex = currentIndex;

            _isLoading = false;

            await UpdateStatisticsAsync(false);
        }
        catch (Exception ex)
        {
            _isLoading = false;

            MonthlyRevenueLabel.Text = "--";
            InvoiceCountLabel.Text = "--";

            await ShowMessage(
                "Lỗi tải dữ liệu",
                "Không thể tải thống kê: " + ex.Message);
        }
    }

    // Khi người dùng chọn tháng khác
    private async void OnMonthChanged(
        object? sender,
        EventArgs e)
    {
        if (_isLoading)
            return;

        await UpdateStatisticsAsync(true);
    }

    // Cập nhật doanh thu và danh sách hóa đơn
    private async Task UpdateStatisticsAsync(bool scrollToInvoices)
    {
        int selectedIndex = MonthPicker.SelectedIndex;

        if (selectedIndex < 0 ||
            selectedIndex >= _availableMonths.Count)
        {
            return;
        }

        DateTime selectedMonth = _availableMonths[selectedIndex];

        DateTime startDate = selectedMonth;
        DateTime endDate = startDate.AddMonths(1);

        // Lọc hóa đơn theo tháng được chọn
        List<HoaDon> monthlyInvoices = _allInvoices
            .Where(hd =>
                hd.NgayLap >= startDate &&
                hd.NgayLap < endDate)
            .OrderByDescending(hd => hd.NgayLap)
            .ToList();

        // Tính tổng doanh thu thực tế
        decimal monthlyRevenue = monthlyInvoices.Sum(
            hd => hd.ThanhToan);

        // Cập nhật tổng doanh thu
        MonthlyRevenueLabel.Text =
            monthlyRevenue.ToString(
                "N0",
                CultureInfo.GetCultureInfo("vi-VN")) + " đ";

        // Cập nhật tháng đang xem
        SelectedMonthLabel.Text =
            $"Tháng {selectedMonth.Month:00}/{selectedMonth.Year}";

        // Cập nhật số lượng hóa đơn
        InvoiceCountLabel.Text =
            monthlyInvoices.Count.ToString(
                "N0",
                CultureInfo.GetCultureInfo("vi-VN"));

        InvoiceListDescription.Text =
            $"Có {monthlyInvoices.Count} hóa đơn trong tháng " +
            $"{selectedMonth.Month:00}/{selectedMonth.Year}";

        // Xóa danh sách cũ
        InvoiceListLayout.Children.Clear();

        EmptyInvoiceLabel.IsVisible =
            monthlyInvoices.Count == 0;

        // Tạo danh sách hóa đơn mới
        foreach (HoaDon hd in monthlyInvoices)
        {
            InvoiceListLayout.Children.Add(
                CreateInvoiceCard(hd));
        }

        // Tự động cuộn đến danh sách hóa đơn
        if (scrollToInvoices)
        {
            await Task.Delay(100);

            await MainScrollView.ScrollToAsync(
                InvoiceSection,
                ScrollToPosition.Start,
                true);
        }
    }

    // Tạo thẻ hiển thị từng hóa đơn
    private View CreateInvoiceCard(HoaDon hd)
    {
        var title = new Label
        {
            Text = $"Hóa đơn #{hd.MaHoaDon}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#172554")
        };

        var customer = new Label
        {
            Text = $"Khách hàng: {hd.TenKhachHang}",
            FontSize = 14,
            TextColor = Color.FromArgb("#4B5563")
        };

        var date = new Label
        {
            Text = $"Ngày lập: {hd.NgayLap:dd/MM/yyyy HH:mm}",
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280")
        };

        var paymentMethod = new Label
        {
            Text = $"Thanh toán: {hd.PhuongThucThanhToan}",
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280")
        };

        var total = new Label
        {
            Text = $"Thành tiền: {hd.ThanhToan:N0} đ",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#059669")
        };

        var detailButton = new Button
        {
            Text = "Xem chi tiết",
            FontSize = 13,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#2563EB"),
            CornerRadius = 8,
            Padding = new Thickness(15, 8),
            HorizontalOptions = LayoutOptions.End,
            CommandParameter = hd
        };

        detailButton.Clicked += OnViewInvoiceDetailClicked;

        var layout = new VerticalStackLayout
        {
            Spacing = 9,
            Children =
            {
                title,
                customer,
                date,
                paymentMethod,
                new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromArgb("#E5E7EB")
                },
                total,
                detailButton
            }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Padding = 16,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 12
            },
            Content = layout
        };
    }

    // Xem chi tiết hóa đơn
    private async void OnViewInvoiceDetailClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not HoaDon hd)
        {
            return;
        }

        try
        {
            var details = await _hoaDonService
                .GetChiTietByHoaDonId(hd.MaHoaDon);

            string detailText =
                $"HÓA ĐƠN #{hd.MaHoaDon}\n" +
                $"Khách hàng: {hd.TenKhachHang}\n" +
                $"Ngày lập: {hd.NgayLap:dd/MM/yyyy HH:mm}\n" +
                $"Phương thức: {hd.PhuongThucThanhToan}\n" +
                "--------------------------------\n";

            if (details.Count > 0)
            {
                detailText += "CHI TIẾT SẢN PHẨM:\n";

                foreach (var item in details)
                {
                    detailText +=
                        $"- {item.TenSanPham}: " +
                        $"{item.SoLuong} {item.DonViTinh} x " +
                        $"{item.DonGiaBan:N0} đ = " +
                        $"{item.ThanhTien:N0} đ\n";
                }
            }
            else
            {
                detailText += "Không có chi tiết hóa đơn.\n";
            }

            detailText +=
                "--------------------------------\n" +
                $"Tổng tiền: {hd.TongTien:N0} đ\n" +
                $"Giảm giá: {hd.GiamGia:N0} đ\n" +
                $"THANH TOÁN: {hd.ThanhToan:N0} đ";

            await ShowMessage("Chi tiết hóa đơn", detailText);
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Lỗi",
                "Không thể xem chi tiết hóa đơn: " + ex.Message);
        }
    }

    // Hiển thị thông báo
    private async Task ShowMessage(
        string title,
        string message)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;

        if (page != null)
        {
            await page.DisplayAlertAsync(
                title,
                message,
                "Đóng");
        }
    }
}
