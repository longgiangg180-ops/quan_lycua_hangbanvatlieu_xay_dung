
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Storage;
using System.Data;
using QuanLyCuaHang.Models;
using QuanLyVatLieuXayDung.Data;

namespace QuanLyCuaHang.Services
{
    public class SanPhamService
    {
        // Lấy danh sách sản phẩm
        public async Task<List<SanPham>> GetAll()
        {
            var danhSach = new List<SanPham>();

            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT MaSanPham, TenSanPham, DanhMuc,
                       DonViTinh, GiaBan, MoTa, SoLuongTon, AnhSanPham
                FROM dbo.SanPham
                ORDER BY MaSanPham";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                danhSach.Add(MapSanPham(reader));
            }

            return danhSach;
        }

        // Lấy sản phẩm theo mã
        public async Task<SanPham?> GetById(int id)
        {
            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT MaSanPham, TenSanPham, DanhMuc,
                       DonViTinh, GiaBan, MoTa, SoLuongTon, AnhSanPham
                FROM dbo.SanPham
                WHERE MaSanPham = @MaSanPham";

            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@MaSanPham", SqlDbType.Int).Value = id;

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapSanPham(reader);
            }

            return null;
        }

        // Thêm sản phẩm
        public async Task<bool> Add(SanPham sp)
        {
            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            // Dùng Serializable để hạn chế trùng mã khi có
            // nhiều thao tác thêm sản phẩm đồng thời.
            using var transaction =
                connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                string query = @"
                    DECLARE @MaMoi INT;

                    SELECT @MaMoi = ISNULL(MAX(MaSanPham), 0) + 1
                    FROM dbo.SanPham WITH (UPDLOCK, HOLDLOCK);

                    INSERT INTO dbo.SanPham
                    (
                        MaSanPham, TenSanPham, DanhMuc,
                        DonViTinh, GiaBan, MoTa, SoLuongTon, AnhSanPham
                    )
                    VALUES
                    (
                        @MaMoi, @TenSanPham, @DanhMuc,
                        @DonViTinh, @GiaBan, @MoTa, @SoLuongTon, @AnhSanPham
                    );

                    SELECT @MaMoi;";

                using var command = new SqlCommand(query, connection, transaction);

                AddProductParameters(command, sp);

                object? result = await command.ExecuteScalarAsync();

                sp.MaSanPham = Convert.ToInt32(result);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Sửa sản phẩm
        public async Task<bool> Update(SanPham sp)
        {
            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                UPDATE dbo.SanPham
                SET TenSanPham = @TenSanPham,
                    DanhMuc = @DanhMuc,
                    DonViTinh = @DonViTinh,
                    GiaBan = @GiaBan,
                    MoTa = @MoTa,
                    SoLuongTon = @SoLuongTon,
                    AnhSanPham = @AnhSanPham
                WHERE MaSanPham = @MaSanPham";

            using var command = new SqlCommand(query, connection);

            AddProductParameters(command, sp);

            command.Parameters.Add("@MaSanPham", SqlDbType.Int).Value =
                sp.MaSanPham;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // Xóa sản phẩm
        public async Task<bool> Delete(int id)
        {
            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                DELETE FROM dbo.SanPham
                WHERE MaSanPham = @MaSanPham";

            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@MaSanPham", SqlDbType.Int).Value = id;

            try
            {
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException(
                    "Sản phẩm này đã phát sinh hóa đơn hoặc phiếu nhập. " +
                    "Không thể xóa để đảm bảo dữ liệu lịch sử!", ex);
            }
        }

        // Nhập 15 ảnh sản phẩm từ Resources/Raw/Products vào SQL Server
        public async Task<int> NapAnhSanPhamAsync()
        {
            var danhSachAnh = new Dictionary<int, string>
            {
                { 1, "sp001_kinh_cuong_luc_10mm.png" },
                { 2, "sp002_bon_nuoc_inox_1000l_ngang.png" },
                { 3, "sp003_gach_ong_4_lo_8x8x18.png" },
                { 4, "sp004_xi_mang_pcb40.png" },
                { 5, "sp005_thep_thanh_van_d12.png" },
                { 6, "sp006_cat_xay_dung_vang.png" },
                { 7, "sp007_da_xay_dung_1x2.png" },
                { 8, "sp008_da_mi_sang.png" },
                { 9, "sp009_gach_the_dac.png" },
                { 10, "sp010_gach_block_be_tong.png" },
                { 11, "sp011_thep_thanh_van_d16.png" },
                { 12, "sp012_thep_cuon_phi_6.png" },
                { 13, "sp013_xi_mang_pcb30.png" },
                { 14, "sp014_son_noi_that_dulux_5l.png" },
                { 15, "sp015_ong_nhua_pvc_phi_90.png" }
            };

            int soAnhDaCapNhat = 0;

            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                foreach (var item in danhSachAnh)
                {
                    string tenFile = "Products/" + item.Value;

                    using var stream =
                        await FileSystem.OpenAppPackageFileAsync(tenFile);

                    using var memoryStream = new MemoryStream();

                    await stream.CopyToAsync(memoryStream);

                    byte[] anh = memoryStream.ToArray();

                    string query = @"
                        UPDATE dbo.SanPham
                        SET AnhSanPham = @AnhSanPham
                        WHERE MaSanPham = @MaSanPham";

                    using var command = new SqlCommand(
                        query, connection, transaction);

                    command.Parameters.Add("@MaSanPham", SqlDbType.Int)
                        .Value = item.Key;

                    command.Parameters.Add("@AnhSanPham", SqlDbType.VarBinary, -1)
                        .Value = anh;

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        soAnhDaCapNhat++;
                    }
                }

                await transaction.CommitAsync();

                return soAnhDaCapNhat;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Tìm kiếm sản phẩm
        public async Task<List<SanPham>> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAll();
            }

            var danhSach = new List<SanPham>();

            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT MaSanPham, TenSanPham, DanhMuc,
                       DonViTinh, GiaBan, MoTa, SoLuongTon, AnhSanPham
                FROM dbo.SanPham
                WHERE TenSanPham LIKE @Keyword
                   OR DanhMuc LIKE @Keyword
                   OR CONVERT(NVARCHAR(20), MaSanPham) LIKE @Keyword
                ORDER BY MaSanPham";

            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200)
                .Value = "%" + keyword.Trim() + "%";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                danhSach.Add(MapSanPham(reader));
            }

            return danhSach;
        }

        // Trừ số lượng tồn kho
        public async Task<bool> GiamTonKho(int maSanPham, int soLuongGiam)
        {
            if (soLuongGiam <= 0)
            {
                return false;
            }

            using var connection = DatabaseConnection.GetConnection();
            await connection.OpenAsync();

            string query = @"
                UPDATE dbo.SanPham
                SET SoLuongTon = SoLuongTon - @SoLuongGiam
                WHERE MaSanPham = @MaSanPham
                  AND SoLuongTon >= @SoLuongGiam";

            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@MaSanPham", SqlDbType.Int).Value =
                maSanPham;

            command.Parameters.Add("@SoLuongGiam", SqlDbType.Int).Value =
                soLuongGiam;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // Ánh xạ dữ liệu SQL Server sang đối tượng SanPham
        private static SanPham MapSanPham(SqlDataReader reader)
        {
            return new SanPham
            {
                MaSanPham = reader.GetInt32(reader.GetOrdinal("MaSanPham")),

                TenSanPham = reader["TenSanPham"] as string ?? "",

                DanhMuc = reader["DanhMuc"] as string ?? "",

                DonViTinh = reader["DonViTinh"] as string ?? "",

                GiaBan = reader.GetDecimal(reader.GetOrdinal("GiaBan")),

                MoTa = reader["MoTa"] as string ?? "",

                SoLuongTon = reader.GetInt32(reader.GetOrdinal("SoLuongTon")),

                AnhSanPham = reader["AnhSanPham"] == DBNull.Value
                    ? null
                    : (byte[])reader["AnhSanPham"]
            };
        }

        // Thêm các tham số dùng chung cho chức năng thêm và sửa
        private static void AddProductParameters(
            SqlCommand command, SanPham sp)
        {
            command.Parameters.Add("@TenSanPham", SqlDbType.NVarChar, 200)
                .Value = sp.TenSanPham;

            command.Parameters.Add("@DanhMuc", SqlDbType.NVarChar, 200)
                .Value = sp.DanhMuc;

            command.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 100)
                .Value = sp.DonViTinh;

            var giaBan = command.Parameters.Add("@GiaBan", SqlDbType.Decimal);
            giaBan.Precision = 18;
            giaBan.Scale = 2;
            giaBan.Value = sp.GiaBan;

            command.Parameters.Add("@MoTa", SqlDbType.NVarChar, 1000)
                .Value = sp.MoTa ?? "";

            command.Parameters.Add("@SoLuongTon", SqlDbType.Int)
                .Value = sp.SoLuongTon;

            command.Parameters.Add("@AnhSanPham", SqlDbType.VarBinary, -1)
                .Value = sp.AnhSanPham == null
                    ? DBNull.Value
                    : sp.AnhSanPham;
        }
    }
}
