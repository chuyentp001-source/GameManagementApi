using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Services;

public class ChietKhauService : IChietKhauService
{
    private readonly IChietKhauRepository _chietKhauRepository;

    public ChietKhauService(IChietKhauRepository chietKhauRepository)
    {
        _chietKhauRepository = chietKhauRepository;
    }

    public async Task<PagedResultDto<ChietKhauDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        int? goiNapId,
        string? trangThai)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Số bản ghi mỗi trang phải từ 1 đến 100.");

        if (goiNapId.HasValue && goiNapId.Value <= 0)
            throw new ArgumentException("Mã gói nạp phải lớn hơn 0.");

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            if (!Enum.TryParse<ChietKhauStatus>(
                    trangThai.Trim(),
                    ignoreCase: false,
                    out var status)
                || !Enum.IsDefined(status))
            {
                throw new ArgumentException("Trạng thái chiết khấu không hợp lệ.");
            }

            trangThai = status.ToString();
        }

        var data = await _chietKhauRepository.GetAllPagedAsync(
            pageNumber,
            pageSize,
            goiNapId,
            trangThai);

        var result = new PagedResultDto<ChietKhauDto>
        {
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalRecords = data.TotalRecords,
            TotalPages = data.TotalPages
        };

        foreach (var item in data.Items)
        {
            if (!Enum.TryParse<ChietKhauStatus>(
                    item.TrangThai,
                    ignoreCase: false,
                    out var status)
                || !Enum.IsDefined(status))
            {
                throw new InvalidOperationException(
                    $"Trạng thái chiết khấu '{item.TrangThai}' trong database không hợp lệ.");
            }

            result.Items.Add(new ChietKhauDto
            {
                ChietKhauId = item.ChietKhauId,
                GoiNapId = item.GoiNapId,
                TenChietKhau = item.TenChietKhau,
                PhanTramChietKhau = item.PhanTramChietKhau,
                NgayBatDau = item.NgayBatDau,
                NgayKetThuc = item.NgayKetThuc,
                TrangThai = status,
                NgayTao = item.NgayTao,
                NgayCapNhat = item.NgayCapNhat
            });
        }

        return result;
    }


    public async Task<ChietKhauDto> CreateAsync(
        CreateChietKhauDto dto,
        int nguoiTaoId)
    {
        if (nguoiTaoId <= 0)
            throw new ArgumentException("Mã người tạo không hợp lệ.");

        if (dto.GoiNapId <= 0)
            throw new ArgumentException("Mã gói nạp phải lớn hơn 0.");

        if (string.IsNullOrWhiteSpace(dto.TenChietKhau))
            throw new ArgumentException("Tên chiết khấu không được để trống.");

        if (dto.PhanTramChietKhau <= 0 || dto.PhanTramChietKhau > 100)
            throw new ArgumentException(
                "Phần trăm chiết khấu phải lớn hơn 0 và không vượt quá 100.");

        if (dto.NgayKetThuc.HasValue &&
            dto.NgayKetThuc.Value <= dto.NgayBatDau)
        {
            throw new ArgumentException(
                "Ngày kết thúc phải sau ngày bắt đầu.");
        }

        var now = DateTime.Now;

        ChietKhauStatus trangThai;

        if (now < dto.NgayBatDau)
        {
            trangThai = ChietKhauStatus.CHUA_BAT_DAU;
        }
        else if (dto.NgayKetThuc.HasValue &&
                 now >= dto.NgayKetThuc.Value)
        {
            trangThai = ChietKhauStatus.KET_THUC;
        }
        else
        {
            trangThai = ChietKhauStatus.DANG_AP_DUNG;
        }

        var chietKhau = new GameManagement.Domain.Entities.ChietKhau
        {
            GoiNapId = dto.GoiNapId,
            TenChietKhau = dto.TenChietKhau.Trim(),
            PhanTramChietKhau = dto.PhanTramChietKhau,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            TrangThai = trangThai.ToString(),
            NguoiTaoId = nguoiTaoId
        };

        var created = await _chietKhauRepository.CreateAsync(chietKhau);

        return new ChietKhauDto
        {
            ChietKhauId = created.ChietKhauId,
            GoiNapId = created.GoiNapId,
            TenChietKhau = created.TenChietKhau,
            PhanTramChietKhau = created.PhanTramChietKhau,
            NgayBatDau = created.NgayBatDau,
            NgayKetThuc = created.NgayKetThuc,
            TrangThai = trangThai,
            NgayTao = created.NgayTao,
            NgayCapNhat = created.NgayCapNhat
        };
    }

    public async Task<ChietKhauDto?> UpdateAsync(
        int chietKhauId,
        UpdateChietKhauDto dto,
        int nguoiCapNhatId)
    {
        if (chietKhauId <= 0)
            throw new ChietKhauException("ID chiết khấu không hợp lệ.");

        if (nguoiCapNhatId <= 0)
            throw new ChietKhauException("Người cập nhật không hợp lệ.");

        if (string.IsNullOrWhiteSpace(dto.TenChietKhau))
            throw new ChietKhauException("Tên chiết khấu không được để trống.");

        if (dto.PhanTramChietKhau <= 0 || dto.PhanTramChietKhau > 100)
            throw new ChietKhauException(
                "Phần trăm chiết khấu phải lớn hơn 0 và không vượt quá 100.");

        if (dto.NgayKetThuc.HasValue &&
            dto.NgayKetThuc.Value <= dto.NgayBatDau)
        {
            throw new ChietKhauException(
                "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
        }

        var chietKhau = new ChietKhau
        {
            TenChietKhau = dto.TenChietKhau.Trim(),
            PhanTramChietKhau = dto.PhanTramChietKhau,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc
        };

        var updated = await _chietKhauRepository.UpdateAsync(
            chietKhauId,
            chietKhau,
            nguoiCapNhatId);

        if (updated == null)
            return null;

        if (!Enum.TryParse<ChietKhauStatus>(
                updated.TrangThai,
                out var trangThai))
        {
            throw new ChietKhauException(
                "Trạng thái chiết khấu trong cơ sở dữ liệu không hợp lệ.",
                500);
        }

        return new ChietKhauDto
        {
            ChietKhauId = updated.ChietKhauId,
            GoiNapId = updated.GoiNapId,
            TenChietKhau = updated.TenChietKhau,
            PhanTramChietKhau = updated.PhanTramChietKhau,
            NgayBatDau = updated.NgayBatDau,
            NgayKetThuc = updated.NgayKetThuc,
            TrangThai = trangThai,
            NgayTao = updated.NgayTao,
            NgayCapNhat = updated.NgayCapNhat
        };
    }

    public async Task DeleteAsync(int chietKhauId, int nguoiCapNhatId)
    {
        if (chietKhauId <= 0)
            throw new ChietKhauException("Mã chiết khấu không hợp lệ.");

        if (nguoiCapNhatId <= 0)
            throw new ChietKhauException("Người cập nhật không hợp lệ.");

        await _chietKhauRepository.DeleteAsync(
            chietKhauId,
            nguoiCapNhatId);
    }

}
