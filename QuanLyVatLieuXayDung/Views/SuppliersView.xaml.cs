
using QuanLyCuaHang.Models;
using QuanLyCuaHang.Services;

namespace QuanLyVatLieuXayDung.Views;

public partial class SuppliersView : ContentView
{
    private readonly NhaCungCapService _nhaCungCapService;

    private int _maNCCDangSua = 0;

    public SuppliersView()
    {
        InitializeComponent();

        _nhaCungCapService = new NhaCungCapService();

        LoadSuppliers();
    }

    // Hiển thị thông báo
    private async Task ShowMessage(string title, string message)
    {
        if (Application.Current?.MainPage is Page page)
        {
            await page.DisplayAlertAsync(title, message, "Đóng");
        }
    }

    // Hiển thị đầy đủ tên nhà cung cấp khi nhấn vào tên
    private async void OnSupplierNameTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Label label ||
            label.BindingContext is not NhaCungCap ncc)
        {
            return;
        }

        await ShowMessage("Tên nhà cung cấp", ncc.TenNCC);
    }

    // Tải danh sách nhà cung cấp
    private void LoadSuppliers()
    {
        try
        {
            var danhSach = _nhaCungCapService.GetAll();
            SuppliersCollection.ItemsSource = danhSach;
        }
        catch (Exception ex)
        {
            _ = ShowMessage(
                "Lỗi",
                "Không thể tải danh sách nhà cung cấp: " + ex.Message);
        }
    }

    // Tìm kiếm khi nội dung ô tìm kiếm thay đổi
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchSuppliers(e.NewTextValue ?? "");
    }

    // Nhấn nút tìm kiếm
    private void OnSearchClicked(object sender, EventArgs e)
    {
        SearchSuppliers(SearchEntry.Text ?? "");
    }

    // Thực hiện tìm kiếm
    private void SearchSuppliers(string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadSuppliers();
                return;
            }

            var danhSach = _nhaCungCapService.Search(keyword);
            SuppliersCollection.ItemsSource = danhSach;
        }
        catch (Exception ex)
        {
            _ = ShowMessage(
                "Lỗi",
                "Lỗi tìm kiếm nhà cung cấp: " + ex.Message);
        }
    }

    // Làm mới danh sách
    private void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchEntry.Text = "";
        ClearForm();
        LoadSuppliers();
    }

    // Đưa thông tin nhà cung cấp lên form để sửa
    private void OnEditSupplierClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not NhaCungCap ncc)
        {
            return;
        }

        _maNCCDangSua = ncc.MaNCC;

        FormTitleLabel.Text = "Cập nhật nhà cung cấp";

        MaNCCEntry.Text = ncc.MaNCC.ToString();
        TenNCCEntry.Text = ncc.TenNCC;
        SoDienThoaiEntry.Text = ncc.SoDienThoai;
        DiaChiEntry.Text = ncc.DiaChi;
        EmailEntry.Text = ncc.Email;

        SaveButton.Text = "Cập nhật";
    }

    // Thêm mới hoặc cập nhật nhà cung cấp
    private async void OnSaveSupplierClicked(object sender, EventArgs e)
    {
        string tenNCC = TenNCCEntry.Text?.Trim() ?? "";
        string soDienThoai = SoDienThoaiEntry.Text?.Trim() ?? "";
        string diaChi = DiaChiEntry.Text?.Trim() ?? "";
        string email = EmailEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenNCC))
        {
            await ShowMessage(
                "Thông báo",
                "Vui lòng nhập tên nhà cung cấp.");

            TenNCCEntry.Focus();
            return;
        }

        if (!string.IsNullOrWhiteSpace(soDienThoai) &&
            !System.Text.RegularExpressions.Regex.IsMatch(
                soDienThoai, @"^[0-9]{10,11}$"))
        {
            await ShowMessage(
                "Thông báo",
                "Số điện thoại phải có 10-11 chữ số.");

            SoDienThoaiEntry.Focus();
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) &&
            !System.Net.Mail.MailAddress.TryCreate(email, out _))
        {
            await ShowMessage(
                "Thông báo",
                "Email không đúng định dạng.");

            EmailEntry.Focus();
            return;
        }

        var ncc = new NhaCungCap
        {
            MaNCC = _maNCCDangSua,
            TenNCC = tenNCC,
            SoDienThoai = soDienThoai,
            DiaChi = diaChi,
            Email = email
        };

        try
        {
            if (_maNCCDangSua == 0)
            {
                _nhaCungCapService.Add(ncc);

                await ShowMessage(
                    "Thành công",
                    "Đã thêm nhà cung cấp mới.");
            }
            else
            {
                _nhaCungCapService.Update(ncc);

                await ShowMessage(
                    "Thành công",
                    "Đã cập nhật thông tin nhà cung cấp.");
            }

            ClearForm();
            LoadSuppliers();
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Lỗi",
                "Không thể lưu nhà cung cấp: " + ex.Message);
        }
    }

    // Xóa nhà cung cấp
    private async void OnDeleteSupplierClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not NhaCungCap ncc)
        {
            return;
        }

        bool confirm = false;

        if (Application.Current?.MainPage is Page page)
        {
            confirm = await page.DisplayAlertAsync(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa nhà cung cấp '{ncc.TenNCC}' không?",
                "Xóa",
                "Hủy");
        }

        if (!confirm)
            return;

        try
        {
            _nhaCungCapService.Delete(ncc.MaNCC);

            await ShowMessage(
                "Thành công",
                "Đã xóa nhà cung cấp.");

            if (_maNCCDangSua == ncc.MaNCC)
            {
                ClearForm();
            }

            LoadSuppliers();
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Không thể xóa",
                ex.Message);
        }
    }

    // Xóa dữ liệu trên form
    private void OnClearFormClicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _maNCCDangSua = 0;

        MaNCCEntry.Text = "";
        TenNCCEntry.Text = "";
        SoDienThoaiEntry.Text = "";
        DiaChiEntry.Text = "";
        EmailEntry.Text = "";

        FormTitleLabel.Text = "Thêm nhà cung cấp mới";
        SaveButton.Text = "Lưu lại";
    }
}
