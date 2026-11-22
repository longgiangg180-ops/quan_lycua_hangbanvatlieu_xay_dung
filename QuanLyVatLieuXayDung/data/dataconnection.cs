using Microsoft.Data.SqlClient;

namespace QuanLyVatLieuXayDung.Data
{
    public static class DatabaseConnection
    {
        private static readonly string connectionString =
            "Server=.;" +
            "Database=Vat_lieu_xay_dung;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;";

        // Tạo kết nối SQL Server
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Kiểm tra kết nối database
        public static async Task<bool> TestConnection()
        {
            try
            {
                using SqlConnection connection = GetConnection();

                await connection.OpenAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi kết nối database: " + ex.Message);
                return false;
            }
        }
    }
}