using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHang.Models
{
    public class HoaDon
    {
        public int MaHoaDon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int MaKhachHang { get; set; }

        public int MaNhanVien { get; set; } = 1;

        public DateTime NgayLap { get; set; } = DateTime.Now;

        public string PhuongThucThanhToan { get; set; } = "Tiền mặt"; // Tiền mặt / Chuyển khoản / QR

        public decimal TongTien { get; set; }

        public decimal GiamGia { get; set; } = 0;

        public decimal ThanhToan { get; set; }

        // Các thuộc tính tiện ích phục vụ hiển thị lên giao diện & chi tiết hóa đơn
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public List<ChiTietHoaDon> DanhSachChiTiet { get; set; } = new();
    }
}
