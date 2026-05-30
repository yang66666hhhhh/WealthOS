using System.Data;
using Microsoft.Data.Sqlite;
using WealthOS.Application.Interfaces;

namespace WealthOS.Infrastructure.Data;

public class SqliteDbContext : IDbContext
{
    private readonly string _connectionString;

    public SqliteDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
