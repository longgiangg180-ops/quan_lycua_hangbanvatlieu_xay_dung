using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class KhachHangService
    {
        private readonly HttpClient? _http;

        // Dữ liệu mẫu khởi tạo dựa theo file Quan_ly_ban_vat_lieu.sql
        private static readonly List<KhachHang> _mockData = new()
        {
            new KhachHang { MaKhachHang = 1, TenKhachHang = "Nguyễn Việt Hưng", SoDienThoai = "0354651535", DiaChi = "Hà Nội" },
            new KhachHang { MaKhachHang = 2, TenKhachHang = "Lê Hoàng Hiệp", SoDienThoai = "0942183541", DiaChi = "Hà Nội" },
            new KhachHang { MaKhachHang = 3, TenKhachHang = "Lê Hoàng Long Giang", SoDienThoai = "0853285123", DiaChi = "Hà Tĩnh" },
            new KhachHang { MaKhachHang = 4, TenKhachHang = "Trần Bá Hữu", SoDienThoai = "0895813575", DiaChi = "Bắc Ninh" },
            new KhachHang { MaKhachHang = 5, TenKhachHang = "Nguyễn Hồng Sơn", SoDienThoai = "0835186515", DiaChi = "Thái Bình" }
        };

        public KhachHangService(HttpClient? http = null)
        {
            _http = http;
        }

        // 1. Lấy danh sách khách hàng
        public async Task<List<KhachHang>> GetAll()
        {
            if (_http != null)
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<List<KhachHang>>("api/khachhang");
                    if (data != null && data.Count > 0) return data;
                }
                catch
                {
                    // Khi chưa có API backend đang chạy, tự động fallback về dữ liệu mẫu
                }
            }

            return await Task.FromResult(_mockData.OrderBy(k => k.MaKhachHang).ToList());
        }

        // 2. Thêm khách hàng mới
        public async Task<bool> Add(KhachHang kh)
        {
            if (_http != null)
            {
                try
                {
                    var res = await _http.PostAsJsonAsync("api/khachhang", kh);
                    if (res.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            // Tự động sinh mã nếu chưa có
            if (kh.MaKhachHang <= 0)
            {
                kh.MaKhachHang = _mockData.Count > 0 ? _mockData.Max(k => k.MaKhachHang) + 1 : 1;
            }

            _mockData.Add(kh);
            return await Task.FromResult(true);
        }

        // 3. Cập nhật thông tin khách hàng
        public async Task<bool> Update(KhachHang kh)
        {
            if (_http != null)
            {
                try
                {
                    var res = await _http.PutAsJsonAsync($"api/khachhang/{kh.MaKhachHang}", kh);
                    if (res.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            var index = _mockData.FindIndex(k => k.MaKhachHang == kh.MaKhachHang);
            if (index >= 0)
            {
                _mockData[index] = kh;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // 4. Xóa khách hàng
        public async Task<bool> Delete(int id)
        {
            if (_http != null)
            {
                try
                {
                    var res = await _http.DeleteAsync($"api/khachhang/{id}");
                    if (res.IsSuccessStatusCode) return true;
                }
                catch { }
            }

            var item = _mockData.FirstOrDefault(k => k.MaKhachHang == id);
            if (item != null)
            {
                _mockData.Remove(item);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // 5. Tìm kiếm khách hàng theo tên hoặc SĐT
        public async Task<List<KhachHang>> Search(string keyword)
        {
            var list = await GetAll();
            if (string.IsNullOrWhiteSpace(keyword)) return list;

            return list.Where(k =>
                k.TenKhachHang.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || k.SoDienThoai.Contains(keyword)
                || k.DiaChi.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }
}
