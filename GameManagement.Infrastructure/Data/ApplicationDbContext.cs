using System;
using System.Collections.Generic;
using GameManagement.Domain.Entities;
using GameManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GameManagement.Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<ChietKhau> ChietKhaus { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GoiNap> GoiNaps { get; set; }

    public virtual DbSet<LichSuVi> LichSuVis { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.HasIndex(e => e.TenDangNhap, "UQ_Account_TenDangNhap").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_Account_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_Account_NgayTao");
            entity.Property(e => e.NguoiCapNhatId).HasColumnName("NguoiCapNhatID");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTaoID");
            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("USER", "DF_Account_Role");
            entity.Property(e => e.SoDu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("HOAT_DONG", "DF_Account_TrangThai");

            entity.HasOne(d => d.NguoiCapNhat).WithMany(p => p.InverseNguoiCapNhat)
                .HasForeignKey(d => d.NguoiCapNhatId)
                .HasConstraintName("FK_Account_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.InverseNguoiTao)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_Account_NguoiTao");
        });

        modelBuilder.Entity<ChietKhau>(entity =>
        {
            entity.ToTable("ChietKhau");

            entity.HasIndex(e => e.GoiNapId, "IX_ChietKhau_GoiNapID");

            entity.Property(e => e.ChietKhauId).HasColumnName("ChietKhauID");
            entity.Property(e => e.GoiNapId).HasColumnName("GoiNapID");
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_ChietKhau_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_ChietKhau_NgayTao");
            entity.Property(e => e.NguoiCapNhatId).HasColumnName("NguoiCapNhatID");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTaoID");
            entity.Property(e => e.PhanTramChietKhau).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TenChietKhau).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("CHUA_BAT_DAU", "DF_ChietKhau_TrangThai");

            entity.HasOne(d => d.GoiNap).WithMany(p => p.ChietKhaus)
                .HasForeignKey(d => d.GoiNapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChietKhau_GoiNap");

            entity.HasOne(d => d.NguoiCapNhat).WithMany(p => p.ChietKhauNguoiCapNhats)
                .HasForeignKey(d => d.NguoiCapNhatId)
                .HasConstraintName("FK_ChietKhau_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.ChietKhauNguoiTaos)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_ChietKhau_NguoiTao");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.ToTable("DonHang");

            entity.HasIndex(e => e.AccountId, "IX_DonHang_AccountID");

            entity.HasIndex(e => e.ChietKhauId, "IX_DonHang_ChietKhauID");

            entity.HasIndex(e => e.GoiNapId, "IX_DonHang_GoiNapID");

            entity.HasIndex(e => new { e.TrangThaiDonHang, e.TrangThaiThanhToan }, "IX_DonHang_TrangThai");

            entity.Property(e => e.DonHangId).HasColumnName("DonHangID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.ChietKhauId).HasColumnName("ChietKhauID");
            entity.Property(e => e.GoiNapId).HasColumnName("GoiNapID");
            entity.Property(e => e.MaGiaoDichBank)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_DonHang_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_DonHang_NgayTao");
            entity.Property(e => e.NguoiCapNhatId).HasColumnName("NguoiCapNhatID");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTaoID");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Server)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TaiKhoanGame)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TongTienThanhToan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThaiDonHang)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("CHO_XU_LY", "DF_DonHang_TrangThaiDonHang");
            entity.Property(e => e.TrangThaiThanhToan)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("CHO_THANH_TOAN", "DF_DonHang_TrangThaiThanhToan");

            entity.HasOne(d => d.Account).WithMany(p => p.DonHangAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_Account");

            entity.HasOne(d => d.ChietKhau).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.ChietKhauId)
                .HasConstraintName("FK_DonHang_ChietKhau");

            entity.HasOne(d => d.GoiNap).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.GoiNapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_GoiNap");

            entity.HasOne(d => d.NguoiCapNhat).WithMany(p => p.DonHangNguoiCapNhats)
                .HasForeignKey(d => d.NguoiCapNhatId)
                .HasConstraintName("FK_DonHang_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.DonHangNguoiTaos)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_DonHang_NguoiTao");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Game");

            entity.HasIndex(e => e.TenGame, "UQ_Game_TenGame").IsUnique();

            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_Game_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_Game_NgayTao");
            entity.Property(e => e.NguoiCapNhatId).HasColumnName("NguoiCapNhatID");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTaoID");
            entity.Property(e => e.TenGame).HasMaxLength(200);
            entity.Property(e => e.TrangThaiGame)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("HOAT_DONG", "DF_Game_TrangThai");

            entity.HasOne(d => d.NguoiCapNhat).WithMany(p => p.GameNguoiCapNhats)
                .HasForeignKey(d => d.NguoiCapNhatId)
                .HasConstraintName("FK_Game_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.GameNguoiTaos)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_Game_NguoiTao");
        });

        modelBuilder.Entity<GoiNap>(entity =>
        {
            entity.ToTable("GoiNap");

            entity.HasIndex(e => e.GameId, "IX_GoiNap_GameID");

            entity.Property(e => e.GoiNapId).HasColumnName("GoiNapID");
            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.GiaGoc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_GoiNap_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_GoiNap_NgayTao");
            entity.Property(e => e.NguoiCapNhatId).HasColumnName("NguoiCapNhatID");
            entity.Property(e => e.NguoiTaoId).HasColumnName("NguoiTaoID");
            entity.Property(e => e.TenGoiNap).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("HOAT_DONG", "DF_GoiNap_TrangThai");

            entity.HasOne(d => d.Game).WithMany(p => p.GoiNaps)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoiNap_Game");

            entity.HasOne(d => d.NguoiCapNhat).WithMany(p => p.GoiNapNguoiCapNhats)
                .HasForeignKey(d => d.NguoiCapNhatId)
                .HasConstraintName("FK_GoiNap_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.GoiNapNguoiTaos)
                .HasForeignKey(d => d.NguoiTaoId)
                .HasConstraintName("FK_GoiNap_NguoiTao");
        });

        modelBuilder.Entity<LichSuVi>(entity =>
        {
            entity.HasKey(e => e.LichSuId);

            entity.ToTable("LichSuVi");

            entity.HasIndex(e => e.AccountId, "IX_LichSuVi_AccountID");

            entity.HasIndex(e => e.DonHangId, "IX_LichSuVi_DonHangID");

            entity.HasIndex(e => e.LoaiGiaoDich, "IX_LichSuVi_LoaiGiaoDich");

            entity.HasIndex(e => e.NguoiThucHienId, "IX_LichSuVi_NguoiThucHienID");

            entity.Property(e => e.LichSuId).HasColumnName("LichSuID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.DonHangId).HasColumnName("DonHangID");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.LoaiGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_LichSuVi_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_LichSuVi_NgayTao");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("NguoiThucHienID");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("CHO_XU_LY", "DF_LichSuVi_TrangThai");

            entity.HasOne(d => d.Account).WithMany(p => p.LichSuViAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichSuVi_Account");

            entity.HasOne(d => d.DonHang).WithMany(p => p.LichSuVis)
                .HasForeignKey(d => d.DonHangId)
                .HasConstraintName("FK_LichSuVi_DonHang");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.LichSuViNguoiThucHiens)
                .HasForeignKey(d => d.NguoiThucHienId)
                .HasConstraintName("FK_LichSuVi_NguoiThucHien");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.AccountId, "UQ_User_Account").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.DiaChi).HasMaxLength(300);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())", "DF_User_NgayCapNhat");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_User_NgayTao");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Account).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
