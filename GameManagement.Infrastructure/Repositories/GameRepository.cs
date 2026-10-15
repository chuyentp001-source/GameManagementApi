
using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;
using GameManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public GameRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResultDto<Game>> GetAllAsync(
        string? keyword,
        GameStatus? trangThai,
        int pageNumber,
        int pageSize)
    {
        var games = new List<Game>();
        int totalRecords = 0;

        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.sp_Game_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200).Value =
            string.IsNullOrWhiteSpace(keyword)
                ? DBNull.Value
                : keyword.Trim();

        command.Parameters.Add("@TrangThaiGame", SqlDbType.VarChar, 30).Value =
            trangThai.HasValue
                ? trangThai.Value.ToString()
                : DBNull.Value;

        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        // Result set 1: Danh sách Game
        while (await reader.ReadAsync())
        {
            games.Add(MapGame(reader));
        }

        // Result set 2: Tổng số bản ghi
        if (await reader.NextResultAsync())
        {
            if (await reader.ReadAsync())
            {
                totalRecords = reader.GetInt32(
                    reader.GetOrdinal("TotalRecords"));
            }
        }

        return new PagedResultDto<Game>
        {
            Items = games,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0
                ? 0
                : (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.sp_Game_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@GameID", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapGame(reader);
        }

        return null;
    }

    public async Task<Game> CreateAsync(Game game)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("dbo.sp_Game_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@TenGame", SqlDbType.NVarChar, 200).Value = game.TenGame;

            command.Parameters.Add(
                "@CoServer", SqlDbType.Bit).Value = game.CoServer;

            command.Parameters.Add(
                "@NguoiTaoID", SqlDbType.Int).Value =
                game.NguoiTaoId.HasValue
                    ? game.NguoiTaoId.Value
                    : DBNull.Value;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapGame(reader);
            }

            throw new InvalidOperationException(
                "Không nhận được dữ liệu Game sau khi tạo.");
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new DuplicateException("Tên Game này đã tồn tại.");
        }
    }

    public async Task<Game> UpdateAsync(Game game)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "dbo.sp_Game_Update", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@GameID", SqlDbType.Int).Value = game.GameId;

            command.Parameters.Add(
                "@TenGame", SqlDbType.NVarChar, 200).Value = game.TenGame;

            command.Parameters.Add(
                "@TrangThaiGame", SqlDbType.VarChar, 30).Value =
                game.TrangThaiGame.ToString();

            command.Parameters.Add(
                "@CoServer", SqlDbType.Bit).Value = game.CoServer;

            command.Parameters.Add(
                "@NguoiCapNhatID", SqlDbType.Int).Value =
                game.NguoiCapNhatId.HasValue
                    ? game.NguoiCapNhatId.Value
                    : DBNull.Value;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapGame(reader);
            }

            throw new KeyNotFoundException(
                $"Không tìm thấy Game có ID = {game.GameId}.");
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            throw new DuplicateException("Tên Game này đã tồn tại.");
        }
    }
    public async Task<Game?> UpdateStatusAsync(
    int id,
    GameStatus trangThai,
    int? nguoiCapNhatId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_Game_UpdateStatus", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@GameID", SqlDbType.Int).Value = id;

        command.Parameters.Add("@TrangThaiGame", SqlDbType.VarChar, 30)
            .Value = trangThai.ToString();

        command.Parameters.Add("@NguoiCapNhatID", SqlDbType.Int)
            .Value = (object?)nguoiCapNhatId ?? DBNull.Value;

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapGame(reader);
    }
    public async Task<bool> DeleteAsync(int id, int? nguoiCapNhatId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_Game_Delete", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@GameID", SqlDbType.Int).Value = id;

        command.Parameters.Add("@NguoiCapNhatID", SqlDbType.Int)
            .Value = (object?)nguoiCapNhatId ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync();

        return result != null
            && Convert.ToInt32(result) == 1;
    }
    private static Game MapGame(SqlDataReader reader)
    {
        return new Game
        {
            GameId = reader.GetInt32(reader.GetOrdinal("GameID")),
            TenGame = reader.GetString(reader.GetOrdinal("TenGame")),
            TrangThaiGame = Enum.Parse<GameStatus>(reader.GetString(reader.GetOrdinal("TrangThaiGame"))),
            CoServer = reader.GetBoolean(reader.GetOrdinal("CoServer")),

            NguoiTaoId = reader.IsDBNull(reader.GetOrdinal("NguoiTaoID"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiTaoID")),

            NguoiCapNhatId = reader.IsDBNull(reader.GetOrdinal("NguoiCapNhatID"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiCapNhatID")),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
            NgayCapNhat = reader.GetDateTime(
                reader.GetOrdinal("NgayCapNhat"))
        };
    }
}
