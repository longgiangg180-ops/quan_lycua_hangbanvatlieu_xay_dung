
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyCuaHang.Models;
using QuanLyVatLieuXayDung.Data;

namespace QuanLyCuaHang.Services
{
    public class KhachHangService
    {
        // 1. Lấy toàn bộ danh sách khách hàng từ SQL Server
        public async Task<List<KhachHang>> GetAll()
        {
            List<KhachHang> customers = new();

            try
            {
                using SqlConnection connection =
                    DatabaseConnection.GetConnection();

                await connection.OpenAsync();

                string query = @"
                    SELECT MaKhachHang, TenKhachHang,
                           SoDienThoai, DiaChi
                    FROM dbo.KhachHang
                    ORDER BY MaKhachHang";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                using SqlDataReader reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    customers.Add(new KhachHang
                    {
                        MaKhachHang = reader.GetInt32(0),
                        TenKhachHang = reader.IsDBNull(1)
                            ? "" : reader.GetString(1),
                        SoDienThoai = reader.IsDBNull(2)
                            ? "" : reader.GetString(2),
                        DiaChi = reader.IsDBNull(3)
                            ? "" : reader.GetString(3)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi lấy danh sách khách hàng: " + ex.Message);
            }

            return customers;
        }

        // 2. Thêm khách hàng mới
        public async Task<bool> Add(KhachHang kh)
        {
            try
            {
                using SqlConnection connection =
                    DatabaseConnection.GetConnection();

                await connection.OpenAsync();

                // Dùng transaction để tránh cấp trùng mã
                using SqlTransaction transaction =
                    (SqlTransaction)await connection.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                try
                {
                    string getIdQuery = @"
                        SELECT ISNULL(MAX(MaKhachHang), 0) + 1
                        FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK)";

                    int newId;

                    using (SqlCommand getIdCommand =
                        new SqlCommand(getIdQuery, connection, transaction))
                    {
                        newId = Convert.ToInt32(
                            await getIdCommand.ExecuteScalarAsync());
                    }

                    string insertQuery = @"
                        INSERT INTO dbo.KhachHang
                            (MaKhachHang, TenKhachHang, SoDienThoai, DiaChi)
                        VALUES
                            (@MaKhachHang, @TenKhachHang, @SoDienThoai, @DiaChi)";

                    using (SqlCommand command =
                        new SqlCommand(insertQuery, connection, transaction))
                    {
                        command.Parameters.Add("@MaKhachHang",
                            SqlDbType.Int).Value = newId;

                        command.Parameters.Add("@TenKhachHang",
                            SqlDbType.NVarChar, 200).Value = kh.TenKhachHang;

                        command.Parameters.Add("@SoDienThoai",
                            SqlDbType.VarChar, 15).Value = kh.SoDienThoai;

                        command.Parameters.Add("@DiaChi",
                            SqlDbType.NVarChar, 400).Value = kh.DiaChi;

                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();

                    kh.MaKhachHang = newId;

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi thêm khách hàng: " + ex.Message);

                return false;
            }
        }

        // 3. Cập nhật thông tin khách hàng
        public async Task<bool> Update(KhachHang kh)
        {
            try
            {
                using SqlConnection connection =
                    DatabaseConnection.GetConnection();

                await connection.OpenAsync();

                string query = @"
                    UPDATE dbo.KhachHang
                    SET TenKhachHang = @TenKhachHang,
                        SoDienThoai = @SoDienThoai,
                        DiaChi = @DiaChi
                    WHERE MaKhachHang = @MaKhachHang";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add("@MaKhachHang",
                    SqlDbType.Int).Value = kh.MaKhachHang;

                command.Parameters.Add("@TenKhachHang",
                    SqlDbType.NVarChar, 200).Value = kh.TenKhachHang;

                command.Parameters.Add("@SoDienThoai",
                    SqlDbType.VarChar, 15).Value = kh.SoDienThoai;

                command.Parameters.Add("@DiaChi",
                    SqlDbType.NVarChar, 400).Value = kh.DiaChi;

                int rows = await command.ExecuteNonQueryAsync();

                return rows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi cập nhật khách hàng: " + ex.Message);

                return false;
            }
        }

        // 4. Xóa khách hàng
        public async Task<bool> Delete(int id)
        {
            try
            {
                using SqlConnection connection =
                    DatabaseConnection.GetConnection();

                await connection.OpenAsync();

                string query = @"
                    DELETE FROM dbo.KhachHang
                    WHERE MaKhachHang = @MaKhachHang";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add("@MaKhachHang",
                    SqlDbType.Int).Value = id;

                int rows = await command.ExecuteNonQueryAsync();

                return rows > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không thể xóa khách hàng do vi phạm khóa ngoại: "
                    + ex.Message);

                throw new InvalidOperationException(
                    "Khách hàng này đã phát sinh hóa đơn hoặc đang được dữ liệu khác tham chiếu. Không thể xóa!",
                    ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi xóa khách hàng: " + ex.ToString());

                throw;
            }
        }

        // 5. Tìm kiếm khách hàng
        public async Task<List<KhachHang>> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            List<KhachHang> customers = new();

            try
            {
                using SqlConnection connection =
                    DatabaseConnection.GetConnection();

                await connection.OpenAsync();

                string query = @"
                    SELECT MaKhachHang, TenKhachHang,
                           SoDienThoai, DiaChi
                    FROM dbo.KhachHang
                    WHERE TenKhachHang LIKE @Keyword
                       OR SoDienThoai LIKE @Keyword
                       OR DiaChi LIKE @Keyword
                    ORDER BY MaKhachHang";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add("@Keyword",
                    SqlDbType.NVarChar, 400).Value =
                    "%" + keyword.Trim() + "%";

                using SqlDataReader reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    customers.Add(new KhachHang
                    {
                        MaKhachHang = reader.GetInt32(0),
                        TenKhachHang = reader.IsDBNull(1)
                            ? "" : reader.GetString(1),
                        SoDienThoai = reader.IsDBNull(2)
                            ? "" : reader.GetString(2),
                        DiaChi = reader.IsDBNull(3)
                            ? "" : reader.GetString(3)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi tìm kiếm khách hàng: " + ex.Message);
            }

            return customers;
        }
    }
}
