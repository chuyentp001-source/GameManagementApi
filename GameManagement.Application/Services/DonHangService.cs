
using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Services;

public class DonHangService : IDonHangService
{
    private readonly IDonHangRepository _donHangRepository;
    private readonly IGoiNapRepository _goiNapRepository;

    public DonHangService(
        IDonHangRepository donHangRepository,
        IGoiNapRepository goiNapRepository)
    {
        _donHangRepository = donHangRepository;
        _goiNapRepository = goiNapRepository;
    }

    public async Task<PagedResultDto<DonHangDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThaiDonHang,
        string? trangThaiThanhToan,
        int? accountId,
        string? tuKhoa)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Số bản ghi mỗi trang phải từ 1 đến 100.");

        var donHangPaged = await _donHangRepository.GetAllPagedAsync(
            pageNumber,
            pageSize,
            loaiGiaoDich,
            trangThaiDonHang,
            trangThaiThanhToan,
            accountId,
            tuKhoa);

        var result = new PagedResultDto<DonHangDto>
        {
            PageNumber = donHangPaged.PageNumber,
            PageSize = donHangPaged.PageSize,
            TotalRecords = donHangPaged.TotalRecords,
            TotalPages = donHangPaged.TotalPages
        };

        foreach (var donHang in donHangPaged.Items)
        {
            result.Items.Add(MapToDto(donHang));
        }

        return result;
    }

    public async Task<DonHangDto> CreateAsync(
        int accountId,
        CreateDonHangDto dto)
    {
        if (accountId <= 0)
            throw new ArgumentException("Tài khoản không hợp lệ.");

        if (dto.GoiNapId <= 0)
            throw new ArgumentException("Gói nạp không hợp lệ.");

        if (string.IsNullOrWhiteSpace(dto.TaiKhoanGame))
            throw new ArgumentException("Tài khoản game không được để trống.");

        if (!dto.PhuongThucThanhToan.HasValue)
        {
            throw new ArgumentException(
                "Phương thức thanh toán không được để trống.");
        }

        var phuongThuc = dto.PhuongThucThanhToan.Value;

        if (phuongThuc != PhuongThucThanhToan.VI &&
            phuongThuc != PhuongThucThanhToan.CHUYEN_KHOAN)
        {
            throw new ArgumentException(
                "Chỉ hỗ trợ thanh toán bằng ví hoặc chuyển khoản.");
        }

        var goiNap = await _goiNapRepository.GetByIdAsync(dto.GoiNapId);

        if (goiNap == null)
            throw new GoiNapNotFoundException(
                "Gói nạp không tồn tại.");

        if (goiNap.TrangThai != GoiNapStatus.HOAT_DONG)
            throw new InvalidGoiNapException(
                "Gói nạp hiện không hoạt động.");

        if (goiNap.GiaGoc < 1000)
            throw new InvalidGoiNapException(
                "Giá gói nạp không hợp lệ.");

        var donHang = new DonHang
        {
            AccountId = accountId,
            GoiNapId = dto.GoiNapId,
            TaiKhoanGame = dto.TaiKhoanGame.Trim(),
            Server = string.IsNullOrWhiteSpace(dto.Server)
                ? null
                : dto.Server.Trim(),
            TongTienThanhToan = goiNap.GiaGoc,
            PhuongThucThanhToan =phuongThuc,
            TrangThaiThanhToan =
                TrangThaiThanhToan.CHO_THANH_TOAN,
            TrangThaiDonHang =
                TrangThaiDonHang.CHO_XU_LY,
            NguoiTaoId = accountId,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        var createdDonHang =
            await _donHangRepository.CreateAsync(donHang);

        return MapToDto(createdDonHang);
    }

    public async Task<DonHangDto?> GetByIdAsync(long id)
    {
        if (id <= 0)
            throw new ArgumentException(
                "Mã đơn hàng phải lớn hơn 0.");

        var donHang =
            await _donHangRepository.GetByIdAsync(id);

        if (donHang == null)
            return null;

        return MapToDto(donHang);
    }

    public async Task<List<DonHangDto>> GetByAccountIdAsync(
        int accountId)
    {
        if (accountId <= 0)
            throw new ArgumentException(
                "Mã tài khoản không hợp lệ.");

        var donHangs =
            await _donHangRepository.GetByAccountIdAsync(accountId);

        var result = new List<DonHangDto>();

        foreach (var donHang in donHangs)
        {
            result.Add(MapToDto(donHang));
        }

        return result;
    }

    public async Task<DonHangDto?> ThanhToanBangViAsync(
        long donHangId,
        int accountId)
    {
        if (donHangId <= 0)
            throw new ArgumentException(
                "Mã đơn hàng phải lớn hơn 0.");

        if (accountId <= 0)
            throw new ArgumentException(
                "Mã tài khoản không hợp lệ.");

        var donHang = await _donHangRepository
            .ThanhToanBangViAsync(donHangId, accountId);

        if (donHang == null)
            return null;

        return MapToDto(donHang);
    }

    public async Task<DonHangDto?> UpdateStatusAsync(
    long donHangId,
    UpdateDonHangStatusDto dto,
    int nguoiCapNhatId)
    {
        if (donHangId <= 0)
        {
            throw new ArgumentException(
                "Mã đơn hàng không hợp lệ.");
        }

        if (nguoiCapNhatId <= 0)
        {
            throw new ArgumentException(
                "Mã người cập nhật không hợp lệ.");
        }

        if (!dto.TrangThaiDonHang.HasValue ||
            !Enum.IsDefined(typeof(TrangThaiDonHang),
                dto.TrangThaiDonHang.Value))
        {
            throw new ArgumentException(
                "Trạng thái đơn hàng không hợp lệ.");
        }

        var trangThai = dto.TrangThaiDonHang.Value;

        if (trangThai != TrangThaiDonHang.DANG_XU_LY &&
            trangThai != TrangThaiDonHang.NAP_GAME_THAT_BAI &&
            trangThai != TrangThaiDonHang.HOAN_THANH)
        {
            throw new InvalidOperationException(
                "Trạng thái đơn hàng không hợp lệ.");
        }
        var donHang = await _donHangRepository.UpdateStatusAsync(
            donHangId,
            trangThai,
            nguoiCapNhatId);

        if (donHang == null)
        {
            return null;
        }

        return MapToDto(donHang);
    }

    public async Task<DonHangDto?> HoanTienAsync(
      long donHangId,
      int nguoiCapNhatId)
    {
        if (donHangId <= 0)
        {
            throw new ArgumentException(
                "Mã đơn hàng không hợp lệ.");
        }

        if (nguoiCapNhatId <= 0)
        {
            throw new ArgumentException(
                "Mã người cập nhật không hợp lệ.");
        }

        var donHang = await _donHangRepository
            .HoanTienAsync(donHangId, nguoiCapNhatId);

        if (donHang == null)
        {
            return null;
        }

        return MapToDto(donHang);
    }

    public async Task<YeuCauNapTienDto?> TaoYeuCauNapTienAsync(
        int accountId,
        CreateYeuCauNapTienDto dto)
    {
        var donHang = await _donHangRepository.TaoYeuCauNapTienAsync(
            accountId,
            dto.SoTien);

        if (donHang == null)
        {
            return null;
        }

        return new YeuCauNapTienDto
        {
            DonHangId = donHang.DonHangId,
            SoTien = donHang.TongTienThanhToan,
            NoiDungChuyenKhoan = donHang.MaGiaoDichBank ?? string.Empty,
            PhuongThucThanhToan = donHang.PhuongThucThanhToan.ToString(),
            TrangThaiThanhToan = donHang.TrangThaiThanhToan.ToString(),
            NgayTao = donHang.NgayTao
        };
    }

    public async Task<DonHangDto?> XacNhanNapTienAsync(
    long donHangId,
    int nguoiXacNhanId)
    {
        var donHang = await _donHangRepository
            .XacNhanNapTienAsync(donHangId, nguoiXacNhanId);

        if (donHang == null)
        {
            return null;
        }

        return MapToDto(donHang);
    }

    public async Task<DonHangDto?> XacNhanThanhToanChuyenKhoanAsync(
        long donHangId,
        int nguoiXacNhanId)
    {
        var donHang = await _donHangRepository
            .XacNhanThanhToanChuyenKhoanAsync(
                donHangId,
                nguoiXacNhanId);

        if (donHang == null)
        {
            return null;
        }

        return MapToDto(donHang);
    }
    public async Task<DonHangDto?> HoanTienChuyenKhoanAsync(
    long donHangId,
    int nguoiCapNhatId)
    {
        if (donHangId <= 0)
            throw new ArgumentException("DonHangId không hợp lệ.");

        if (nguoiCapNhatId <= 0)
            throw new ArgumentException("AccountId người cập nhật không hợp lệ.");

        var donHang = await _donHangRepository.HoanTienChuyenKhoanAsync(
            donHangId,
            nguoiCapNhatId);

        if (donHang == null)
            return null;

        return MapToDto(donHang);
    }

    private static DonHangDto MapToDto(DonHang donHang)
    {
        return new DonHangDto
        {
            DonHangId = donHang.DonHangId,
            AccountId = donHang.AccountId,
            GoiNapId = donHang.GoiNapId,
            ChietKhauId = donHang.ChietKhauId,
            TaiKhoanGame = donHang.TaiKhoanGame,
            Server = donHang.Server,
            TongTienThanhToan = donHang.TongTienThanhToan,
            PhuongThucThanhToan = donHang.PhuongThucThanhToan,
            MaGiaoDichBank = donHang.MaGiaoDichBank,
            TrangThaiThanhToan = donHang.TrangThaiThanhToan,
            TrangThaiDonHang = donHang.TrangThaiDonHang,
            NgayTao = donHang.NgayTao,
            NgayCapNhat = donHang.NgayCapNhat
        };
    }
}
