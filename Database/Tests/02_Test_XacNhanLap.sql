USE [GameManagementDB_Test];
GO
SET NOCOUNT ON;
SET XACT_ABORT OFF;

IF DB_NAME() <> N'GameManagementDB_Test'
    THROW 51199, N'Chi duoc chay file nay tren GameManagementDB_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);
DECLARE @AccountId INT, @AdminId INT, @DonHangId BIGINT;
DECLARE @SoTien DECIMAL(18,2) = 10000;
DECLARE @SoDuTruoc DECIMAL(18,2), @SoDuSauLan1 DECIMAL(18,2), @SoDuSauLan2 DECIMAL(18,2);
DECLARE @SoDongLichSu INT, @MaLoiLan2 INT = NULL;

-- Tao du lieu test rieng. Khong dung AccountId=1/2 co san.
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
VALUES (N't02_u_' + @Tag, 'test-only', 0, 'USER', 'HOAT_DONG');
SET @AccountId = CONVERT(INT, SCOPE_IDENTITY());
INSERT dbo.[User]
    (AccountID, HoTen, Email, TenNganHang, SoTaiKhoanNganHang, TenChuTaiKhoanNganHang)
VALUES
    (@AccountId, N'Test ' + @Tag, @Tag + N'@test.local', N'Vietcombank', '1234567890', N'TEST USER');
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
VALUES (N't02_a_' + @Tag, 'test-only', 0, 'ADMIN', 'HOAT_DONG');
SET @AdminId = CONVERT(INT, SCOPE_IDENTITY());
SELECT @SoDuTruoc = SoDu FROM dbo.Account WHERE AccountId = @AccountId;

BEGIN TRY
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
    IF @DonHangId IS NULL THROW 51102, N'Khong tao duoc don test.', 1;

    EXEC dbo.sp_DonHang_XacNhanNapTien @DonHangId = @DonHangId, @NguoiXacNhanId = @AdminId;
    SELECT @SoDuSauLan1 = SoDu FROM dbo.Account WHERE AccountId = @AccountId;
    IF @SoDuSauLan1 <> @SoDuTruoc + @SoTien THROW 51103, N'So du sau lan dau khong dung.', 1;

    BEGIN TRY
        EXEC dbo.sp_DonHang_XacNhanNapTien @DonHangId = @DonHangId, @NguoiXacNhanId = @AdminId;
    END TRY
    BEGIN CATCH
        SET @MaLoiLan2 = ERROR_NUMBER();
    END CATCH;

    IF @MaLoiLan2 <> 50413 THROW 51104, N'Lan xac nhan thu hai khong tra ve loi 50413.', 1;
    SELECT @SoDuSauLan2 = SoDu FROM dbo.Account WHERE AccountId = @AccountId;
    IF @SoDuSauLan2 <> @SoDuSauLan1 THROW 51105, N'So du da thay doi khi xac nhan lap.', 1;

    SELECT @SoDongLichSu = COUNT(*) FROM dbo.LichSuVi
    WHERE DonHangId = @DonHangId AND LoaiGiaoDich = 'NAP_TIEN_VI' AND TrangThai = 'THANH_CONG';
    IF @SoDongLichSu <> 1 THROW 51106, N'Lich su vi bi ghi trung hoac bi thieu.', 1;

    SELECT N'PASS - Chan xac nhan nap tien lap' AS KetQua, @DonHangId AS DonHangTest,
           @MaLoiLan2 AS MaLoiLan2, @SoDuTruoc AS SoDuBanDau,
           @SoDuSauLan1 AS SoDuSauLan1, @SoDuSauLan2 AS SoDuSauLan2,
           @SoDongLichSu AS SoBanGhiLichSu;
END TRY
BEGIN CATCH
    -- Don va lich su lien quan duoc don rieng; tai khoan test cung duoc xoa neu khong con FK phu thuoc.
    BEGIN TRY
        IF @DonHangId IS NOT NULL
        BEGIN
            DELETE FROM dbo.LichSuVi WHERE DonHangId = @DonHangId;
            DELETE FROM dbo.DonHang WHERE DonHangId = @DonHangId;
        END;
        IF @AccountId IS NOT NULL
        BEGIN
            DELETE FROM dbo.[User] WHERE AccountID = @AccountId;
            DELETE FROM dbo.Account WHERE AccountId = @AccountId;
        END;
        IF @AdminId IS NOT NULL DELETE FROM dbo.Account WHERE AccountId = @AdminId;
    END TRY
    BEGIN CATCH
        PRINT N'Canh bao: khong don duoc mot phan du lieu test; hay kiem tra cac ID test moi tao.';
    END CATCH;
    THROW;
END CATCH;

-- Don dep sau khi PASS
DELETE FROM dbo.LichSuVi WHERE DonHangId = @DonHangId;
DELETE FROM dbo.DonHang WHERE DonHangId = @DonHangId;
DELETE FROM dbo.[User] WHERE AccountID = @AccountId;
DELETE FROM dbo.Account WHERE AccountId IN (@AccountId, @AdminId);
PRINT N'Du lieu test da duoc don dep.';
GO
