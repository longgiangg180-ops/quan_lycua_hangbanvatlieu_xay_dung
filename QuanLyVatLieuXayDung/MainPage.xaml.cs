
namespace QuanLyVatLieuXayDung;

public partial class MainPage : ContentPage
{
    // Luu l?i n?i dung trang ch? ban d?u
    private View _homeContent;

    public MainPage()
    {
        InitializeComponent();

        // Luu n?i dung trang ch? d? có th? quay l?i
        _homeContent = PageContent.Content;
    }

    // Thay d?i màu n?n c?a các nút menu
    private void SetActiveButton(Button selectedButton)
    {
        // Ðua t?t c? các nút v? tr?ng thái chua du?c ch?n
        HomeButton.BackgroundColor = Colors.Transparent;
        ProductsButton.BackgroundColor = Colors.Transparent;
        CustomersButton.BackgroundColor = Colors.Transparent;
        SuppliersButton.BackgroundColor = Colors.Transparent;
        InvoicesButton.BackgroundColor = Colors.Transparent;
        StatisticsButton.BackgroundColor = Colors.Transparent;

        // Ð?i màu nút dang du?c ch?n
        selectedButton.BackgroundColor = Color.FromArgb("#2563EB");
    }

    // Hàm hi?n th? m?t trang
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

    // Nút Trang ch?
    private void OnHomeClicked(object sender, EventArgs e)
    {
        SetActiveButton(HomeButton);

        PageTitle.Text = "T?ng quan";
        PageContent.Content = _homeContent;
    }

    // Nút S?n ph?m
    private void OnProductsClicked(object sender, EventArgs e)
    {
        SetActiveButton(ProductsButton);

        ShowPage(
            "Qu?n lý s?n ph?m",
            "Ðây là khu v?c qu?n lý danh sách s?n ph?m v?t li?u xây d?ng."
        );
    }

    // Nút Khách hàng
    private void OnCustomersClicked(object sender, EventArgs e)
    {
        SetActiveButton(CustomersButton);

        ShowPage(
            "Qu?n lý khách hàng",
            "Ðây là khu v?c qu?n lý thông tin khách hàng."
        );
    }

    // Nút Nhà cung c?p
    private void OnSuppliersClicked(object sender, EventArgs e)
    {
        SetActiveButton(SuppliersButton);

        ShowPage(
            "Qu?n lý nhà cung c?p",
            "Ðây là khu v?c qu?n lý thông tin nhà cung c?p."
        );
    }

    // Nút Hóa don
    private void OnInvoicesClicked(object sender, EventArgs e)
    {
        SetActiveButton(InvoicesButton);

        ShowPage(
            "Qu?n lý hóa don",
            "Ðây là khu v?c qu?n lý hóa don bán hàng."
        );
    }

    // Nút Th?ng kê
    private void OnStatisticsClicked(object sender, EventArgs e)
    {
        SetActiveButton(StatisticsButton);

        ShowPage(
            "Th?ng kê",
            "Ðây là khu v?c th?ng kê ho?t d?ng kinh doanh c?a c?a hàng."
        );
    }
}
