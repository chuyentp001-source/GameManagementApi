SET NOCOUNT ON;
IF DB_NAME() NOT LIKE N'%[_]Test' THROW 51999, N'Chỉ chạy test trên database *_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);

INSERT dbo.Game (TenGame, TrangThaiGame, CoServer) VALUES (N'G_' + @Tag, 'HOAT_DONG', 0);
INSERT dbo.GoiNap (GameID, TenGoiNap, GiaGoc, TrangThai) VALUES (SCOPE_IDENTITY(), N'Goi test', 30000, 'HOAT_DONG');
DECLARE @Goi INT = SCOPE_IDENTITY();

-- Tài khoản đủ tiền
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai) VALUES (N'ok_' + @Tag, 'x', 100000, 'USER', 'HOAT_DONG');
DECLARE @AccOk INT = SCOPE_IDENTITY();
INSERT dbo.[User] (AccountID, HoTen, Email) VALUES (@AccOk, N'Ok ' + @Tag, N'ok' + @Tag + N'@test.local');

-- Tài khoản thiếu tiền
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai) VALUES (N'low_' + @Tag, 'x', 10000, 'USER', 'HOAT_DONG');
DECLARE @AccLow INT = SCOPE_IDENTITY();
INSERT dbo.[User] (AccountID, HoTen, Email) VALUES (@AccLow, N'Low ' + @Tag, N'low' + @Tag + N'@test.local');

-- Case 1: đủ tiền
EXEC dbo.sp_DonHang_Create @AccountId = @AccOk, @GoiNapId = @Goi,
     @TaiKhoanGame = 'p1', @PhuongThucThanhToan = 'VI';
DECLARE @Don BIGINT = (SELECT TOP 1 DonHangId FROM dbo.DonHang WHERE AccountId = @AccOk ORDER BY DonHangId DESC);

IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @AccOk) <> 70000
    THROW 51600, N'FAIL 06.1: số dư phải là 70000.', 1;
IF (SELECT COUNT(*) FROM dbo.LichSuVi
    WHERE DonHangId = @Don AND LoaiGiaoDich = 'THANH_TOAN_DON' AND SoTien = 30000 AND TrangThai = 'THANH_CONG') <> 1
    THROW 51601, N'FAIL 06.1: phải có đúng 1 dòng THANH_TOAN_DON 30000.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.DonHang
               WHERE DonHangId = @Don AND TrangThaiThanhToan = 'THANH_CONG' AND TrangThaiDonHang = 'CHO_XU_LY')
    THROW 51602, N'FAIL 06.1: trạng thái đơn sai.', 1;

-- Case 2: thiếu tiền -> 50106, không trừ tiền, không có đơn
DECLARE @Err INT = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_Create @AccountId = @AccLow, @GoiNapId = @Goi,
         @TaiKhoanGame = 'p1', @PhuongThucThanhToan = 'VI';
END TRY
BEGIN CATCH
    SET @Err = ERROR_NUMBER();
END CATCH;

IF @Err IS NULL OR @Err <> 50106 THROW 51603, N'FAIL 06.2: thiếu tiền phải báo 50106.', 1;
IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @AccLow) <> 10000
    THROW 51604, N'FAIL 06.2: số dư không được đổi.', 1;
IF EXISTS (SELECT 1 FROM dbo.DonHang WHERE AccountId = @AccLow)
    THROW 51605, N'FAIL 06.2: không được tạo đơn khi thiếu tiền.', 1;

PRINT N'PASS 06_ThanhToanVi';