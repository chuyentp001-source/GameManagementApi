USE [GameManagementDB_Test];
GO
SET NOCOUNT ON;

IF DB_NAME() <> N'GameManagementDB_Test'
    THROW 51299, N'Chi duoc chay file nay tren GameManagementDB_Test.', 1;

DECLARE @KetQua TABLE
(
    TruongHop NVARCHAR(100),
    MaLoiMongDoi INT,
    MaLoiThucTe INT,
    KetQua NVARCHAR(10)
);
DECLARE @MaLoi INT;

-- AccountId khong ton tai de chung minh validate so tien duoc thuc hien truoc validate tai khoan.
IF EXISTS (SELECT 1 FROM dbo.Account WHERE AccountId = 2147483647)
    THROW 51210, N'AccountId danh cho test da ton tai; hay chon ID khac.', 1;

SET @MaLoi = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = 2147483647, @SoTien = 5000;
END TRY
BEGIN CATCH
    SET @MaLoi = ERROR_NUMBER();
END CATCH;
INSERT INTO @KetQua VALUES (N'Duoi muc toi thieu', 50401, @MaLoi,
    IIF(@MaLoi = 50401, N'PASS', N'FAIL'));

SET @MaLoi = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = 2147483647, @SoTien = 1010000;
END TRY
BEGIN CATCH
    SET @MaLoi = ERROR_NUMBER();
END CATCH;
INSERT INTO @KetQua VALUES (N'Vuot muc toi da', 50403, @MaLoi,
    IIF(@MaLoi = 50403, N'PASS', N'FAIL'));

SET @MaLoi = NULL;
BEGIN TRY
    EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = 2147483647, @SoTien = 15000;
END TRY
BEGIN CATCH
    SET @MaLoi = ERROR_NUMBER();
END CATCH;
INSERT INTO @KetQua VALUES (N'Khong phai boi so 10.000', 50404, @MaLoi,
    IIF(@MaLoi = 50404, N'PASS', N'FAIL'));

SELECT * FROM @KetQua;
IF EXISTS (SELECT 1 FROM @KetQua WHERE KetQua = N'FAIL')
    THROW 51211, N'Co test dau vao khong hop le bi that bai.', 1;
PRINT N'PASS 03_Test_NapTien_DauVao';
GO