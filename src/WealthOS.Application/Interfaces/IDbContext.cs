using System.Data;

namespace WealthOS.Application.Interfaces;

public interface IDbContext
{
    IDbConnection CreateConnection();
}
