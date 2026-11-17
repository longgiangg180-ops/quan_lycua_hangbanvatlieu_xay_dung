using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class HoaDonService
    {
        private readonly HttpClient? _http;

        // Dữ liệu mẫu khởi tạo theo file Quan_ly_ban_vat_lieu.sql
        private static readonly List<HoaDon> _mockHoaDon = new()
        {
            new HoaDon { MaHoaDon = 1, MaKhachHang = 1, TenKhachHang = "Nguyễn Việt Hưng", MaNhanVien = 1, NgayLap = DateTime.Parse("2026-04-15"), PhuongThucThanhToan = "QR", TongTien = 3250000, GiamGia = 0, ThanhToan = 3250000 },
            new HoaDon { MaHoaDon = 2, MaKhachHang = 2, TenKhachHang = "Lê Hoàng Hiệp", MaNhanVien = 2, NgayLap = DateTime.Parse("2026-04-16"), PhuongThucThanhToan = "Tiền mặt", TongTien = 330000, GiamGia = 0, ThanhToan = 330000 },
            new HoaDon { MaHoaDon = 3, MaKhachHang = 3, TenKhachHang = "Lê Hoàng Long Giang", MaNhanVien = 3, NgayLap = DateTime.Parse("2026-04-17"), PhuongThucThanhToan = "QR", TongTien = 2640000, GiamGia = 0, ThanhToan = 2640000 },
            new HoaDon { MaHoaDon = 4, MaKhachHang = 4, TenKhachHang = "Trần Bá Hữu", MaNhanVien = 4, NgayLap = DateTime.Parse("2026-04-18"), PhuongThucThanhToan = "Tiền mặt", TongTien = 150000, GiamGia = 0, ThanhToan = 150000 },
            new HoaDon { MaHoaDon = 5, MaKhachHang = 5, TenKhachHang = "Nguyễn Hồng Sơn", MaNhanVien = 5, NgayLap = DateTime.Parse("2026-04-19"), PhuongThucThanhToan = "QR", TongTien = 3200000, GiamGia = 0, ThanhToan = 3200000 }
        };

        private static readonly List<ChiTietHoaDon> _mockChiTiet = new()
        {
            new ChiTietHoaDon { MaChiTietHD = 1, MaHoaDon = 1, MaSanPham = 1, TenSanPham = "Kính cường lực 10mm", DonViTinh = "m2", SoLuong = 5, DonGiaBan = 650000 },
            new ChiTietHoaDon { MaChiTietHD = 2, MaHoaDon = 2, MaSanPham = 5, TenSanPham = "Thép thanh vằn D12 CB300-V", DonViTinh = "Cây", SoLuong = 2, DonGiaBan = 165000 },
            new ChiTietHoaDon { MaChiTietHD = 3, MaHoaDon = 3, MaSanPham = 4, TenSanPham = "Xi măng Portland hỗn hợp PCB40", DonViTinh = "Bao", SoLuong = 30, DonGiaBan = 88000 },
            new ChiTietHoaDon { MaChiTietHD = 4, MaHoaDon = 4, MaSanPham = 3, TenSanPham = "Gạch ống 4 lỗ 8x8x18", DonViTinh = "Viên", SoLuong = 125, DonGiaBan = 1200 },
            new ChiTietHoaDon { MaChiTietHD = 5, MaHoaDon = 5, MaSanPham = 2, TenSanPham = "Bồn nước Inox 1000L Ngang", DonViTinh = "Cái", SoLuong = 1, DonGiaBan = 3200000 }
        };

        public HoaDonService(HttpClient? http = null)
        {
            _http = http;
        }

        // 1. Lấy danh sách hóa đơn
        public async Task<List<HoaDon>> GetAll()
        {
            if (_http != null)
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<List<HoaDon>>("api/hoadon");
                    if (data != null && data.Count > 0) return data;
                }
                catch { }
            }

            return await Task.FromResult(_mockHoaDon.OrderByDescending(h => h.NgayLap).ToList());
        }

        // 2. Lấy chi tiết của một hóa đơn
        public async Task<List<ChiTietHoaDon>> GetChiTietByHoaDonId(int maHoaDon)
        {
            if (_http != null)
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<List<ChiTietHoaDon>>($"api/chitiethoadon/{maHoaDon}");
                    if (data != null && data.Count > 0) return data;
                }
                catch { }
            }

            return await Task.FromResult(_mockChiTiet.Where(ct => ct.MaHoaDon == maHoaDon).ToList());
        }

        // 3. Tự động tính tổng tiền hóa đơn (Chức năng 4)
        public decimal TinhTongTien(IEnumerable<ChiTietHoaDon> chiTietList)
        {
            return chiTietList.Sum(ct => ct.ThanhTien);
        }

        // 4. Kiểm tra tồn kho trước khi bán (Chức năng 5 - Kiểm tra)
        // Trả về (true, "") nếu hợp lệ; trả về (false, "thông báo lỗi") nếu không đủ tồn kho
        public (bool IsValid, string ErrorMessage) KiemTraTonKho(
            IEnumerable<ChiTietHoaDon> chiTietList, 
            IEnumerable<SanPham> danhSachSanPham)
        {
            foreach (var item in chiTietList)
            {
                var sp = danhSachSanPham.FirstOrDefault(s => s.MaSanPham == item.MaSanPham);
                if (sp == null)
                {
                    return (false, $"Không tìm thấy sản phẩm có mã {item.MaSanPham} trong hệ thống.");
                }

                if (sp.SoLuongTon < item.SoLuong)
                {
                    return (false, $"Sản phẩm '{sp.TenSanPham}' chỉ còn tồn kho {sp.SoLuongTon} {sp.DonViTinh}, không đủ số lượng bán ({item.SoLuong})!");
                }
            }

            return (true, string.Empty);
        }

        // 5. Lập hóa đơn bán hàng & Tự động trừ tồn kho (Chức năng 2 & 5)
        public async Task<(bool Success, string Message, HoaDon? ResultHoaDon)> TaoHoaDon(
            HoaDon hoaDon,
            List<ChiTietHoaDon> chiTietList,
            List<SanPham> danhSachSanPham)
        {
            if (chiTietList == null || chiTietList.Count == 0)
            {
                return (false, "Hóa đơn phải có ít nhất 1 sản phẩm!", null);
            }

            // Bước A: Kiểm tra tồn kho trước khi bán
            var checkTonKho = KiemTraTonKho(chiTietList, danhSachSanPham);
            if (!checkTonKho.IsValid)
            {
                return (false, checkTonKho.ErrorMessage, null);
            }

            // Bước B: Tự động tính tổng tiền hóa đơn
            hoaDon.TongTien = TinhTongTien(chiTietList);
            hoaDon.ThanhToan = hoaDon.TongTien - hoaDon.GiamGia;
            if (hoaDon.ThanhToan < 0) hoaDon.ThanhToan = 0;

            // Bước C: Tự động sinh mã hóa đơn
            hoaDon.MaHoaDon = _mockHoaDon.Count > 0 ? _mockHoaDon.Max(h => h.MaHoaDon) + 1 : 1;
            hoaDon.NgayLap = DateTime.Now;

            // Bước D: Tự động trừ tồn kho sản phẩm sau khi bán thành công
            foreach (var item in chiTietList)
            {
                var sp = danhSachSanPham.FirstOrDefault(s => s.MaSanPham == item.MaSanPham);
                if (sp != null)
                {
                    sp.SoLuongTon -= item.SoLuong;
                }

                // Gán mã hóa đơn cho từng dòng chi tiết
                item.MaHoaDon = hoaDon.MaHoaDon;
                item.MaChiTietHD = _mockChiTiet.Count > 0 ? _mockChiTiet.Max(c => c.MaChiTietHD) + 1 : 1;
                _mockChiTiet.Add(item);
            }

            hoaDon.DanhSachChiTiet = new List<ChiTietHoaDon>(chiTietList);
            _mockHoaDon.Add(hoaDon);

            // Gửi API backend nếu có
            if (_http != null)
            {
                try
                {
                    await _http.PostAsJsonAsync("api/hoadon", hoaDon);
                }
                catch { }
            }

            return (true, "Lập hóa đơn và cập nhật tồn kho thành công!", hoaDon);
        }
    }
}
