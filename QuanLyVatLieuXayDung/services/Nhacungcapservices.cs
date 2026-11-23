
using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyCuaHang.Models;
using QuanLyVatLieuXayDung.Data;

namespace QuanLyCuaHang.Services
{
    public class NhaCungCapService
    {
        // Lấy toàn bộ danh sách nhà cung cấp
        public List<NhaCungCap> GetAll()
        {
            var danhSach = new List<NhaCungCap>();

            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                SELECT MaNCC, TenNCC, SoDienThoai, DiaChi, Email
                FROM dbo.NhaCungCap
                ORDER BY MaNCC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                danhSach.Add(MapNhaCungCap(reader));
            }

            return danhSach;
        }

        // Lấy nhà cung cấp theo mã
        public NhaCungCap? GetById(int maNCC)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                SELECT MaNCC, TenNCC, SoDienThoai, DiaChi, Email
                FROM dbo.NhaCungCap
                WHERE MaNCC = @MaNCC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = maNCC;

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapNhaCungCap(reader);
            }

            return null;
        }

        // Thêm nhà cung cấp
        public void Add(NhaCungCap ncc)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            using SqlTransaction transaction =
                conn.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                string getIdQuery = @"
                    SELECT ISNULL(MAX(MaNCC), 0) + 1
                    FROM dbo.NhaCungCap WITH (UPDLOCK, HOLDLOCK)";

                int newId;

                using (SqlCommand idCmd = new SqlCommand(
                    getIdQuery, conn, transaction))
                {
                    newId = Convert.ToInt32(idCmd.ExecuteScalar());
                }

                string insertQuery = @"
                    INSERT INTO dbo.NhaCungCap
                    (MaNCC, TenNCC, SoDienThoai, DiaChi, Email)
                    VALUES
                    (@MaNCC, @TenNCC, @SoDienThoai, @DiaChi, @Email)";

                using SqlCommand cmd =
                    new SqlCommand(insertQuery, conn, transaction);

                cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = newId;
                cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(ncc.TenNCC)
                        ? DBNull.Value : ncc.TenNCC.Trim();

                cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(ncc.SoDienThoai)
                        ? DBNull.Value : ncc.SoDienThoai.Trim();

                cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(ncc.DiaChi)
                        ? DBNull.Value : ncc.DiaChi.Trim();

                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value =
                    string.IsNullOrWhiteSpace(ncc.Email)
                        ? DBNull.Value : ncc.Email.Trim();

                cmd.ExecuteNonQuery();

                transaction.Commit();

                ncc.MaNCC = newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Cập nhật nhà cung cấp
        public void Update(NhaCungCap ncc)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                UPDATE dbo.NhaCungCap
                SET TenNCC = @TenNCC,
                    SoDienThoai = @SoDienThoai,
                    DiaChi = @DiaChi,
                    Email = @Email
                WHERE MaNCC = @MaNCC";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = ncc.MaNCC;

            cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(ncc.TenNCC)
                    ? DBNull.Value : ncc.TenNCC.Trim();

            cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value =
                string.IsNullOrWhiteSpace(ncc.SoDienThoai)
                    ? DBNull.Value : ncc.SoDienThoai.Trim();

            cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value =
                string.IsNullOrWhiteSpace(ncc.DiaChi)
                    ? DBNull.Value : ncc.DiaChi.Trim();

            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value =
                string.IsNullOrWhiteSpace(ncc.Email)
                    ? DBNull.Value : ncc.Email.Trim();

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy nhà cung cấp cần cập nhật.");
            }
        }

        // Xóa nhà cung cấp
        public void Delete(int maNCC)
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                DELETE FROM dbo.NhaCungCap
                WHERE MaNCC = @MaNCC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = maNCC;

            try
            {
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy nhà cung cấp cần xóa.");
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhà cung cấp này vì đã được sử dụng trong phiếu nhập.");
            }
        }

        // Tìm kiếm nhà cung cấp
        public List<NhaCungCap> Search(string keyword)
        {
            var danhSach = new List<NhaCungCap>();

            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            string query = @"
                SELECT MaNCC, TenNCC, SoDienThoai, DiaChi, Email
                FROM dbo.NhaCungCap
                WHERE
                    CAST(MaNCC AS VARCHAR(20)) LIKE @Keyword
                    OR TenNCC LIKE @Keyword
                    OR SoDienThoai LIKE @Keyword
                    OR DiaChi LIKE @Keyword
                    OR Email LIKE @Keyword
                ORDER BY MaNCC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200)
                .Value = "%" + (keyword ?? "").Trim() + "%";

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                danhSach.Add(MapNhaCungCap(reader));
            }

            return danhSach;
        }

        // Chuyển dữ liệu từ SQL Server sang đối tượng NhaCungCap
        private NhaCungCap MapNhaCungCap(SqlDataReader reader)
        {
            return new NhaCungCap
            {
                MaNCC = reader.GetInt32(reader.GetOrdinal("MaNCC")),

                TenNCC = reader["TenNCC"] == DBNull.Value
                    ? ""
                    : reader["TenNCC"].ToString()!,

                SoDienThoai = reader["SoDienThoai"] == DBNull.Value
                    ? ""
                    : reader["SoDienThoai"].ToString()!,

                DiaChi = reader["DiaChi"] == DBNull.Value
                    ? ""
                    : reader["DiaChi"].ToString()!,

                Email = reader["Email"] == DBNull.Value
                    ? ""
                    : reader["Email"].ToString()!
            };
        }
    }
}
