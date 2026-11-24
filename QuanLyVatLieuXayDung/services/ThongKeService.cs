
using Microsoft.Data.SqlClient;
using QuanLyVatLieuXayDung.Data;

namespace QuanLyCuaHang.Services
{
    public class ThongKeService
    {
        // Lấy số liệu tổng quan hiển thị trên Trang chủ
        public async Task<(
            int TongSanPham,
            int HoaDonHomNay,
            decimal DoanhThuHomNay)> GetThongKeTrangChuAsync()
        {
            const string sql = @"
                SELECT
                    (
                        SELECT COUNT(*)
                        FROM dbo.SanPham
                    ) AS TongSanPham,

                    (
                        SELECT COUNT(*)
                        FROM dbo.HoaDon
                        WHERE NgayLap >= @NgayBatDau
                          AND NgayLap < @NgayKetThuc
                    ) AS HoaDonHomNay,

                    (
                        SELECT COALESCE(SUM(ThanhToan), 0)
                        FROM dbo.HoaDon
                        WHERE NgayLap >= @NgayBatDau
                          AND NgayLap < @NgayKetThuc
                    ) AS DoanhThuHomNay;
            ";

            DateTime ngayBatDau = DateTime.Today;
            DateTime ngayKetThuc = ngayBatDau.AddDays(1);

            using SqlConnection connection =
                DatabaseConnection.GetConnection();

            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@NgayBatDau", System.Data.SqlDbType.DateTime)
                .Value = ngayBatDau;

            command.Parameters.Add("@NgayKetThuc", System.Data.SqlDbType.DateTime)
                .Value = ngayKetThuc;

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (
                    reader.GetInt32(reader.GetOrdinal("TongSanPham")),
                    reader.GetInt32(reader.GetOrdinal("HoaDonHomNay")),
                    reader.GetDecimal(reader.GetOrdinal("DoanhThuHomNay"))
                );
            }

            return (0, 0, 0);
        }
    }
}
