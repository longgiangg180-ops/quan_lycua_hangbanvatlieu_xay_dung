using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class SanPhamService
    {
        private readonly HttpClient _http;

        public SanPhamService(HttpClient http)
        {
            _http = http;
        }

        // Lấy danh sách sản phẩm
        public async Task<List<SanPham>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<SanPham>>(
                "api/sanpham") ?? new List<SanPham>();
        }

        // Thêm sản phẩm
        public async Task<bool> Add(SanPham sp)
        {
            var response = await _http.PostAsJsonAsync(
                "api/sanpham", sp);

            return response.IsSuccessStatusCode;
        }

        // Sửa sản phẩm
        public async Task<bool> Update(SanPham sp)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/sanpham/{sp.MaSanPham}", sp);

            return response.IsSuccessStatusCode;
        }

        // Xóa sản phẩm
        public async Task<bool> Delete(int id)
        {
            var response = await _http.DeleteAsync(
                $"api/sanpham/{id}");

            return response.IsSuccessStatusCode;
        }

        // Tìm kiếm sản phẩm
        public async Task<List<SanPham>> Search(string keyword)
        {
            var danhSach = await GetAll();

            return danhSach.Where(sp =>
                sp.TenSanPham.Contains(
                    keyword, StringComparison.OrdinalIgnoreCase)
                || sp.DanhMuc.Contains(
                    keyword, StringComparison.OrdinalIgnoreCase)
                || sp.MaSanPham.ToString().Contains(keyword)
            ).ToList();
        }
    }
}