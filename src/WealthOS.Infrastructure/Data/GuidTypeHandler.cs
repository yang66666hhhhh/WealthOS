using System.Data;
using Dapper;

namespace WealthOS.Infrastructure.Data;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }

    public override Guid Parse(object value)
    {
        return value switch
        {
            Guid guid => guid,
            string s => Guid.Parse(s),
            byte[] bytes => new Guid(bytes),
            _ => throw new ArgumentException($"Cannot convert {value.GetType()} to Guid")
        };
    }
}
