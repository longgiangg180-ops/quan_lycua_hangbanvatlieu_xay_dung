create database Vat_lieu_xay_dung
go
use Vat_lieu_xay_dung
go
CREATE TABLE KhachHang
(
    MaKhachHang INT,
    TenKhachHang NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    CONSTRAINT PK_KhachHang
    PRIMARY KEY (MaKhachHang)
);
GO
CREATE TABLE NhanVien
(
    MaNhanVien INT,
    HoTen NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    NgaySinh DATE,
    ChucVu NVARCHAR(50),
    CONSTRAINT PK_NhanVien
    PRIMARY KEY (MaNhanVien)
);
GO
CREATE TABLE NhaCungCap
(
    MaNCC INT,
    TenNCC NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    Email VARCHAR(100),
    CONSTRAINT PK_NhaCungCap
    PRIMARY KEY (MaNCC)
);
GO
CREATE TABLE SanPham
(
    MaSanPham INT,
    TenSanPham NVARCHAR(100),
    DanhMuc NVARCHAR(100),
    DonViTinh NVARCHAR(50),
    GiaBan DECIMAL(18,2),
    MoTa NVARCHAR(500),
    CONSTRAINT PK_SanPham
    PRIMARY KEY (MaSanPham)
);
GO
CREATE TABLE HoaDon
(
    MaHoaDon INT,
    MaKhachHang INT,
    MaNhanVien INT,
    NgayLap DATETIME,
    PhuongThucThanhToan NVARCHAR(50),
    TongTien DECIMAL(18,2),
    GiamGia DECIMAL(18,2),
    ThanhToan DECIMAL(18,2),
    CONSTRAINT PK_HoaDon
    PRIMARY KEY (MaHoaDon),
    CONSTRAINT FK_HoaDon_KhachHang
    FOREIGN KEY (MaKhachHang)
    REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT FK_HoaDon_NhanVien
    FOREIGN KEY (MaNhanVien)
    REFERENCES NhanVien(MaNhanVien)
);
GO
CREATE TABLE PhieuNhap
(
    MaPhieuNhap INT,
    MaNCC INT,
    MaNhanVien INT,
    NgayNhap DATETIME,
    TongTien DECIMAL(18,2),
    CONSTRAINT PK_PhieuNhap
    PRIMARY KEY (MaPhieuNhap),
    CONSTRAINT FK_PhieuNhap_NhaCungCap
    FOREIGN KEY (MaNCC)
    REFERENCES NhaCungCap(MaNCC),
    CONSTRAINT FK_PhieuNhap_NhanVien
    FOREIGN KEY (MaNhanVien)
    REFERENCES NhanVien(MaNhanVien)
);
GO
CREATE TABLE ChiTietHoaDon
(
    MaChiTietHD INT,
    MaHoaDon INT,
    MaSanPham INT,
    SoLuong INT,
    DonGiaBan DECIMAL(18,2),
    ThanhTien DECIMAL(18,2),
    CONSTRAINT PK_ChiTietHoaDon
    PRIMARY KEY (MaChiTietHD),
    CONSTRAINT FK_ChiTietHoaDon_HoaDon
    FOREIGN KEY (MaHoaDon)
    REFERENCES HoaDon(MaHoaDon),
    CONSTRAINT FK_ChiTietHoaDon_SanPham
    FOREIGN KEY (MaSanPham)
    REFERENCES SanPham(MaSanPham)
);
GO
CREATE TABLE ChiTietPhieuNhap
(
    MaChiTietPN INT,
    MaPhieuNhap INT,
    MaSanPham INT,
    SoLuong INT,
    DonGiaNhap DECIMAL(18,2),
    ThanhTien DECIMAL(18,2),
    CONSTRAINT PK_ChiTietPhieuNhap
    PRIMARY KEY (MaChiTietPN),
    CONSTRAINT FK_ChiTietPhieuNhap_PhieuNhap
    FOREIGN KEY (MaPhieuNhap)
    REFERENCES PhieuNhap(MaPhieuNhap),
    CONSTRAINT FK_ChiTietPhieuNhap_SanPham
    FOREIGN KEY (MaSanPham)
    REFERENCES SanPham(MaSanPham)
);
GO
INSERT INTO KhachHang
(MaKhachHang, TenKhachHang, SoDienThoai, DiaChi)
VALUES
(1, N'Nguyễn Việt Hưng', '0354651535', N'Hà Nội'),
(2, N'Lê Hoàng Hiệp', '0942183541', N'Hà Nội'),
(3, N'Lê Hoàng Long Giang', '0853285123', N'Hà Tĩnh'),
(4, N'Trần Bá Hữu', '0895813575', N'Bắc Ninh'),
(5, N'Nguyễn Hồng Sơn', '0835186515', N'Thái Bình');
INSERT INTO NhanVien
(MaNhanVien, HoTen, SoDienThoai, DiaChi, NgaySinh, ChucVu)
VALUES
(1, N'Trần Văn Thuận', '0354815645', N'Hưng Yên', '2006-05-26', N'Quản Lý'),
(2, N'Nguyễn Đình Trung Hiếu', '0685135516', N'Hà Nội', '2006-12-31', N'Nhân Viên'),
(3, N'Phan Chí Thành', '0980542315', N'Phú Thọ', '2006-01-01', N'Phó Giám Đốc'),
(4, N'Trịnh Minh Hoàng', '0348165486', N'Thanh Hoá', '2006-07-24', N'Giám Đốc'),
(5, N'Trần Đức Mạnh', '0368651235', N'Hải Phòng', '2006-08-27', N'Nhân Viên');
INSERT INTO NhaCungCap
(MaNCC, TenNCC, SoDienThoai, DiaChi)
VALUES
(1, N'Công ty TNHH Vật liệu Xây dựng Minh Phát', '0912345678', N'Hải Phòng'),
(2, N'Công ty TNHH Thép Hòa Bình', '0987654321', N'Hà Nội'),
(3, N'Công ty CP Xi măng Việt Nam', '0354515465', N'Hải Dương'),
(4, N'Công ty TNHH Gạch ngói Thành Công', '0354651354', N'Bình Thuận'),
(5, N'Công ty TNHH Thiết bị Nước Đại Thành', '0954213545', N'Bắc Ninh');
INSERT INTO SanPham
(MaSanPham, TenSanPham, DanhMuc, DonViTinh, GiaBan, MoTa)
VALUES
(1, N'Kính cường lực 10mm', N'Thuỷ tinh', N'm2', 650000,
 N'Kính cường lực xây dựng, dễ vỡ khi va đập góc'),
(2, N'Bồn nước Inox 1000L Ngang', N'Bồn nước', N'Cái', 3200000,
 N'Bồn chứa nước thiết bị trong nhà và ngoài trời'),
(3, N'Gạch ống 4 lỗ 8x8x18', N'Gạch', N'Viên', 1200,
 N'Gạch đất sét nung tuynel dùng xây nhà'),
(4, N'Xi măng Portland hỗn hợp PCB40', N'Xi măng', N'Bao', 88000,
 N'Xi măng bao 50kg trộn cát vữa xây tô'),
(5, N'Thép thanh vằn D12 CB300-V', N'Sắt thép', N'Cây', 165000,
 N'Thép cốt bê tông chịu lực phi 12');
INSERT INTO HoaDon (MaHoaDon, MaKhachHang, MaNhanVien, NgayLap, PhuongThucThanhToan, TongTien, GiamGia, ThanhToan)VALUES
(1, 1, 1, '2026-04-15', N'QR', 3250000, 0, 3250000),
(2, 2, 2, '2026-04-16', N'Cash', 330000, 0, 330000),
(3, 3, 3, '2026-04-17', N'QR', 2640000, 0, 2640000),
(4, 4, 4, '2026-04-18', N'Cash', 150000, 0, 150000),
(5, 5, 5, '2026-04-19', N'QR', 3200000, 0, 3200000);
INSERT INTO PhieuNhap (MaPhieuNhap, MaNCC, MaNhanVien, NgayNhap, TongTien)VALUES
(1, 1, 1, '2026-04-15', 10400000),
(2, 2, 2, '2026-04-16', 13500000),
(3, 3, 3, '2026-04-17', 21600000),
(4, 4, 4, '2026-04-18', 4500000),
(5, 5, 5, '2026-04-19', 26000000);
INSERT INTO ChiTietHoaDon (MaChiTietHD, MaHoaDon, MaSanPham, SoLuong, DonGiaBan, ThanhTien)VALUES
(1, 1, 1, 5, 650000, 3250000),
(2, 2, 5, 2, 165000, 330000),
(3, 3, 4, 30, 88000, 2640000),
(4, 4, 3, 125, 1200, 150000),
(5, 5, 2, 1, 3200000, 3200000);
INSERT INTO ChiTietPhieuNhap (MaChiTietPN, MaPhieuNhap, MaSanPham, SoLuong, DonGiaNhap, ThanhTien)VALUES
(1, 1, 1, 20, 520000, 10400000),
(2, 2, 5, 100, 135000, 13500000),
(3, 3, 4, 300, 72000, 21600000),
(4, 4, 3, 5000, 900, 4500000),
(5, 5, 2, 10, 2600000, 26000000);
GO
SELECT * FROM KhachHang;
SELECT * FROM NhanVien;
SELECT * FROM NhaCungCap;
SELECT * FROM SanPham;
SELECT * FROM HoaDon;
SELECT * FROM PhieuNhap;
SELECT * FROM ChiTietHoaDon;
SELECT * FROM ChiTietPhieuNhap;