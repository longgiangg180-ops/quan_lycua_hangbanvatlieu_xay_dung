using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHang.Models
{
    public class SanPham
    {
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string TenSanPham { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập danh mục")]
        public string DanhMuc { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
        public string DonViTinh { get; set; } = "";

        [Range(0.01, 999999999,
            ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal GiaBan { get; set; }

        public string MoTa { get; set; } = "";
    }
}