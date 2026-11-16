using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class NhaCungCapService
    {
        private readonly HttpClient _http;

        public NhaCungCapService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NhaCungCap>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<NhaCungCap>>(
                "api/nhacungcap") ?? new List<NhaCungCap>();
        }

        public async Task<bool> Add(NhaCungCap ncc)
        {
            var response = await _http.PostAsJsonAsync(
                "api/nhacungcap", ncc);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Update(NhaCungCap ncc)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/nhacungcap/{ncc.MaNCC}", ncc);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Delete(int id)
        {
            var response = await _http.DeleteAsync(
                $"api/nhacungcap/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<List<NhaCungCap>> Search(string keyword)
        {
            var danhSach = await GetAll();

            return danhSach.Where(ncc =>
                ncc.TenNCC.Contains(
                    keyword, StringComparison.OrdinalIgnoreCase)
                || ncc.SoDienThoai.Contains(keyword)
                || ncc.MaNCC.ToString().Contains(keyword)
            ).ToList();
        }
    }
}