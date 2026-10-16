using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    public AccountService(
      IAccountRepository accountRepository,
      IJwtService jwtService,
      ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<AccountDto>> GetAllAsync(
        int pageNumber,
        int pageSize,
        int? accountId,
        AccountRole? role,
        AccountStatus? trangThai)
    {
        if (pageNumber < 1)
            throw new ArgumentException(
                "Số trang phải lớn hơn hoặc bằng 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException(
                "Số tài khoản mỗi trang phải từ 1 đến 100.");

        var accountPage = await _accountRepository.GetAllAsync(
            pageNumber,
            pageSize,
            accountId,
            role?.ToString(),
            trangThai?.ToString());

        var items = new List<AccountDto>();

        foreach (var account in accountPage.Items)
        {
            items.Add(new AccountDto
            {
                AccountId = account.AccountId,
                TenDangNhap = account.TenDangNhap,
                SoDu = account.SoDu,
                Role = account.Role,
                TrangThai = account.TrangThai
            });
        }

        return new PagedResultDto<AccountDto>
        {
            Items = items,
            PageNumber = accountPage.PageNumber,
            PageSize = accountPage.PageSize,
            TotalRecords = accountPage.TotalRecords,
            TotalPages = accountPage.TotalPages
        };
    }



    public async Task<MyAccountDto?> GetMeAsync(int accountId)
    {
        return await _accountRepository.GetMeAsync(accountId);
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);

        if (account == null)
            return null;

        return new AccountDto
        {
            AccountId = account.AccountId,
            TenDangNhap = account.TenDangNhap,
            SoDu = account.SoDu,
            Role = account.Role,
            TrangThai = account.TrangThai
        };
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        // 1. Kiểm tra tên đăng nhập đã tồn tại chưa
        var exists = await _accountRepository
            .ExistsByUsernameAsync(dto.TenDangNhap);

        if (exists)
        {
            throw new DuplicateException(
                "Tên đăng nhập đã tồn tại.");
        }

        // 2. Tạo tài khoản đăng nhập
        var account = new Account
        {
            TenDangNhap = dto.TenDangNhap.Trim(),
            MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
            SoDu = 0,
            Role = AccountRole.USER,
            TrangThai = AccountStatus.HOAT_DONG
        };

        // 3. Tạo hồ sơ người dùng
        var user = new User
        {
            HoTen = dto.HoTen.Trim(),
            Email = dto.Email.Trim(),

            SoDienThoai = string.IsNullOrWhiteSpace(
                dto.SoDienThoai)
                    ? null
                    : dto.SoDienThoai.Trim(),

            DiaChi = string.IsNullOrWhiteSpace(dto.DiaChi)
                ? null
                : dto.DiaChi.Trim()
        };

        // 4. Lưu tài khoản và hồ sơ trong cùng transaction
        var createdAccount = await _accountRepository
            .RegisterAsync(account, user);

        // 5. Trả kết quả, không trả mật khẩu
        return new AccountDto
        {
            AccountId = createdAccount.AccountId,
            TenDangNhap = createdAccount.TenDangNhap,
            SoDu = createdAccount.SoDu,
            Role = createdAccount.Role,
            TrangThai = createdAccount.TrangThai
        };
    }

    public async Task<int> AdminCreateAsync(
        AdminCreateAccountDto dto,
        int nguoiTaoId)
    {
        if (nguoiTaoId <= 0)
            throw new ArgumentException("AccountID người tạo không hợp lệ.");

        if (dto == null)
            throw new ArgumentException("Dữ liệu tài khoản không hợp lệ.");

        if (string.IsNullOrWhiteSpace(dto.TenDangNhap))
            throw new ArgumentException("Tên đăng nhập không được để trống.");

        if (string.IsNullOrWhiteSpace(dto.MatKhau) ||
            dto.MatKhau.Length < 8 ||
            dto.MatKhau.Length > 72)
        {
            throw new ArgumentException(
                "Mật khẩu phải có từ 8 đến 72 ký tự.");
        }

        if (string.IsNullOrWhiteSpace(dto.HoTen))
            throw new ArgumentException("Họ tên không được để trống.");

        if (!Enum.IsDefined(typeof(AccountRole), dto.Role))
            throw new ArgumentException("Vai trò không hợp lệ.");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);

        return await _accountRepository.AdminCreateAsync(
            dto,
            hashedPassword,
            nguoiTaoId);
    }



    public async Task<AccountDto?> UpdateAsync(int id, UpdateAccountDto dto, int nguoiCapNhatId)
    {
        // 1. Kiểm tra Enum hợp lệ
        if (!Enum.IsDefined(typeof(AccountRole), dto.Role))
        {
            throw new ArgumentException(
                "Vai trò tài khoản không hợp lệ.");
        }

        if (!Enum.IsDefined(typeof(AccountStatus), dto.TrangThai))
        {
            throw new ArgumentException(
                "Trạng thái tài khoản không hợp lệ.");
        }

        // 2. Tạo đối tượng cập nhật
        var account = new Account
        {
            AccountId = id,
            Role = dto.Role,
            TrangThai = dto.TrangThai
        };

        // 3. Gọi Repository
        var updatedAccount = await _accountRepository.UpdateAsync(id, dto, nguoiCapNhatId);

        if (updatedAccount == null)
        {
            return null;
        }

        // 4. Trả DTO, không trả mật khẩu
        return new AccountDto
        {
            AccountId = updatedAccount.AccountId,
            TenDangNhap = updatedAccount.TenDangNhap,
            SoDu = updatedAccount.SoDu,
            Role = updatedAccount.Role,
            TrangThai = updatedAccount.TrangThai
        };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var account = await _accountRepository.GetByUsernameAsync(dto.TenDangNhap);

        if (account == null)
        {
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không đúng.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.MatKhau, account.MatKhau))
        {
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không đúng.");
        }

        if (account.TrangThai != AccountStatus.HOAT_DONG)
        {
            throw new UnauthorizedAccessException("Tài khoản tạm thời bị khóa, liên hệ admin để biết thông tin chi tiết.");
        }

        var token = _jwtService.GenerateToken(
            account.AccountId,
            account.TenDangNhap,
            account.Role.ToString());

        var acccountDto = new AccountDto
        {
            AccountId = account.AccountId,
            TenDangNhap = account.TenDangNhap,
            SoDu = account.SoDu,
            Role = account.Role,
            TrangThai = account.TrangThai
        };

        return new LoginResponseDto
        {
            Token = token,
            Account = acccountDto
        };
    }

    public async Task ChangePasswordAsync(int accountId, ChangePasswordDto dto)
    {
        // 1. Kiểm tra mật khẩu mới và xác nhận mật khẩu khớp nhau
        if (dto.MatKhauMoi != dto.XacNhanMatKhauMoi)
        {
            throw new Exception("Mật khẩu mới và xác nhận mật khẩu không khớp.");
        }
        if (string.IsNullOrWhiteSpace(dto.MatKhauMoi)
                || dto.MatKhauMoi.Length < 8
                || dto.MatKhauMoi.Length > 72)
        {
            throw new ArgumentException("Mật khẩu mới phải có từ 8 đến 72 ký tự.");
        }

        // 2. Lấy thông tin tài khoản từ DB để kiểm tra mật khẩu hiện tại
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new Exception("Không tìm thấy thông tin tài khoản.");
        }

        // 3. Kiểm tra mật khẩu cũ (dùng !isOldPasswordValid để báo lỗi khi SAI)
        bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(dto.MatKhauCu, account.MatKhau);
        if (!isOldPasswordValid)
        {
            throw new Exception("Mật khẩu hiện tại không chính xác.");
        }

        // 4. Hash mật khẩu mới
        string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);

        // 5. Cập nhật mật khẩu mới xuống DB qua Repository
        await _accountRepository.UpdatePasswordAsync(accountId, newPasswordHash);
    }

    public async Task<AccountDto?> UpdateStatusAsync(int accountId, ChangeAccountStatusDto dto)
    {
        // Thử parse từ chuỗi sang Enum AccountStatus (không phân biệt hoa thường)
        if (!Enum.TryParse<AccountStatus>(dto.TrangThai, ignoreCase: true, out var statusEnum)
            || !Enum.IsDefined(typeof(AccountStatus), statusEnum))
        {
            throw new ArgumentException("Trạng thái tài khoản không hợp lệ (Chỉ chấp nhận HOAT_DONG hoặc BI_KHOA).");
        }

        // Truyền statusEnum xuống Repository
        var account = await _accountRepository.UpdateStatusAsync(accountId, statusEnum);

        if (account == null)
        {
            return null;
        }

        return new AccountDto
        {
            AccountId = account.AccountId,
            TenDangNhap = account.TenDangNhap,
            Role = account.Role,
            TrangThai = account.TrangThai,
            SoDu = account.SoDu
        };
    }
    public async Task<bool> DeleteAsync(int accountId)
    {
        var nguoiCapNhatId = _currentUserService.AccountId;

        if (!nguoiCapNhatId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Không xác định được tài khoản đang đăng nhập.");
        }

        if (accountId == nguoiCapNhatId.Value)
        {
            throw new InvalidOperationException(
                "Không thể xóa tài khoản đang đăng nhập.");
        }

        return await _accountRepository.DeleteAsync(
            accountId,
            nguoiCapNhatId.Value);
    }

}