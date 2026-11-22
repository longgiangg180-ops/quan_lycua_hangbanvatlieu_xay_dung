
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHang.Models
{
    public class ChiTietHoaDon
    {
        public int MaChiTietHD { get; set; }

        public int MaHoaDon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int MaSanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng mua phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        public decimal DonGiaBan { get; set; }

        // Tự động tính thành tiền = Số lượng x Đơn giá bán
        public decimal ThanhTien => SoLuong * DonGiaBan;

        // Các thuộc tính tiện ích phục vụ hiển thị lên giao diện
        public string? TenSanPham { get; set; }

        public string? DonViTinh { get; set; }
    }
}
