param(
    [string]$Server = "(localdb)\MSSQLLocalDB",
    [string]$Db = "GameManagementDB_Test"
)

function Run-Sql([string]$Sql) { sqlcmd -S $Server -d $Db -E -b -I -h -1 -W -Q $Sql }
function Fail([string]$m) { Write-Host "FAIL  $m" -ForegroundColor Red; exit 1 }

if ($Db -notlike "*_Test") { Fail "Chỉ chạy trên database *_Test." }

# Chạy cùng một câu lệnh ở 2 tiến trình, bắt đầu cùng một thời điểm
function Invoke-Parallel([string]$Body, [int]$Count = 2) {
    $when = (Get-Date).AddSeconds(6).ToString("HH:mm:ss")
    $sql = "WAITFOR TIME '$when'; BEGIN TRY $Body PRINT 'RESULT:OK'; END TRY BEGIN CATCH PRINT 'RESULT:ERR:' + CAST(ERROR_NUMBER() AS VARCHAR(10)); END CATCH"
    $jobs = 1..$Count | ForEach-Object {
        Start-Job -ArgumentList $Server, $Db, $sql -ScriptBlock {
            param($s, $d, $q) sqlcmd -S $s -d $d -E -b -I -h -1 -W -Q $q
        }
    }
    $jobs | Wait-Job | Out-Null
    $out = ($jobs | ForEach-Object { (Receive-Job $_) -join "`n" }) -join "`n"
    $jobs | Remove-Job
    return $out
}

function Count-Match([string]$text, [string]$pattern) { ([regex]::Matches($text, $pattern)).Count }

# ---- Test A: hai admin xác nhận cùng một đơn nạp tiền ----
$ids = (Run-Sql @"
SET NOCOUNT ON;
DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai) VALUES (N'u_'+@Tag, 'x', 0, 'USER', 'HOAT_DONG');
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
INSERT dbo.Account (TenDangNhap, MatKhau, Role, TrangThai) VALUES (N'a_'+@Tag, 'x', 'ADMIN', 'HOAT_DONG');
DECLARE @Admin INT = SCOPE_IDENTITY();
EXEC dbo.sp_DonHang_TaoYeuCauNapTien @AccountId = @Acc, @SoTien = 10000;
DECLARE @Don BIGINT = (SELECT TOP 1 DonHangId FROM dbo.DonHang WHERE AccountId = @Acc ORDER BY DonHangId DESC);
PRINT 'IDS:' + CAST(@Acc AS VARCHAR(20)) + ',' + CAST(@Admin AS VARCHAR(20)) + ',' + CAST(@Don AS VARCHAR(20));
"@) -join "`n"
$m = [regex]::Match($ids, 'IDS:(\d+),(\d+),(\d+)')
if (-not $m.Success) { Fail "08A: không tạo được dữ liệu. Output: $ids" }
$acc, $admin, $don = $m.Groups[1].Value, $m.Groups[2].Value, $m.Groups[3].Value

$out = Invoke-Parallel "EXEC dbo.sp_DonHang_XacNhanNapTien @DonHangId = $don, @NguoiXacNhanId = $admin;"
$ok = Count-Match $out 'RESULT:OK'
$rej = Count-Match $out 'RESULT:ERR:50413'
$bal = [decimal]((Run-Sql "SET NOCOUNT ON; SELECT SoDu FROM dbo.Account WHERE AccountId = $acc") | Select-Object -First 1)
$led = [int]((Run-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.LichSuVi WHERE DonHangId = $don AND LoaiGiaoDich = 'NAP_TIEN_VI'") | Select-Object -First 1)
if ($ok -ne 1 -or $rej -ne 1) { Fail "08A: cần đúng 1 OK và 1 lỗi 50413 (OK=$ok, 50413=$rej).`n$out" }
if ($bal -ne 10000) { Fail "08A: ví phải là 10000, thực tế $bal." }
if ($led -ne 1) { Fail "08A: phải có đúng 1 dòng lịch sử, thực tế $led." }
Write-Host "PASS  08A xác nhận nạp tiền song song" -ForegroundColor Green

# ---- Test B: hai yêu cầu mua gói bằng ví, số dư chỉ đủ cho một ----
$ids = (Run-Sql @"
SET NOCOUNT ON;
DECLARE @Tag NVARCHAR(10) = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 10);
INSERT dbo.Account (TenDangNhap, MatKhau, SoDu, Role, TrangThai) VALUES (N'u_'+@Tag, 'x', 50000, 'USER', 'HOAT_DONG');
DECLARE @Acc INT = SCOPE_IDENTITY();
INSERT dbo.[User] (AccountID, HoTen, Email) VALUES (@Acc, N'Test '+@Tag, @Tag+N'@test.local');
INSERT dbo.Game (TenGame, TrangThaiGame, CoServer) VALUES (N'G_'+@Tag, 'HOAT_DONG', 0);
INSERT dbo.GoiNap (GameID, TenGoiNap, GiaGoc, TrangThai) VALUES (SCOPE_IDENTITY(), N'Goi', 40000, 'HOAT_DONG');
DECLARE @Goi INT = SCOPE_IDENTITY();
PRINT 'IDS:' + CAST(@Acc AS VARCHAR(20)) + ',' + CAST(@Goi AS VARCHAR(20));
"@) -join "`n"
$m = [regex]::Match($ids, 'IDS:(\d+),(\d+)')
if (-not $m.Success) { Fail "08B: không tạo được dữ liệu. Output: $ids" }
$acc, $goi = $m.Groups[1].Value, $m.Groups[2].Value

$out = Invoke-Parallel "EXEC dbo.sp_DonHang_Create @AccountId = $acc, @GoiNapId = $goi, @TaiKhoanGame = 'p1', @PhuongThucThanhToan = 'VI';"
$ok = Count-Match $out 'RESULT:OK'
$rej = Count-Match $out 'RESULT:ERR:50106'
$bal = [decimal]((Run-Sql "SET NOCOUNT ON; SELECT SoDu FROM dbo.Account WHERE AccountId = $acc") | Select-Object -First 1)
$ord = [int]((Run-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.DonHang WHERE AccountId = $acc") | Select-Object -First 1)
if ($ok -ne 1 -or $rej -ne 1) { Fail "08B: cần đúng 1 OK và 1 lỗi 50106 (OK=$ok, 50106=$rej).`n$out" }
if ($bal -ne 10000) { Fail "08B: ví phải còn 10000, thực tế $bal." }
if ($ord -ne 1) { Fail "08B: phải có đúng 1 đơn, thực tế $ord." }
Write-Host "PASS  08B mua gói bằng ví song song" -ForegroundColor Green