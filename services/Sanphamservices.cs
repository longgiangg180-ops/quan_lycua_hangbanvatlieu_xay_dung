using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class SanPhamService
    {
        private readonly HttpClient? _http;

        // Dữ liệu mẫu sản phẩm đồng bộ từ file SQL của nhóm
        private static readonly List<SanPham> _mockSanPham = new()
        {
            new SanPham { MaSanPham = 1, TenSanPham = "Kính cường lực 10mm", DanhMuc = "Thuỷ tinh", DonViTinh = "m2", GiaBan = 650000, SoLuongTon = 35, MoTa = "Kính cường lực xây dựng" },
            new SanPham { MaSanPham = 2, TenSanPham = "Bồn nước Inox 1000L Ngang", DanhMuc = "Bồn nước", DonViTinh = "Cái", GiaBan = 3200000, SoLuongTon = 12, MoTa = "Bồn chứa nước thiết bị" },
            new SanPham { MaSanPham = 3, TenSanPham = "Gạch ống 4 lỗ 8x8x18", DanhMuc = "Gạch", DonViTinh = "Viên", GiaBan = 1200, SoLuongTon = 5000, MoTa = "Gạch đất sét nung tuynel" },
            new SanPham { MaSanPham = 4, TenSanPham = "Xi măng Portland hỗn hợp PCB40", DanhMuc = "Xi măng", DonViTinh = "Bao", GiaBan = 88000, SoLuongTon = 250, MoTa = "Xi măng bao 50kg" },
            new SanPham { MaSanPham = 5, TenSanPham = "Thép thanh vằn D12 CB300-V", DanhMuc = "Sắt thép", DonViTinh = "Cây", GiaBan = 165000, SoLuongTon = 180, MoTa = "Thép cốt bê tông chịu lực" },
            new SanPham { MaSanPham = 6, TenSanPham = "Cát xây dựng vàng", DanhMuc = "Cát xây dựng", DonViTinh = "m3", GiaBan = 350000, SoLuongTon = 60, MoTa = "Cát vàng trộn bê tông" },
            new SanPham { MaSanPham = 7, TenSanPham = "Đá xây dựng 1x2", DanhMuc = "Đá xây dựng", DonViTinh = "m3", GiaBan = 420000, SoLuongTon = 80, MoTa = "Đá xây dựng 1x2" },
            new SanPham { MaSanPham = 8, TenSanPham = "Đá mi sàng", DanhMuc = "Đá xây dựng", DonViTinh = "m3", GiaBan = 280000, SoLuongTon = 90, MoTa = "Đá mi sàng" },
            new SanPham { MaSanPham = 9, TenSanPham = "Gạch thẻ đặc", DanhMuc = "Gạch", DonViTinh = "Viên", GiaBan = 1500, SoLuongTon = 3000, MoTa = "Gạch thẻ đặc" },
            new SanPham { MaSanPham = 10, TenSanPham = "Gạch block bê tông", DanhMuc = "Gạch", DonViTinh = "Viên", GiaBan = 8500, SoLuongTon = 1500, MoTa = "Gạch không nung" },
            new SanPham { MaSanPham = 11, TenSanPham = "Thép thanh vằn D16 CB300-V", DanhMuc = "Sắt thép", DonViTinh = "Cây", GiaBan = 245000, SoLuongTon = 110, MoTa = "Thép xây dựng chịu lực" },
            new SanPham { MaSanPham = 12, TenSanPham = "Thép cuộn phi 6", DanhMuc = "Sắt thép", DonViTinh = "Kg", GiaBan = 18000, SoLuongTon = 800, MoTa = "Thép cuộn" },
            new SanPham { MaSanPham = 13, TenSanPham = "Xi măng Portland PCB30", DanhMuc = "Xi măng", DonViTinh = "Bao", GiaBan = 75000, SoLuongTon = 200, MoTa = "Xi măng xây tô" },
            new SanPham { MaSanPham = 14, TenSanPham = "Sơn nội thất Dulux 5L", DanhMuc = "Sơn", DonViTinh = "Thùng", GiaBan = 650000, SoLuongTon = 45, MoTa = "Sơn nội thất" },
            new SanPham { MaSanPham = 15, TenSanPham = "Ống nhựa PVC phi 90", DanhMuc = "Thiết bị nước", DonViTinh = "Cây", GiaBan = 125000, SoLuongTon = 75, MoTa = "Ống nhựa PVC" }
        };

        public SanPhamService(HttpClient? http = null)
        {
            _http = http;
        }

        // Lấy danh sách sản phẩm
        public async Task<List<SanPham>> GetAll()
        {
            if (_http != null)
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<List<SanPham>>("api/sanpham");
                    if (data != null && data.Count > 0) return data;
                }
                catch { }
            }

            return await Task.FromResult(_mockSanPham.ToList());
        }

        // Lấy sản phẩm theo mã
        public async Task<SanPham?> GetById(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(s => s.MaSanPham == id);
        }

        // Thêm sản phẩm
        public async Task<bool> Add(SanPham sp)
        {
            if (_http != null)
            {
                try
                {
                    var response = await _http.PostAsJsonAsync("api/sanpham", sp);
                    if (response.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            if (sp.MaSanPham <= 0)
            {
                sp.MaSanPham = _mockSanPham.Count > 0 ? _mockSanPham.Max(s => s.MaSanPham) + 1 : 1;
            }
            _mockSanPham.Add(sp);
            return await Task.FromResult(true);
        }

        // Sửa sản phẩm
        public async Task<bool> Update(SanPham sp)
        {
            if (_http != null)
            {
                try
                {
                    var response = await _http.PutAsJsonAsync($"api/sanpham/{sp.MaSanPham}", sp);
                    if (response.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            var index = _mockSanPham.FindIndex(s => s.MaSanPham == sp.MaSanPham);
            if (index >= 0)
            {
                _mockSanPham[index] = sp;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // Xóa sản phẩm
        public async Task<bool> Delete(int id)
        {
            if (_http != null)
            {
                try
                {
                    var response = await _http.DeleteAsync($"api/sanpham/{id}");
                    if (response.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            var item = _mockSanPham.FirstOrDefault(s => s.MaSanPham == id);
            if (item != null)
            {
                _mockSanPham.Remove(item);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // Tìm kiếm sản phẩm
        public async Task<List<SanPham>> Search(string keyword)
        {
            var danhSach = await GetAll();
            if (string.IsNullOrWhiteSpace(keyword)) return danhSach;

            return danhSach.Where(sp =>
                sp.TenSanPham.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || sp.DanhMuc.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || sp.MaSanPham.ToString().Contains(keyword)
            ).ToList();
        }

        // Trừ số lượng tồn kho (Chức năng 5)
        public async Task<bool> GiamTonKho(int maSanPham, int soLuongGiam)
        {
            var sp = _mockSanPham.FirstOrDefault(s => s.MaSanPham == maSanPham);
            if (sp != null && sp.SoLuongTon >= soLuongGiam)
            {
                sp.SoLuongTon -= soLuongGiam;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
    }
}
