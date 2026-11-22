namespace QuanLyVatLieuXayDung;

public partial class MainPage : ContentPage
{
    // Lưu lại nội dung trang chủ ban đầu
    private View _homeContent;

    public MainPage()
    {
    InitializeComponent();

    // Lưu nội dung trang chủ để có thể quay lại
    _homeContent = PageContent.Content;

    // Kiểm tra kết nối database
    }

private async Task CheckDatabaseConnection()
{
    bool isConnected =
        await QuanLyVatLieuXayDung.Data.DatabaseConnection.TestConnection();

    if (isConnected)
    {
        await DisplayAlert(
            "Kết nối thành công",
            "Ứng dụng đã kết nối được với database Vat_lieu_xay_dung!",
            "OK"
        );
    }
    else
    {
        await DisplayAlert(
            "Kết nối thất bại",
            "Không thể kết nối database. Hãy kiểm tra lại cấu hình SQL Server.",
            "OK"
        );
    }
}

    // Thay đổi màu nền của các nút menu
    private void SetActiveButton(Button selectedButton)
    {
        // Đưa tất cả các nút về trạng thái chưa được chọn
        HomeButton.BackgroundColor = Colors.Transparent;
        ProductsButton.BackgroundColor = Colors.Transparent;
        CustomersButton.BackgroundColor = Colors.Transparent;
        SuppliersButton.BackgroundColor = Colors.Transparent;
        InvoicesButton.BackgroundColor = Colors.Transparent;
        StatisticsButton.BackgroundColor = Colors.Transparent;

        // Đổi màu nút đang được chọn
        selectedButton.BackgroundColor = Color.FromArgb("#2563EB");
    }

    // Hàm hiển thị một trang tạm
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
    private void OnHomeClicked(object sender, EventArgs e)
    {
        SetActiveButton(HomeButton);

        PageTitle.Text = "Tổng quan";
        PageContent.Content = _homeContent;
    }

    // Nút Sản phẩm
    private void OnProductsClicked(object sender, EventArgs e)
    {
        SetActiveButton(ProductsButton);

        ShowPage(
            "Quản lý sản phẩm",
            "Đây là khu vực quản lý danh sách sản phẩm vật liệu xây dựng."
        );
    }

    // Nút Khách hàng (Thành viên 4)
    private void OnCustomersClicked(object sender, EventArgs e)
    {
        SetActiveButton(CustomersButton);
        PageTitle.Text = "Quản lý khách hàng";
        PageContent.Content = new Views.CustomersView();
    }

    // Nút Nhà cung cấp
    private void OnSuppliersClicked(object sender, EventArgs e)
    {
        SetActiveButton(SuppliersButton);

        ShowPage(
            "Quản lý nhà cung cấp",
            "Đây là khu vực quản lý thông tin nhà cung cấp."
        );
    }

    // Nút Hóa đơn (Thành viên 4)
    private void OnInvoicesClicked(object sender, EventArgs e)
    {
        SetActiveButton(InvoicesButton);
        PageTitle.Text = "Quản lý bán hàng & Hóa đơn";
        PageContent.Content = new Views.InvoicesView();
    }

    // Nút Thống kê
    private void OnStatisticsClicked(object sender, EventArgs e)
    {
        SetActiveButton(StatisticsButton);

        ShowPage(
            "Thống kê",
            "Đây là khu vực thống kê hoạt động kinh doanh của cửa hàng."
        );
    }
}