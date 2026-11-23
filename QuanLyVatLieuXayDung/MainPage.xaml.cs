using Microsoft.Maui.Controls;
using QuanLyCuaHang.Services;
using System.Globalization;

namespace QuanLyVatLieuXayDung;

public partial class MainPage : ContentPage
{
    private View? _homeContent;

    private readonly ThongKeService _thongKeService = new();

    public MainPage()
    {
        InitializeComponent();

        // Lưu nội dung trang chủ ban đầu
        _homeContent = PageContent.Content;

        // Tải dữ liệu thống kê khi mở ứng dụng
        _ = LoadHomeStatisticsAsync();
    }

    // Thay đổi màu nền của các nút menu
    private void SetActiveButton(Button selectedButton)
    {
        HomeButton.BackgroundColor = Colors.Transparent;
        ProductsButton.BackgroundColor = Colors.Transparent;
        CustomersButton.BackgroundColor = Colors.Transparent;
        SuppliersButton.BackgroundColor = Colors.Transparent;
        InvoicesButton.BackgroundColor = Colors.Transparent;
        StatisticsButton.BackgroundColor = Colors.Transparent;

        selectedButton.BackgroundColor = Color.FromArgb("#2563EB");
    }

    // Tải dữ liệu thống kê từ SQL Server
    private async Task LoadHomeStatisticsAsync()
    {
        try
        {
            TotalProductsLabel.Text = "...";
            TodayInvoicesLabel.Text = "...";
            TodayRevenueLabel.Text = "...";

            var thongKe =
                await _thongKeService.GetThongKeTrangChuAsync();

            TotalProductsLabel.Text =
                thongKe.TongSanPham.ToString(
                    "N0",
                    CultureInfo.GetCultureInfo("vi-VN"));

            TodayInvoicesLabel.Text =
                thongKe.HoaDonHomNay.ToString(
                    "N0",
                    CultureInfo.GetCultureInfo("vi-VN"));

            TodayRevenueLabel.Text =
                thongKe.DoanhThuHomNay.ToString(
                    "N0",
                    CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }
        catch (Exception ex)
        {
            TotalProductsLabel.Text = "--";
            TodayInvoicesLabel.Text = "--";
            TodayRevenueLabel.Text = "--";

            System.Diagnostics.Debug.WriteLine(
                "Lỗi tải thống kê trang chủ: " + ex.Message);
        }
    }

    // Hiển thị trang tạm cho các chức năng chưa hoàn thiện
    private void ShowPage(string title, string description)
    {
        PageTitle.Text = title;

        PageContent.Content = new VerticalStackLayout
        {
            Padding = new Thickness(5, 10),
            Spacing = 12,

            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#172554")
                },

                new Label
                {
                    Text = description,
                    FontSize = 15,
                    TextColor = Color.FromArgb("#6B7280")
                }
            }
        };
    }

    // Nút Trang chủ
    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        SetActiveButton(HomeButton);

        PageTitle.Text = "Tổng quan";
        PageContent.Content = _homeContent;

        await LoadHomeStatisticsAsync();
    }

    // Nút Sản phẩm
    private void OnProductsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(ProductsButton);

        PageTitle.Text = "Quản lý sản phẩm";

        var productsPage = new Pages.ProductsPage();

        PageContent.Content = productsPage.Content;
    }

    // Nút Khách hàng
    private void OnCustomersClicked(object? sender, EventArgs e)
    {
        SetActiveButton(CustomersButton);

        PageTitle.Text = "Quản lý khách hàng";
        PageContent.Content = new Views.CustomersView();
    }

    // Nút Nhà cung cấp
    private void OnSuppliersClicked(object? sender, EventArgs e)
    {
        SetActiveButton(SuppliersButton);

        PageTitle.Text = "Quản lý nhà cung cấp";
        PageContent.Content = new Views.SuppliersView();
    }

    // Nút Hóa đơn
    private void OnInvoicesClicked(object? sender, EventArgs e)
    {
        SetActiveButton(InvoicesButton);

        PageTitle.Text = "Quản lý bán hàng & Hóa đơn";
        PageContent.Content = new Views.InvoicesView();
    }

    // Nút Thống kê
    private void OnStatisticsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(StatisticsButton);

        PageTitle.Text = "Thống kê";

        // Mở giao diện thống kê thực tế
        PageContent.Content = new Views.StatisticsView();
    }
}
