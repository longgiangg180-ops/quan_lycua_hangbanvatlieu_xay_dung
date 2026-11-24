using System.Globalization;
using QuanLyCuaHang.Models;
using QuanLyCuaHang.Services;

namespace QuanLyVatLieuXayDung.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly SanPhamService _sanPhamService;
    private bool _daTaiDuLieu;
    private SanPham? _sanPhamDangSua;

    public ProductsPage()
    {
        InitializeComponent();

        _sanPhamService = new SanPhamService();

        ProductsRoot.Loaded += OnProductsRootLoaded;
    }

    private Page? GetMainPage()
    {
        var windows = Application.Current?.Windows;

        if (windows == null || windows.Count == 0)
            return null;

        return windows[0].Page;
    }

    private async void OnProductsRootLoaded(object? sender, EventArgs e)
    {
        if (_daTaiDuLieu)
            return;

        _daTaiDuLieu = true;

        await LoadProductsAsync();
    }

    // TẢI DANH SÁCH SẢN PHẨM
    private async Task LoadProductsAsync(string keyword = "")
    {
        try
        {
            StatusLabel.Text = "Đang tải dữ liệu...";
            StatusLabel.IsVisible = true;

            List<SanPham> danhSach;

            if (string.IsNullOrWhiteSpace(keyword))
                danhSach = await _sanPhamService.GetAll();
            else
                danhSach = await _sanPhamService.Search(keyword);

            ProductsContainer.Children.Clear();

            if (danhSach.Count == 0)
            {
                StatusLabel.Text = "Không tìm thấy sản phẩm nào.";
                return;
            }

            StatusLabel.Text = $"Tìm thấy {danhSach.Count} sản phẩm.";

            foreach (var sanPham in danhSach)
            {
                ProductsContainer.Children.Add(
                    CreateProductCard(sanPham));
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Không thể tải danh sách sản phẩm.";

            var mainPage = GetMainPage();

            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync(
                    "Lỗi tải dữ liệu",
                    ex.Message,
                    "Đóng");
            }
        }
    }

    // TẠO THẺ SẢN PHẨM
    private View CreateProductCard(SanPham sp)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 120 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 15
        };

        View anhView;

        if (sp.AnhSanPham != null && sp.AnhSanPham.Length > 0)
        {
            byte[] imageData = sp.AnhSanPham;

            anhView = new Image
            {
                Source = ImageSource.FromStream(
                    () => new MemoryStream(imageData)),
                Aspect = Aspect.AspectFit,
                HeightRequest = 120,
                WidthRequest = 120,
                BackgroundColor = Color.FromArgb("#F8FAFC")
            };
        }
        else
        {
            anhView = new Border
            {
                HeightRequest = 120,
                WidthRequest = 120,
                BackgroundColor = Color.FromArgb("#F1F5F9"),
                Stroke = Colors.Transparent,
                Content = new Label
                {
                    Text = "Chưa có ảnh",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#94A3B8"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }

        grid.Add(anhView, 0, 0);

        var thongTin = new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center
        };

        thongTin.Children.Add(new Label
        {
            Text = sp.TenSanPham,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#172554")
        });

        thongTin.Children.Add(new Label
        {
            Text = $"Mã sản phẩm: {sp.MaSanPham}",
            FontSize = 12,
            TextColor = Color.FromArgb("#64748B")
        });

        thongTin.Children.Add(new Label
        {
            Text = $"Danh mục: {sp.DanhMuc}",
            FontSize = 13,
            TextColor = Color.FromArgb("#475569")
        });

        thongTin.Children.Add(new Label
        {
            Text = $"Đơn vị tính: {sp.DonViTinh}",
            FontSize = 13,
            TextColor = Color.FromArgb("#475569")
        });

        thongTin.Children.Add(new Label
        {
            Text = $"Giá bán: {sp.GiaBan:N0} đ",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#DC2626")
        });

        thongTin.Children.Add(new Label
        {
            Text = $"Số lượng tồn: {sp.SoLuongTon}",
            FontSize = 13,
            TextColor = Color.FromArgb("#475569")
        });

        // KHU VỰC CÁC NÚT CHỨC NĂNG
        // Sử dụng HorizontalStackLayout thay cho FlexLayout
        var khuVucNut = new HorizontalStackLayout
        {
            Spacing = 6,
            Margin = new Thickness(0, 5, 0, 0),
            HorizontalOptions = LayoutOptions.Start
        };

        // NÚT CHI TIẾT
        var btnChiTiet = new Button
        {
            Text = "Chi tiết",
            FontSize = 12,
            HeightRequest = 36,
            Padding = new Thickness(10, 4),
            BackgroundColor = Color.FromArgb("#64748B"),
            TextColor = Colors.White,
            CornerRadius = 7
        };

        btnChiTiet.Clicked += async (sender, e) =>
        {
            await ShowProductDetailAsync(sp);
        };

        // NÚT SỬA
        var btnSua = new Button
        {
            Text = "Sửa",
            FontSize = 12,
            HeightRequest = 36,
            Padding = new Thickness(10, 4),
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            CornerRadius = 7
        };

        btnSua.Clicked += async (sender, e) =>
        {
            await OpenEditProductAsync(sp);
        };

        // NÚT XÓA
        var btnXoa = new Button
        {
            Text = "Xóa",
            FontSize = 12,
            HeightRequest = 36,
            Padding = new Thickness(10, 4),
            BackgroundColor = Color.FromArgb("#DC2626"),
            TextColor = Colors.White,
            CornerRadius = 7
        };

        btnXoa.Clicked += async (sender, e) =>
        {
            await DeleteProductAsync(sp);
        };

        khuVucNut.Children.Add(btnChiTiet);
        khuVucNut.Children.Add(btnSua);
        khuVucNut.Children.Add(btnXoa);

        thongTin.Children.Add(khuVucNut);

        grid.Add(thongTin, 1, 0);

        var card = new Border
        {
            Padding = 12,
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 8
            },
            BackgroundColor = Colors.White,
            Content = grid
        };

        return card;
    }

    // XEM CHI TIẾT SẢN PHẨM
    private async Task ShowProductDetailAsync(SanPham sp)
    {
        var mainPage = GetMainPage();

        if (mainPage == null)
            return;

        string moTa = string.IsNullOrWhiteSpace(sp.MoTa)
            ? "Chưa có mô tả sản phẩm."
            : sp.MoTa;

        string noiDung =
            $"Mã sản phẩm: {sp.MaSanPham}\n\n" +
            $"Danh mục: {sp.DanhMuc}\n\n" +
            $"Đơn vị tính: {sp.DonViTinh}\n\n" +
            $"Giá bán: {sp.GiaBan:N0} đ\n\n" +
            $"Số lượng tồn: {sp.SoLuongTon}\n\n" +
            $"Mô tả sản phẩm:\n{moTa}";

        await mainPage.DisplayAlertAsync(
            sp.TenSanPham,
            noiDung,
            "Đóng");
    }

    // TÌM KIẾM SẢN PHẨM
    private async void OnSearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        await LoadProductsAsync(e.NewTextValue ?? "");
    }

    // MỞ BIỂU MẪU THÊM SẢN PHẨM
    private void OnAddProductClicked(object? sender, EventArgs e)
    {
        _sanPhamDangSua = null;

        FormTitleLabel.Text = "Thêm sản phẩm mới";
        SaveProductButton.Text = "Lưu sản phẩm";

        NewTenSanPham.Text = "";
        NewDanhMuc.Text = "";
        NewDonViTinh.Text = "";
        NewGiaBan.Text = "";
        NewSoLuongTon.Text = "0";
        NewMoTa.Text = "";

        AddProductOverlay.IsVisible = true;
    }

    // MỞ BIỂU MẪU SỬA SẢN PHẨM
    private async Task OpenEditProductAsync(SanPham sp)
    {
        _sanPhamDangSua = sp;

        NewTenSanPham.Text = sp.TenSanPham;
        NewDanhMuc.Text = sp.DanhMuc;
        NewDonViTinh.Text = sp.DonViTinh;

        NewGiaBan.Text = sp.GiaBan.ToString(
            CultureInfo.CurrentCulture);

        NewSoLuongTon.Text = sp.SoLuongTon.ToString();
        NewMoTa.Text = sp.MoTa ?? "";

        FormTitleLabel.Text = "Sửa thông tin sản phẩm";
        SaveProductButton.Text = "Cập nhật sản phẩm";

        AddProductOverlay.IsVisible = true;

        await Task.CompletedTask;
    }

    // ĐÓNG BIỂU MẪU
    private void OnCancelAddProductClicked(object? sender, EventArgs e)
    {
        AddProductOverlay.IsVisible = false;
        _sanPhamDangSua = null;
    }

    // LƯU SẢN PHẨM MỚI HOẶC CẬP NHẬT SẢN PHẨM
    private async void OnSaveProductClicked(
        object? sender,
        EventArgs e)
    {
        var mainPage = GetMainPage();

        if (mainPage == null)
            return;

        string tenSanPham = NewTenSanPham.Text?.Trim() ?? "";
        string danhMuc = NewDanhMuc.Text?.Trim() ?? "";
        string donViTinh = NewDonViTinh.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenSanPham))
        {
            await mainPage.DisplayAlertAsync(
                "Thông báo",
                "Vui lòng nhập tên sản phẩm.",
                "Đóng");
            return;
        }

        if (string.IsNullOrWhiteSpace(danhMuc))
        {
            await mainPage.DisplayAlertAsync(
                "Thông báo",
                "Vui lòng nhập danh mục sản phẩm.",
                "Đóng");
            return;
        }

        if (string.IsNullOrWhiteSpace(donViTinh))
        {
            await mainPage.DisplayAlertAsync(
                "Thông báo",
                "Vui lòng nhập đơn vị tính.",
                "Đóng");
            return;
        }

        if (!decimal.TryParse(
                NewGiaBan.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal giaBan) || giaBan <= 0)
        {
            await mainPage.DisplayAlertAsync(
                "Thông báo",
                "Giá bán phải là số lớn hơn 0.",
                "Đóng");
            return;
        }

        if (!int.TryParse(
                NewSoLuongTon.Text,
                out int soLuongTon) || soLuongTon < 0)
        {
            await mainPage.DisplayAlertAsync(
                "Thông báo",
                "Số lượng tồn phải là số nguyên không âm.",
                "Đóng");
            return;
        }

        try
        {
            bool ketQua;

            if (_sanPhamDangSua == null)
            {
                // THÊM SẢN PHẨM MỚI
                var sanPhamMoi = new SanPham
                {
                    TenSanPham = tenSanPham,
                    DanhMuc = danhMuc,
                    DonViTinh = donViTinh,
                    GiaBan = giaBan,
                    SoLuongTon = soLuongTon,
                    MoTa = NewMoTa.Text?.Trim() ?? "",
                    AnhSanPham = null
                };

                ketQua = await _sanPhamService.Add(sanPhamMoi);

                if (ketQua)
                {
                    AddProductOverlay.IsVisible = false;

                    await mainPage.DisplayAlertAsync(
                        "Thành công",
                        $"Đã thêm sản phẩm '{sanPhamMoi.TenSanPham}'.\n" +
                        $"Mã sản phẩm mới: {sanPhamMoi.MaSanPham}",
                        "Đóng");
                }
                else
                {
                    await mainPage.DisplayAlertAsync(
                        "Thông báo",
                        "Không thể thêm sản phẩm.",
                        "Đóng");

                    return;
                }
            }
            else
            {
                // CẬP NHẬT SẢN PHẨM
                var sanPhamCapNhat = new SanPham
                {
                    MaSanPham = _sanPhamDangSua.MaSanPham,
                    TenSanPham = tenSanPham,
                    DanhMuc = danhMuc,
                    DonViTinh = donViTinh,
                    GiaBan = giaBan,
                    SoLuongTon = soLuongTon,
                    MoTa = NewMoTa.Text?.Trim() ?? "",
                    AnhSanPham = _sanPhamDangSua.AnhSanPham
                };

                ketQua = await _sanPhamService.Update(sanPhamCapNhat);

                if (ketQua)
                {
                    AddProductOverlay.IsVisible = false;

                    await mainPage.DisplayAlertAsync(
                        "Thành công",
                        $"Đã cập nhật sản phẩm '{sanPhamCapNhat.TenSanPham}'.",
                        "Đóng");
                }
                else
                {
                    await mainPage.DisplayAlertAsync(
                        "Thông báo",
                        "Không tìm thấy sản phẩm cần cập nhật.",
                        "Đóng");

                    return;
                }
            }

            _sanPhamDangSua = null;

            await LoadProductsAsync(SearchEntry.Text ?? "");
        }
        catch (Exception ex)
        {
            await mainPage.DisplayAlertAsync(
                "Lỗi lưu sản phẩm",
                ex.Message,
                "Đóng");
        }
    }

    // XÓA SẢN PHẨM
    private async Task DeleteProductAsync(SanPham sp)
    {
        var mainPage = GetMainPage();

        if (mainPage == null)
            return;

        // YÊU CẦU XÁC NHẬN TRƯỚC KHI XÓA
        bool xacNhan = await mainPage.DisplayAlertAsync(
            "Xác nhận xóa sản phẩm",
            $"Bạn có chắc chắn muốn xóa sản phẩm:\n\n" +
            $"{sp.TenSanPham}\n" +
            $"Mã sản phẩm: {sp.MaSanPham}\n\n" +
            "Thao tác này không thể hoàn tác!",
            "Đồng ý xóa",
            "Hủy");

        if (!xacNhan)
            return;

        try
        {
            // GỌI SERVICE ĐỂ XÓA TRONG DATABASE
            bool ketQua = await _sanPhamService.Delete(sp.MaSanPham);

            if (ketQua)
            {
                await mainPage.DisplayAlertAsync(
                    "Xóa thành công",
                    $"Đã xóa sản phẩm '{sp.TenSanPham}'.",
                    "Đóng");

                // TẢI LẠI DANH SÁCH
                await LoadProductsAsync(SearchEntry.Text ?? "");
            }
            else
            {
                await mainPage.DisplayAlertAsync(
                    "Thông báo",
                    "Không tìm thấy sản phẩm cần xóa.",
                    "Đóng");
            }
        }
        catch (InvalidOperationException ex)
        {
            await mainPage.DisplayAlertAsync(
                "Không thể xóa sản phẩm",
                ex.Message,
                "Đóng");
        }
        catch (Exception ex)
        {
            await mainPage.DisplayAlertAsync(
                "Lỗi xóa sản phẩm",
                ex.Message,
                "Đóng");
        }
    }

    // NẠP ẢNH SẢN PHẨM
    private async void OnNapAnhClicked(object? sender, EventArgs e)
    {
        var mainPage = GetMainPage();

        if (mainPage == null)
            return;

        bool xacNhan = await mainPage.DisplayAlertAsync(
            "Xác nhận nạp ảnh",
            "Bạn có muốn nạp 15 ảnh sản phẩm vào cơ sở dữ liệu không?",
            "Đồng ý",
            "Hủy");

        if (!xacNhan)
            return;

        try
        {
            int soAnh = await _sanPhamService.NapAnhSanPhamAsync();

            await mainPage.DisplayAlertAsync(
                "Kết quả nạp ảnh",
                $"Đã cập nhật ảnh cho {soAnh}/15 sản phẩm.",
                "Đóng");

            await LoadProductsAsync(SearchEntry.Text ?? "");
        }
        catch (Exception ex)
        {
            await mainPage.DisplayAlertAsync(
                "Lỗi nạp ảnh",
                ex.Message,
                "Đóng");
        }
    }
}
