using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Services
{
    /// <summary>
    /// Service xử lý toàn bộ logic nghiệp vụ liên quan đến Game.
    /// Đóng vai trò làm "Chủ thầu" nhận yêu cầu từ Controller, xử lý nghiệp vụ 
    /// và điều phối "Thợ" Repository làm việc với Database.
    /// </summary>
    public class GameService : IGameService
    {
        // Khai báo các phụ thuộc (Dependencies) cần dùng trong Service
        private readonly IGameRepository _gameRepository;       // Thợ tương tác với DB cho bảng Game
        private readonly ICurrentUserService _currentUserService; // Dịch vụ lấy thông tin người dùng đang đăng nhập qua JWT Token

        // Inject các phụ thuộc thông qua Constructor (Dependency Injection)
        public GameService(IGameRepository gameRepository, ICurrentUserService currentUserService)
        {
            _gameRepository = gameRepository;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các Game có trong hệ thống.
        /// </summary>
        public async Task<List<GameDto>> GetAllAsync()
        {
            // 1. Gọi Repository để lấy tất cả Entity Game từ Database
            var games = await _gameRepository.GetAllAsync();

            // 2. Chuyển đổi (Map) danh sách Entity Game sang danh sách GameDto để trả về cho Client
            return games.Select(x => new GameDto
            {
                GameId = x.GameId,
                TenGame = x.TenGame,
                TrangThaiGame = x.TrangThaiGame,
                CoServer = x.CoServer,
                NguoiTaoID = x.NguoiTaoId,
                NguoiCapNhatID = x.NguoiCapNhatId,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat
            }).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một Game theo Id.
        /// </summary>
        public async Task<GameDto?> GetByIdAsync(int id)
        {
            // 1. Tìm Entity Game theo Id trong DB
            var games = await _gameRepository.GetByIdAsync(id);

            // 2. Nếu không tìm thấy Game, trả về null
            if (games == null)
                return null;

            // 3. Nếu tìm thấy, ánh xạ (Map) sang GameDto và trả về
            return new GameDto
            {
                GameId = games.GameId,
                TenGame = games.TenGame,
                TrangThaiGame = games.TrangThaiGame,
                CoServer = games.CoServer,
                NguoiTaoID = games.NguoiTaoId,
                NguoiCapNhatID = games.NguoiCapNhatId,
                NgayTao = games.NgayTao,
                NgayCapNhat = games.NgayCapNhat
            };
        }

        /// <summary>
        /// Tạo mới một Game vào hệ thống.
        /// </summary>
        public async Task<GameDto> CreateAsync(CreateGameDto dto)
        {
            // 1. Khởi tạo đối tượng Entity Game từ DTO được gửi từ Client
            var games = new Game
            {
                TenGame = dto.TenGame,
                CoServer = dto.CoServer,
                TrangThaiGame = "HOAT_DONG", // Game mới tạo mặc định ở trạng thái HOAT_DONG
                NguoiTaoId = _currentUserService.AccountId, // Tự động lấy AccountId của Admin đang đăng nhập từ JWT Token
                NgayTao = DateTime.UtcNow       // Ghi nhận thời gian tạo
            };

            // 2. Gọi Repository để lưu Entity Game mới vào Database
            var createGame = await _gameRepository.CreateAsync(games);

            // 3. Trả về GameDto kèm các thông tin vừa được tạo thành công
            return new GameDto
            {
                GameId = createGame.GameId,
                TenGame = createGame.TenGame,
                TrangThaiGame = createGame.TrangThaiGame,
                CoServer = createGame.CoServer,
                NguoiTaoID = createGame.NguoiTaoId,
                NguoiCapNhatID = createGame.NguoiCapNhatId,
                NgayTao = createGame.NgayTao,
                NgayCapNhat = createGame.NgayCapNhat
            };
        }

        /// <summary>
        /// Cập nhật thông tin của một Game đã tồn tại.
        /// </summary>
        public async Task<GameDto?> UpdateAsync(int id, UpdateGameDto dto)
        {
            // 1. Tìm Game cần cập nhật trong Database
            var game = await _gameRepository.GetByIdAsync(id);

            // 2. Nếu Game không tồn tại, trả về null để Controller biết và báo lỗi 404
            if (game == null)
                return null;

            // 3. Cập nhật các trường thông tin mới từ DTO vào Entity
            game.TenGame = dto.TenGame;
            game.TrangThaiGame = dto.TrangThaiGame; // Ví dụ: Có thể đổi thành BAO_TRI hoặc NGUNG_HOAT_DONG
            game.CoServer = dto.CoServer;

            // 4. Ghi nhận vết hệ thống (Audit field): Ai là người sửa và sửa vào lúc nào
            game.NguoiCapNhatId = _currentUserService.AccountId; // Tự động lấy AccountId của Admin thực hiện sửa
            game.NgayCapNhat = DateTime.UtcNow;                  // Ghi nhận thời điểm cập nhật

            // 5. Gọi Repository lưu sự thay đổi vào Database
            var updatedGame = await _gameRepository.UpdateAsync(game);

            // 6. Ánh xạ dữ liệu đã cập nhật sang GameDto và trả về
            return new GameDto
            {
                GameId = updatedGame.GameId,
                TenGame = updatedGame.TenGame,
                TrangThaiGame = updatedGame.TrangThaiGame,
                CoServer = updatedGame.CoServer,
                NguoiTaoID = updatedGame.NguoiTaoId,
                NguoiCapNhatID = updatedGame.NguoiCapNhatId,
                NgayTao = updatedGame.NgayTao,
                NgayCapNhat = updatedGame.NgayCapNhat
            };
        }
    }
}