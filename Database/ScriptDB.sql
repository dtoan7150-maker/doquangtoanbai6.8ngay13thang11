CREATE DATABASE BilliardsDB;
GO
USE BilliardsDB;
GO

-- 1. Bảng Danh sách bàn chơi
CREATE TABLE BanBida (
    MaBan INT PRIMARY KEY IDENTITY(1,1),
    TenBan NVARCHAR(50) NOT NULL,
    LoaiBan NVARCHAR(30),
    GiaTheoGio DECIMAL(18,2),
    TrangThai NVARCHAR(20) DEFAULT N'Trống'
);

-- 2. Bảng Menu đồ ăn, nước uống, thuê gậy
CREATE TABLE MenuDichVu (
    MaDV INT PRIMARY KEY IDENTITY(1,1),
    TenDV NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2),
    TonKho INT -- Quy ước: TonKho = -1 là dịch vụ thuê gậy (không trừ kho)
);

-- 3. Bảng Hóa đơn tính tiền
CREATE TABLE HoaDon (
    MaHD INT PRIMARY KEY IDENTITY(1,1),
    MaBan INT FOREIGN KEY REFERENCES BanBida(MaBan),
    GioBatDau DATETIME NOT NULL,
    GioKetThuc DATETIME NULL,
    TienGioChoi DECIMAL(18,2) DEFAULT 0,
    TienDichVu DECIMAL(18,2) DEFAULT 0,
    TrangThai BIT DEFAULT 0 -- 0: Chưa thanh toán, 1: Đã thanh toán
);

-- 4. Bảng Chi tiết dịch vụ gọi thêm
CREATE TABLE ChiTietHoaDon (
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaDV INT FOREIGN KEY REFERENCES MenuDichVu(MaDV),
    SoLuong INT CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaHD, MaDV)
);
GO

-- Chèn dữ liệu mẫu
INSERT INTO BanBida (TenBan, LoaiBan, GiaTheoGio, TrangThai) VALUES
(N'Bàn Lỗ 01', N'Pool (Lỗ)', 60000, N'Đang chơi'),
(N'Bàn Lỗ 02', N'Pool (Lỗ)', 60000, N'Trống'),
(N'Bàn Lỗ 03', N'Pool (Lỗ)', 65000, N'Trống'),
(N'Bàn Carom 01', N'Carom (Phăng)', 70000, N'Đang chơi'),
(N'Bàn Carom 02', N'Carom (Phăng)', 70000, N'Trống'),
(N'Bàn Snooker 01', N'Snooker', 90000, N'Trống');

INSERT INTO MenuDichVu (TenDV, DonGia, TonKho) VALUES
(N'Nước tăng lực Sting', 15000, 50),
(N'Nước suối Aquafina', 10000, 60),
(N'Mì xào bò', 35000, 20),
(N'Khăn lạnh', 5000, 100),
(N'Thuê gậy Carbon', 30000, -1); -- Dịch vụ thuê gậy không trừ kho

-- Tạo sẵn 2 hóa đơn đang chơi để kiểm thử ngay
INSERT INTO HoaDon (MaBan, GioBatDau, GioKetThuc, TienGioChoi, TienDichVu, TrangThai) VALUES
(1, DATEADD(MINUTE, -75, GETDATE()), NULL, 0, 0, 0),
(4, DATEADD(MINUTE, -40, GETDATE()), NULL, 0, 0, 0);

INSERT INTO ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) VALUES
(1, 1, 2, 15000),
(1, 5, 1, 30000),
(2, 2, 2, 10000);
GO