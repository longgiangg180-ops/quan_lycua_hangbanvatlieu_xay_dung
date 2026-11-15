using System;
using System.Collections.Generic;

namespace Quan_ly_ban_vat_lieu.Models
{
    public class Phieunhap
    {
        public int MaPN { get; set; }
        public DateTime NgayNhap { get; set; }
        public int MaNV { get; set; }
        public int MaNCC { get; set; }
        public List<Chitietphieunhap> ChiTietPhieuNhaps { get; set; } = new List<Chitietphieunhap>();
    }
}