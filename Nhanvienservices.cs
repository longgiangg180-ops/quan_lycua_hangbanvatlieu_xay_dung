using System.Net.Http.Json;
using QuanLyCuaHang.Models;

namespace QuanLyCuaHang.Services
{
    public class NhanVienService
    {
        private readonly HttpClient _http;

        public NhanVienService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Nhanvien>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<Nhanvien>>("api/nhanvien") ?? new List<Nhanvien>();
        }

        public async Task<bool> Add(Nhanvien nv)
        {
            var response = await _http.PostAsJsonAsync("api/nhanvien", nv);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Update(Nhanvien nv)
        {
            var response = await _http.PutAsJsonAsync($"api/nhanvien/{nv.MaNhanVien}", nv);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Delete(int id)
        {
            var response = await _http.DeleteAsync($"api/nhanvien/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}