SET NOCOUNT ON;
IF DB_NAME() NOT LIKE N'%[_]Test' THROW 51999, N'Chỉ chạy test trên database *_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);

INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai) VALUES (N'u_' + @Tag, 'x', 100000, 'USER', 'HOAT_DONG');
DECLARE @Acc INT = SCOPE_IDENTITY();
INSERT dbo.[User] (AccountID, HoTen, Email) VALUES (@Acc, N'Test ' + @Tag, @Tag + N'@test.local');
INSERT dbo.Account (TenDangNhap, MatKhau, Role, TrangThai) VALUES (N'a_' + @Tag, 'x', 'ADMIN', 'HOAT_DONG');
DECLARE @Admin INT = SCOPE_IDENTITY();
INSERT dbo.Game (TenGame, TrangThaiGame, CoServer) VALUES (N'G_' + @Tag, 'HOAT_DONG', 0);
INSERT dbo.GoiNap (GameID, TenGoiNap, GiaGoc, TrangThai) VALUES (SCOPE_IDENTITY(), N'Goi test', 30000, 'HOAT_DONG');
DECLARE @Goi INT = SCOPE_IDENTITY();

-- Arrange: mua gói bằng ví, rồi đánh dấu nạp game thất bại
EXEC dbo.sp_DonHang_Create @AccountId = @Acc, @GoiNapId = @Goi,
     @TaiKhoanGame = 'p1', @PhuongThucThanhToan = 'VI';
DECLARE @Don BIGINT = (SELECT TOP 1 DonHangId FROM dbo.DonHang WHERE AccountId = @Acc ORDER BY DonHangId DESC);
UPDATE dbo.DonHang SET TrangThaiDonHang = 'NAP_GAME_THAT_BAI' WHERE DonHangId = @Don;

IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @Acc) <> 70000
    THROW 51500, N'FAIL 05: sau khi mua, số dư phải là 70000.', 1;

-- Hoàn lần 1: thành công
EXEC dbo.sp_DonHang_HoanTien @DonHangId = @Don, @NguoiCapNhatId = @Admin;
IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @Acc) <> 100000
    THROW 51501, N'FAIL 05: sau hoàn lần 1, số dư phải về 100000.', 1;

-- Hoàn lần 2: bị chặn
DECLARE @Err INT = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_HoanTien @DonHangId = @Don, @NguoiCapNhatId = @Admin;
END TRY
BEGIN CATCH
    SET @Err = ERROR_NUMBER();
END CATCH;

IF @Err IS NULL OR @Err <> 50302
    THROW 51502, N'FAIL 05: hoàn lần hai phải báo 50302.', 1;
    --SELECT @Err AS MaLoiHoanTienLan2;
IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @Acc) <> 100000
    THROW 51503, N'FAIL 05: số dư không được đổi sau hoàn lần hai.', 1;
IF (SELECT COUNT(*) FROM dbo.LichSuVi WHERE DonHangId = @Don) <> 2
    THROW 51504, N'FAIL 05: lịch sử ví phải có đúng 2 dòng (thanh toán + hoàn).', 1;

PRINT N'PASS 05_HoanTienLap';