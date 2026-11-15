using System.Net.Http.Json;
using QuanLyCuaHang.Models; 

namespace QuanLyCuaHang.Services
{
    public class PhieuNhapService
    {
        private readonly HttpClient _http;

        public PhieuNhapService(HttpClient http)
        {
            _http = http;
        }
       
        public async Task<List<Phieunhap>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<Phieunhap>>("api/phieunhap") ?? new List<Phieunhap>();
        }

        public async Task<bool> Add(Phieunhap phieuNhap)
        {
            var response = await _http.PostAsJsonAsync("api/phieunhap", phieuNhap);
            return response.IsSuccessStatusCode;
        }
    }
}