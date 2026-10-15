using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GameManagement.Infrastructure.Data;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Không tìm thấy chuỗi kết nối DefaultConnection.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
