using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Services
{
    /// <summary>
    /// Service xử lý logic nghiệp vụ cho Gói nạp (GoiNap).
    /// Nhận yêu cầu từ Controller, áp dụng các quy tắc nghiệp vụ (validate),
    /// và tương tác với các Repository để cập nhật dữ liệu vào Database.
    /// </summary>
    public class GoiNapService : IGoiNapService
    {
        // Khai báo các phụ thuộc (Dependencies)
        private readonly IGoiNapRepository _goiNapRepository;   // Thợ thao tác với DB bảng GoiNap
        private readonly IGameRepository _gameRepository;       // Thợ thao tác với DB bảng Game (để kiểm tra game tồn tại)
        private readonly ICurrentUserService _currentUserService; // Dịch vụ lấy thông tin người dùng đang đăng nhập từ JWT Token

        // Inject các phụ thuộc thông qua Constructor (Dependency Injection)
        public GoiNapService(
            IGoiNapRepository goiNapRepository,
            IGameRepository gameRepository,
            ICurrentUserService currentUserService)
        {
            _goiNapRepository = goiNapRepository;
            _gameRepository = gameRepository;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các gói nạp trong hệ thống.
        /// </summary>
        public async Task<List<GoiNapDto>> GetAllAsync()
        {
            // 1. Gọi Repository lấy tất cả danh sách GoiNap từ Database
            var goiNaps = await _goiNapRepository.GetAllAsync();

            // 2. Chuyển đổi (Map) danh sách Entity GoiNap sang GoiNapDto để trả về cho client
            return goiNaps.Select(x => new GoiNapDto
            {
                GoiNapId = x.GoiNapId,
                GameId = x.GameId,
                TenGoiNap = x.TenGoiNap,
                GiaGoc = x.GiaGoc,
                TrangThai = x.TrangThai,
                NguoiTaoID = x.NguoiTaoId,
                NguoiCapNhatID = x.NguoiCapNhatId,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat
            }).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết một gói nạp theo Id.
        /// </summary>
        public async Task<GoiNapDto?> GetByIdAsync(int id)
        {
            // 1. Gọi Repository tìm bản ghi GoiNap theo Id
            var goiNap = await _goiNapRepository.GetByIdAsync(id);

            // 2. Nếu không tìm thấy, trả về null để Controller phản hồi lỗi 404
            if (goiNap == null)
                return null;

            // 3. Nếu tìm thấy, ánh xạ sang GoiNapDto và trả về kết quả
            return new GoiNapDto
            {
                GoiNapId = goiNap.GoiNapId,
                GameId = goiNap.GameId,
                TenGoiNap = goiNap.TenGoiNap,
                GiaGoc = goiNap.GiaGoc,
                TrangThai = goiNap.TrangThai,
                NguoiTaoID = goiNap.NguoiTaoId,
                NguoiCapNhatID = goiNap.NguoiCapNhatId,
                NgayTao = goiNap.NgayTao,
                NgayCapNhat = goiNap.NgayCapNhat
            };
        }

        /// <summary>
        /// Tạo mới một gói nạp cho Game.
        /// </summary>
        public async Task<GoiNapDto> CreateAsync(CreateGoiNapDto dto)
        {
            // 1. Kiểm tra GameId truyền lên có tồn tại trong DB không
            var game = await _gameRepository.GetByIdAsync(dto.GameId);

            if (game == null)
                throw new GameNotFoundException("Game không tồn tại");

            // 2. Kiểm tra Game có đang ở trạng thái hoạt động không (chỉ tạo gói cho game HOAT_DONG)
            if (game.TrangThaiGame != "HOAT_DONG")
                throw new GameInactiveException("Game hiện không hoạt động");

            // 3. Ràng buộc nghiệp vụ: Giá gốc gói nạp không được nhỏ hơn 1,000 VNĐ
            if (dto.GiaGoc < 1000)
                throw new InvalidGoiNapException("Giá gốc phải từ 1,000 VNĐ trở lên");

            // 4. Khởi tạo đối tượng Entity GoiNap và ánh xạ dữ liệu từ DTO
            var goiNap = new GoiNap
            {
                GameId = dto.GameId,
                TenGoiNap = dto.TenGoiNap,
                GiaGoc = dto.GiaGoc,
                TrangThai = "HOAT_DONG", // Gói nạp mới tạo mặc định ở trạng thái HOAT_DONG
                NguoiTaoId = _currentUserService.AccountId, // Lấy ID tài khoản Admin đang thực hiện từ JWT
                NgayTao = DateTime.UtcNow                  // Ghi nhận thời gian tạo theo giờ chuẩn UTC
            };

            // 5. Gọi Repository để thêm mới bản ghi vào Database
            var createdGoiNap = await _goiNapRepository.CreateAsync(goiNap);

            // 6. Trả về GoiNapDto kèm theo thông tin vừa được tạo thành công
            return new GoiNapDto
            {
                GoiNapId = createdGoiNap.GoiNapId,
                GameId = createdGoiNap.GameId,
                TenGoiNap = createdGoiNap.TenGoiNap,
                GiaGoc = createdGoiNap.GiaGoc,
                TrangThai = createdGoiNap.TrangThai,
                NguoiTaoID = createdGoiNap.NguoiTaoId,
                NguoiCapNhatID = createdGoiNap.NguoiCapNhatId,
                NgayTao = createdGoiNap.NgayTao,
                NgayCapNhat = createdGoiNap.NgayCapNhat
            };
        }

        /// <summary>
        /// Cập nhật thông tin gói nạp đã có.
        /// </summary>
        public async Task<GoiNapDto?> UpdateAsync(int id, UpdateGoiNapDto dto)
        {
            // 1. Tìm bản ghi gói nạp cần sửa theo Id
            var goiNap = await _goiNapRepository.GetByIdAsync(id);

            // 2. Nếu không tìm thấy gói nạp, trả về null
            if (goiNap == null)
                return null;

            // 3. Ràng buộc nghiệp vụ: Kiểm tra giá gốc tối thiểu
            if (dto.GiaGoc < 1000)
                throw new InvalidGoiNapException("Giá gốc phải từ 1,000 VNĐ trở lên");

            // 4. Kiểm tra giá trị trạng thái hợp lệ (chỉ cho phép HOAT_DONG hoặc NGUNG_HOAT_DONG)
            if (dto.TrangThai != "HOAT_DONG" &&
                 dto.TrangThai != "NGUNG_HOAT_DONG")
            {
                throw new InvalidGoiNapException("Trạng thái gói nạp không hợp lệ");
            }

            // 5. Cập nhật các thông tin mới từ DTO vào Entity
            goiNap.TenGoiNap = dto.TenGoiNap;
            goiNap.GiaGoc = dto.GiaGoc;
            goiNap.TrangThai = dto.TrangThai;

            // 6. Ghi vết hệ thống (Audit field): Ai sửa và sửa khi nào
            goiNap.NguoiCapNhatId = _currentUserService.AccountId; // Lấy ID tài khoản người sửa
            goiNap.NgayCapNhat = DateTime.UtcNow;                  // Ghi nhận thời gian cập nhật

            // 7. Gọi Repository để lưu các thay đổi vào Database
            var updateGoiNap = await _goiNapRepository.UpdateAsync(goiNap);

            // 8. Trả về DTO chứa dữ liệu mới nhất sau khi cập nhật thành công
            return new GoiNapDto
            {
                GoiNapId = updateGoiNap.GoiNapId,
                GameId = updateGoiNap.GameId,
                TenGoiNap = updateGoiNap.TenGoiNap,
                GiaGoc = updateGoiNap.GiaGoc,
                TrangThai = updateGoiNap.TrangThai,
                NguoiTaoID = updateGoiNap.NguoiTaoId,
                NguoiCapNhatID = updateGoiNap.NguoiCapNhatId,
                NgayTao = updateGoiNap.NgayTao,
                NgayCapNhat = updateGoiNap.NgayCapNhat
            };
        }
    }
}