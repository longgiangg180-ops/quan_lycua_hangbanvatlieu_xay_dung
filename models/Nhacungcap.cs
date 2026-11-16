using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHang.Models
{
    public class NhaCungCap
    {
        public int MaNCC { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
        public string TenNCC { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$",
            ErrorMessage = "Số điện thoại phải có 10-11 chữ số")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = "";
    }
}