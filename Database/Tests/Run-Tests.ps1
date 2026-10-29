param(
    [string]$Server = "(localdb)\MSSQLLocalDB",
    [string]$Db = "GameManagementDB_Test"
)

$ErrorActionPreference = "Stop"

if ($Db -notlike "*_Test") {
    throw "Chi duoc chay runner tren database co ten ket thuc bang _Test."
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "Khong tim thay sqlcmd. Cai SQL Server Command Line Utilities hoac them sqlcmd vao PATH."
}

$databaseDir = Split-Path $PSScriptRoot -Parent
$schemaFile = Join-Path $databaseDir "01_schema.sql"

if (-not (Test-Path $schemaFile)) {
    throw "Khong tim thay schema: $schemaFile"
}
if ($Db -ne "GameManagementDB_Test") {
    throw "Database không đúng tên kiểm thử dự kiến."
}

$confirm = Read-Host "Database [$Db] sẽ bị xóa và tạo lại. Nhập YES để tiếp tục"
if ($confirm -cne "YES") {
    throw "Đã hủy chạy test."
}

Write-Host "Tao lai database test [$Db] tren [$Server]..." -ForegroundColor Cyan
$sqlCreate = @"
IF DB_ID(N'$Db') IS NOT NULL
BEGIN
    ALTER DATABASE [$Db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$Db];
END;
CREATE DATABASE [$Db];
"@

sqlcmd -S $Server -E -b -Q $sqlCreate
if ($LASTEXITCODE -ne 0) {
    throw "Khong the tao database test."
}

Write-Host "Nap schema..." -ForegroundColor Cyan
sqlcmd -S $Server -d $Db -E -b -I -i $schemaFile
if ($LASTEXITCODE -ne 0) {
    throw "Khong the nap schema vao database test."
}

$pass = 0
$fail = 0

$testFiles = Get-ChildItem -Path $PSScriptRoot -File |
    Where-Object { $_.Name -match '^\d{2}_Test_.*\.sql$' } |
    Sort-Object Name

foreach ($file in $testFiles) {
    Write-Host "`nChay $($file.Name)..." -ForegroundColor Cyan
    $output = & sqlcmd -S $Server -d $Db -E -b -I -i $file.FullName 2>&1
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "PASS  $($file.Name)" -ForegroundColor Green
        $pass++
    }
    else {
        Write-Host "FAIL  $($file.Name)" -ForegroundColor Red
        $output | ForEach-Object { Write-Host $_ }
        $fail++
    }
}

Write-Host "`nChay test dong thoi..." -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "08_Test_DongThoi.ps1") -Server $Server -Db $Db
$concurrencyExitCode = $LASTEXITCODE
if ($concurrencyExitCode -eq 0) {
    $pass++
}
else {
    $fail++
}

Write-Host "`nKet qua: $pass dat, $fail loi." -ForegroundColor $(if ($fail -eq 0) { "Green" } else { "Red" })
if ($fail -gt 0) {
    exit 1
}
