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

    public IDbTransaction BeginTransaction()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return new ConnectionOwningTransaction(connection.BeginTransaction(), connection);
    }
}

internal sealed class ConnectionOwningTransaction : IDbTransaction
{
    private readonly IDbTransaction _inner;
    private readonly IDbConnection _connection;

    public ConnectionOwningTransaction(IDbTransaction inner, IDbConnection connection)
    {
        _inner = inner;
        _connection = connection;
    }

    public IDbConnection Connection => _inner.Connection;

    public IsolationLevel IsolationLevel => _inner.IsolationLevel;

    public void Commit() => _inner.Commit();

    public void Rollback() => _inner.Rollback();

    public void Dispose()
    {
        _inner.Dispose();
        _connection.Dispose();
    }
}
