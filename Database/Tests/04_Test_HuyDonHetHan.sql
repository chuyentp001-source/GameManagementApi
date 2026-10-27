USE [GameManagementDB_Test];
GO
SET NOCOUNT ON;
SET XACT_ABORT OFF;

IF DB_NAME() <> N'GameManagementDB_Test'
    THROW 51399, N'Chi duoc chay file nay tren GameManagementDB_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);
DECLARE @AccountId INT, @DonHangId BIGINT;
DECLARE @SoTien DECIMAL(18,2) = 10000;
DECLARE @SoDonDaHuy INT, @TrangThaiThanhToan VARCHAR(30), @TrangThaiDonHang VARCHAR(30);

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
    VALUES (N't04_u_' + @Tag, 'test-only', 0, 'USER', 'HOAT_DONG');
    SET @AccountId = CONVERT(INT, SCOPE_IDENTITY());
    INSERT dbo.[User]
        (AccountID, HoTen, Email, TenNganHang, SoTaiKhoanNganHang, TenChuTaiKhoanNganHang)
    VALUES
        (@AccountId, N'Test ' + @Tag, @Tag + N'@test.local', N'Vietcombank', '1234567890', N'TEST USER');

    DECLARE @DonHang TABLE
    (
        DonHangId BIGINT, LoaiGiaoDich VARCHAR(20), AccountId INT, GoiNapId INT NULL,
        ChietKhauId INT NULL, TaiKhoanGame VARCHAR(200) NULL, Server VARCHAR(100) NULL,
        TongTienThanhToan DECIMAL(18,2), PhuongThucThanhToan VARCHAR(30),
        MaGiaoDichBank VARCHAR(100) NULL, TrangThaiThanhToan VARCHAR(30),
        TrangThaiDonHang VARCHAR(30), HanThanhToan DATETIME2 NULL,
        NguoiTaoId INT NULL, NguoiCapNhatId INT NULL, NgayTao DATETIME2 NULL, NgayCapNhat DATETIME2 NULL
    );
    INSERT INTO @DonHang
    EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = @AccountId, @SoTien = @SoTien;
    SELECT TOP (1) @DonHangId = DonHangId FROM @DonHang;
    IF @DonHangId IS NULL THROW 51300, N'Khong tao duoc don test.', 1;

    UPDATE dbo.DonHang SET HanThanhToan = DATEADD(MINUTE, -1, SYSDATETIME())
    WHERE DonHangId = @DonHangId AND TrangThaiThanhToan = 'CHO_THANH_TOAN';
    IF @@ROWCOUNT <> 1 THROW 51301, N'Khong the dat don test thanh het han.', 1;

    EXEC dbo.sp_DonHang_HuyGiaoDichHetHan;

    SELECT @TrangThaiThanhToan = TrangThaiThanhToan, @TrangThaiDonHang = TrangThaiDonHang
    FROM dbo.DonHang WHERE DonHangId = @DonHangId;
    IF @TrangThaiThanhToan <> 'THAT_BAI' OR @TrangThaiDonHang <> 'HUY'
        THROW 51302, N'Don het han chua duoc huy dung.', 1;

    SELECT @SoDonDaHuy = COUNT(*) FROM dbo.DonHang
    WHERE DonHangId = @DonHangId AND TrangThaiThanhToan = 'THAT_BAI' AND TrangThaiDonHang = 'HUY';
    SELECT N'PASS - Huy don het han' AS KetQua, @DonHangId AS DonHangTest,
           @SoDonDaHuy AS SoDonTestDaHuy, @TrangThaiThanhToan AS TrangThaiThanhToan,
           @TrangThaiDonHang AS TrangThaiDonHang;

    ROLLBACK TRANSACTION;
    PRINT N'Du lieu test da rollback.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
