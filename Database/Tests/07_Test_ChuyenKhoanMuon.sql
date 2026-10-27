SET NOCOUNT ON;
IF DB_NAME() NOT LIKE N'%[_]Test' THROW 51999, N'Chỉ chạy test trên database *_Test.', 1;

DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);

INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai)
VALUES (N'u_' + @Tag, 'x', 0, 'USER', 'HOAT_DONG');

DECLARE @Acc INT = SCOPE_IDENTITY();

INSERT dbo.[User]
(
    AccountID,
    HoTen,
    Email,
    TenNganHang,
    SoTaiKhoanNganHang,
    TenChuTaiKhoanNganHang
)
VALUES
(
    @Acc,
    N'Test ' + @Tag,
    @Tag + N'@test.local',
    N'Vietcombank',
    '1234567890',
    N'TEST USER'
);
INSERT dbo.Account (TenDangNhap, MatKhau, Role, TrangThai) VALUES (N'a_' + @Tag, 'x', 'ADMIN', 'HOAT_DONG');
DECLARE @Admin INT = SCOPE_IDENTITY();

-- Arrange: yêu cầu nạp, cho hết hạn, job hủy
EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = @Acc, @SoTien = 10000;
DECLARE @Don BIGINT = (SELECT TOP 1 DonHangId FROM dbo.DonHang WHERE AccountId = @Acc ORDER BY DonHangId DESC);
UPDATE dbo.DonHang SET HanThanhToan = DATEADD(MINUTE, -1, SYSDATETIME()) WHERE DonHangId = @Don;
EXEC dbo.sp_DonHang_HuyGiaoDichHetHan;

IF NOT EXISTS (SELECT 1 FROM dbo.DonHang
               WHERE DonHangId = @Don AND TrangThaiThanhToan = 'THAT_BAI' AND TrangThaiDonHang = 'HUY')
    THROW 51700, N'FAIL 07: đơn hết hạn phải bị hủy trước.', 1;

-- Act: ghi nhận chuyển khoản muộn
EXEC dbo.sp_DonHang_XacNhanChuyenKhoanMuon @DonHangId = @Don, @NguoiXacNhanId = @Admin, @GhiChu = N'test';

IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @Acc) <> 10000
    THROW 51701, N'FAIL 07: ví phải được cộng 10000.', 1;
IF (SELECT COUNT(*) FROM dbo.LichSuVi WHERE DonHangId = @Don AND LoaiGiaoDich = 'NAP_TIEN_VI' AND TrangThai = 'THANH_CONG') <> 1
    THROW 51702, N'FAIL 07: phải có đúng 1 dòng NAP_TIEN_VI.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.DonHang
               WHERE DonHangId = @Don AND TrangThaiThanhToan = 'THANH_CONG' AND TrangThaiDonHang = 'HOAN_THANH')
    THROW 51703, N'FAIL 07: đơn nạp tiền muộn phải HOAN_THANH.', 1;

-- Gọi lần hai: bị chặn, ví không đổi
DECLARE @Err INT = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_XacNhanChuyenKhoanMuon @DonHangId = @Don, @NguoiXacNhanId = @Admin;
END TRY
BEGIN CATCH
    SET @Err = ERROR_NUMBER();
END CATCH;

IF @Err IS NULL OR @Err <> 50802 THROW 51704, N'FAIL 07: lần hai phải báo 50802.', 1;
IF (SELECT SoDu FROM dbo.Account WHERE AccountId = @Acc) <> 10000
    THROW 51705, N'FAIL 07: ví không được cộng lần hai.', 1;

PRINT N'PASS 07_ChuyenKhoanMuon';