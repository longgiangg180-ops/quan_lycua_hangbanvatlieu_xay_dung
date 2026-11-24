
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyCuaHang.Models;
using QuanLyVatLieuXayDung.Data;

namespace QuanLyCuaHang.Services
{
    public class HoaDonService
    {
        // 1. Lấy danh sách hóa đơn từ SQL Server
        public async Task<List<HoaDon>> GetAll()
        {
            var danhSach = new List<HoaDon>();

            const string sql = @"
                SELECT
                    hd.MaHoaDon,
                    hd.MaKhachHang,
                    kh.TenKhachHang,
                    hd.MaNhanVien,
                    hd.NgayLap,
                    hd.PhuongThucThanhToan,
                    hd.TongTien,
                    hd.GiamGia,
                    hd.ThanhToan
                FROM dbo.HoaDon hd
                LEFT JOIN dbo.KhachHang kh
                    ON hd.MaKhachHang = kh.MaKhachHang
                ORDER BY hd.NgayLap DESC;";

            using SqlConnection connection =
                DatabaseConnection.GetConnection();

            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                danhSach.Add(new HoaDon
                {
                    MaHoaDon = reader.GetInt32(reader.GetOrdinal("MaHoaDon")),
                    MaKhachHang = reader.GetInt32(reader.GetOrdinal("MaKhachHang")),
                    TenKhachHang = reader["TenKhachHang"] == DBNull.Value
                        ? ""
                        : reader["TenKhachHang"].ToString()!,
                    MaNhanVien = reader.GetInt32(reader.GetOrdinal("MaNhanVien")),
                    NgayLap = reader.GetDateTime(reader.GetOrdinal("NgayLap")),
                    PhuongThucThanhToan =
                        reader["PhuongThucThanhToan"]?.ToString() ?? "",
                    TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                    GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                    ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan"))
                });
            }

            return danhSach;
        }

        // 2. Lấy chi tiết hóa đơn từ SQL Server
        public async Task<List<ChiTietHoaDon>> GetChiTietByHoaDonId(
            int maHoaDon)
        {
            var danhSach = new List<ChiTietHoaDon>();

            const string sql = @"
                SELECT
                    ct.MaChiTietHD,
                    ct.MaHoaDon,
                    ct.MaSanPham,
                    sp.TenSanPham,
                    sp.DonViTinh,
                    ct.SoLuong,
                    ct.DonGiaBan,
                    ct.ThanhTien
                FROM dbo.ChiTietHoaDon ct
                LEFT JOIN dbo.SanPham sp
                    ON ct.MaSanPham = sp.MaSanPham
                WHERE ct.MaHoaDon = @MaHoaDon
                ORDER BY ct.MaChiTietHD;";

            using SqlConnection connection =
                DatabaseConnection.GetConnection();

            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@MaHoaDon", SqlDbType.Int).Value =
                maHoaDon;

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                danhSach.Add(new ChiTietHoaDon
                {
                    MaChiTietHD = reader.GetInt32(
                        reader.GetOrdinal("MaChiTietHD")),

                    MaHoaDon = reader.GetInt32(
                        reader.GetOrdinal("MaHoaDon")),

                    MaSanPham = reader.GetInt32(
                        reader.GetOrdinal("MaSanPham")),

                    TenSanPham = reader["TenSanPham"] == DBNull.Value
                        ? ""
                        : reader["TenSanPham"].ToString()!,

                    DonViTinh = reader["DonViTinh"] == DBNull.Value
                        ? ""
                        : reader["DonViTinh"].ToString()!,

                    SoLuong = reader.GetInt32(
                        reader.GetOrdinal("SoLuong")),

                    DonGiaBan = reader.GetDecimal(
                        reader.GetOrdinal("DonGiaBan")),

                    
                });
            }

            return danhSach;
        }

        // 3. Tính tổng tiền hóa đơn
        public decimal TinhTongTien(
            IEnumerable<ChiTietHoaDon> chiTietList)
        {
            return chiTietList.Sum(ct => ct.ThanhTien);
        }

        // 4. Tạo hóa đơn và lưu xuống SQL Server
        // Không còn kiểm tra hoặc trừ số lượng tồn kho
        public async Task<(bool Success, string Message, HoaDon? ResultHoaDon)>
            TaoHoaDon(
                HoaDon hoaDon,
                List<ChiTietHoaDon> chiTietList,
                List<SanPham> danhSachSanPham)
        {
            if (chiTietList == null || chiTietList.Count == 0)
            {
                return (false,
                    "Hóa đơn phải có ít nhất 1 sản phẩm!",
                    null);
            }

            using SqlConnection connection =
                DatabaseConnection.GetConnection();

            await connection.OpenAsync();

            using SqlTransaction transaction =
                connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                // Bước A: Tính tổng tiền
                hoaDon.TongTien = TinhTongTien(chiTietList);

                hoaDon.ThanhToan =
                    hoaDon.TongTien - hoaDon.GiamGia;

                if (hoaDon.ThanhToan < 0)
                    hoaDon.ThanhToan = 0;

                hoaDon.NgayLap = DateTime.Now;

                // Bước B: Tự sinh mã hóa đơn
                const string sqlMaHoaDon = @"
                    SELECT ISNULL(MAX(MaHoaDon), 0) + 1
                    FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK);";

                using SqlCommand cmdMaHoaDon =
                    new SqlCommand(sqlMaHoaDon, connection, transaction);

                int maHoaDon = Convert.ToInt32(
                    await cmdMaHoaDon.ExecuteScalarAsync());

                hoaDon.MaHoaDon = maHoaDon;

                // Bước C: Lưu thông tin hóa đơn
                const string sqlHoaDon = @"
                    INSERT INTO dbo.HoaDon
                    (
                        MaHoaDon,
                        MaKhachHang,
                        MaNhanVien,
                        NgayLap,
                        PhuongThucThanhToan,
                        TongTien,
                        GiamGia,
                        ThanhToan
                    )
                    VALUES
                    (
                        @MaHoaDon,
                        @MaKhachHang,
                        @MaNhanVien,
                        @NgayLap,
                        @PhuongThucThanhToan,
                        @TongTien,
                        @GiamGia,
                        @ThanhToan
                    );";

                using SqlCommand cmdHoaDon =
                    new SqlCommand(sqlHoaDon, connection, transaction);

                cmdHoaDon.Parameters.Add("@MaHoaDon", SqlDbType.Int)
                    .Value = hoaDon.MaHoaDon;

                cmdHoaDon.Parameters.Add("@MaKhachHang", SqlDbType.Int)
                    .Value = hoaDon.MaKhachHang;

                cmdHoaDon.Parameters.Add("@MaNhanVien", SqlDbType.Int)
                    .Value = hoaDon.MaNhanVien;

                cmdHoaDon.Parameters.Add("@NgayLap", SqlDbType.DateTime)
                    .Value = hoaDon.NgayLap;

                cmdHoaDon.Parameters.Add("@PhuongThucThanhToan",
                    SqlDbType.NVarChar, 50).Value =
                    hoaDon.PhuongThucThanhToan;

                cmdHoaDon.Parameters.Add("@TongTien", SqlDbType.Decimal)
                    .Value = hoaDon.TongTien;

                cmdHoaDon.Parameters.Add("@GiamGia", SqlDbType.Decimal)
                    .Value = hoaDon.GiamGia;

                cmdHoaDon.Parameters.Add("@ThanhToan", SqlDbType.Decimal)
                    .Value = hoaDon.ThanhToan;

                await cmdHoaDon.ExecuteNonQueryAsync();

                // Bước D: Tự sinh mã chi tiết và lưu từng sản phẩm
                const string sqlMaChiTiet = @"
                    SELECT ISNULL(MAX(MaChiTietHD), 0) + 1
                    FROM dbo.ChiTietHoaDon WITH (UPDLOCK, HOLDLOCK);";

                using SqlCommand cmdMaChiTiet =
                    new SqlCommand(sqlMaChiTiet, connection, transaction);

                int maChiTiet = Convert.ToInt32(
                    await cmdMaChiTiet.ExecuteScalarAsync());

                const string sqlChiTiet = @"
                    INSERT INTO dbo.ChiTietHoaDon
                    (
                        MaChiTietHD,
                        MaHoaDon,
                        MaSanPham,
                        SoLuong,
                        DonGiaBan,
                        ThanhTien
                    )
                    VALUES
                    (
                        @MaChiTietHD,
                        @MaHoaDon,
                        @MaSanPham,
                        @SoLuong,
                        @DonGiaBan,
                        @ThanhTien
                    );";

                foreach (var item in chiTietList)
                {
                    item.MaChiTietHD = maChiTiet;
                    item.MaHoaDon = maHoaDon;

                    using SqlCommand cmdChiTiet =
                        new SqlCommand(sqlChiTiet, connection, transaction);

                    cmdChiTiet.Parameters.Add("@MaChiTietHD", SqlDbType.Int)
                        .Value = item.MaChiTietHD;

                    cmdChiTiet.Parameters.Add("@MaHoaDon", SqlDbType.Int)
                        .Value = item.MaHoaDon;

                    cmdChiTiet.Parameters.Add("@MaSanPham", SqlDbType.Int)
                        .Value = item.MaSanPham;

                    cmdChiTiet.Parameters.Add("@SoLuong", SqlDbType.Int)
                        .Value = item.SoLuong;

                    var donGia = cmdChiTiet.Parameters.Add(
                        "@DonGiaBan", SqlDbType.Decimal);
                    donGia.Precision = 18;
                    donGia.Scale = 2;
                    donGia.Value = item.DonGiaBan;

                    var thanhTien = cmdChiTiet.Parameters.Add(
                        "@ThanhTien", SqlDbType.Decimal);
                    thanhTien.Precision = 18;
                    thanhTien.Scale = 2;
                    thanhTien.Value = item.ThanhTien;

                    await cmdChiTiet.ExecuteNonQueryAsync();

                    maChiTiet++;
                }

                // Bước E: Hoàn tất transaction
                await transaction.CommitAsync();

                hoaDon.DanhSachChiTiet =
                    new List<ChiTietHoaDon>(chiTietList);

                return (
                    true,
                    "Lập hóa đơn và lưu dữ liệu thành công!",
                    hoaDon
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                System.Diagnostics.Debug.WriteLine(
                    "Lỗi tạo hóa đơn: " + ex.Message);

                return (
                    false,
                    "Không thể lập hóa đơn: " + ex.Message,
                    null
                );
            }
        }
    }
}
