/*
    02_seed.sql
    Du lieu mau cho GameManagementDB.
    Chi them Game va GoiNap; khong tao tai khoan, User, mat khau hay du lieu giao dich.
    Gia goi nap ben duoi la gia TRI MAU de phat trien/kiem thu, khong phai bang gia chinh thuc.
    Chay sau 01_schema.sql.
*/

USE [GameManagementDB];
GO

SET NOCOUNT ON;
GO

/* 1. Them cac game mau neu chua ton tai. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Game
    WHERE TenGame = N'Liên Quân Mobile'
)
BEGIN
    INSERT INTO dbo.Game (TenGame, TrangThaiGame, CoServer)
    VALUES (N'Liên Quân Mobile', 'HOAT_DONG', 0);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Game
    WHERE TenGame = N'Free Fire'
)
BEGIN
    INSERT INTO dbo.Game (TenGame, TrangThaiGame, CoServer)
    VALUES (N'Free Fire', 'HOAT_DONG', 0);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Game
    WHERE TenGame = N'PUBG Mobile'
)
BEGIN
    INSERT INTO dbo.Game (TenGame, TrangThaiGame, CoServer)
    VALUES (N'PUBG Mobile', 'HOAT_DONG', 0);
END;
GO

/* 2. Them cac goi nap mau neu chua ton tai. */
INSERT INTO dbo.GoiNap (GameID, TenGoiNap, GiaGoc)
SELECT
    g.GameID,
    mau.TenGoiNap,
    mau.GiaGoc
FROM
(
    VALUES
        (N'Liên Quân Mobile', N'Gói mẫu 20.000 VNĐ', CAST(20000 AS DECIMAL(18, 2))),
        (N'Liên Quân Mobile', N'Gói mẫu 50.000 VNĐ', CAST(50000 AS DECIMAL(18, 2))),
        (N'Free Fire',        N'Gói mẫu 20.000 VNĐ', CAST(20000 AS DECIMAL(18, 2))),
        (N'Free Fire',        N'Gói mẫu 50.000 VNĐ', CAST(50000 AS DECIMAL(18, 2))),
        (N'PUBG Mobile',      N'Gói mẫu 20.000 VNĐ', CAST(20000 AS DECIMAL(18, 2))),
        (N'PUBG Mobile',      N'Gói mẫu 50.000 VNĐ', CAST(50000 AS DECIMAL(18, 2)))
) AS mau(TenGame, TenGoiNap, GiaGoc)
INNER JOIN dbo.Game AS g
    ON g.TenGame = mau.TenGame
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.GoiNap AS gn
    WHERE gn.GameID = g.GameID
      AND gn.TenGoiNap = mau.TenGoiNap
);
GO

/* 3. Kiem tra nhanh du lieu mau. */
SELECT GameID, TenGame, TrangThaiGame
FROM dbo.Game
WHERE TenGame IN (N'Liên Quân Mobile', N'Free Fire', N'PUBG Mobile')
ORDER BY TenGame;
GO

SELECT g.TenGame, gn.TenGoiNap, gn.GiaGoc, gn.TrangThai
FROM dbo.GoiNap AS gn
INNER JOIN dbo.Game AS g ON g.GameID = gn.GameID
WHERE g.TenGame IN (N'Liên Quân Mobile', N'Free Fire', N'PUBG Mobile')
  AND gn.TenGoiNap IN (N'Gói mẫu 20.000 VNĐ', N'Gói mẫu 50.000 VNĐ')
ORDER BY g.TenGame, gn.GiaGoc;
GO
