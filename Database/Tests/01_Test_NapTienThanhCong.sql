USE [GameManagementDB_Test];
GO
SET NOCOUNT ON;
SET XACT_ABORT OFF;

-- Chi chay tren DB Test. Tao tai khoan va du lieu rieng, khong phu thuoc ID co san.
IF DB_NAME() <> N'GameManagementDB_Test'
    THROW 51099, N'Chi duoc chay file nay tren GameManagementDB_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);
DECLARE @AccountId INT, @AdminId INT, @DonHangId BIGINT;
DECLARE @SoTien DECIMAL(18,2) = 10000;
DECLARE @SoDuTruoc DECIMAL(18,2), @SoDuSau DECIMAL(18,2), @SoDongLichSu INT;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
    VALUES (N't01_u_' + @Tag, 'test-only', 0, 'USER', 'HOAT_DONG');
    SET @AccountId = CONVERT(INT, SCOPE_IDENTITY());

    INSERT dbo.[User]
        (AccountID, HoTen, Email, TenNganHang, SoTaiKhoanNganHang, TenChuTaiKhoanNganHang)
    VALUES
        (@AccountId, N'Test ' + @Tag, @Tag + N'@test.local', N'Vietcombank', '1234567890', N'TEST USER');

    INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
    VALUES (N't01_a_' + @Tag, 'test-only', 0, 'ADMIN', 'HOAT_DONG');
    SET @AdminId = CONVERT(INT, SCOPE_IDENTITY());

    SELECT @SoDuTruoc = SoDu FROM dbo.Account WHERE AccountId = @AccountId;

    -- Tao yeu cau nap tien va lay DonHangId tu result set cua procedure.
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
    IF @DonHangId IS NULL THROW 51003, N'Khong tao duoc don nap tien test.', 1;

    EXEC dbo.sp_DonHang_XacNhanNapTien @DonHangId = @DonHangId, @NguoiXacNhanId = @AdminId;

    SELECT @SoDuSau = SoDu FROM dbo.Account WHERE AccountId = @AccountId;
    IF @SoDuSau <> @SoDuTruoc + @SoTien
        THROW 51004, N'So du khong tang dung so tien nap.', 1;

    SELECT @SoDongLichSu = COUNT(*) FROM dbo.LichSuVi
    WHERE DonHangId = @DonHangId AND LoaiGiaoDich = 'NAP_TIEN_VI' AND TrangThai = 'THANH_CONG';
    IF @SoDongLichSu <> 1 THROW 51005, N'So ban ghi lich su vi khong dung.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.DonHang WHERE DonHangId = @DonHangId
                   AND TrangThaiThanhToan = 'THANH_CONG' AND TrangThaiDonHang = 'HOAN_THANH')
        THROW 51006, N'Trang thai don nap tien khong dung.', 1;

    SELECT N'PASS - Luong nap tien thanh cong' AS KetQua,
           @DonHangId AS DonHangTest, @SoDuTruoc AS SoDuTruoc,
           @SoDuSau AS SoDuSau, @SoDongLichSu AS SoDongLichSu;
    ROLLBACK TRANSACTION;
    PRINT N'Du lieu test da rollback.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
