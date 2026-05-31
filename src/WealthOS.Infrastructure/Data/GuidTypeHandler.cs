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
            _ => throw new ArgumentException($"Cannot convert {value?.GetType()} to Guid")
        };
    }
}

public class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    public override void SetValue(IDbDataParameter parameter, Guid? value)
    {
        parameter.Value = value?.ToString();
    }

    public override Guid? Parse(object value)
    {
        return value switch
        {
            null => null,
            Guid guid => guid,
            string s => string.IsNullOrEmpty(s) ? null : Guid.Parse(s),
            byte[] bytes => new Guid(bytes),
            _ => null
        };
    }
}

public class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
{
    public override void SetValue(IDbDataParameter parameter, decimal value)
    {
        parameter.Value = (double)value;
    }

    public override decimal Parse(object value)
    {
        return value switch
        {
            decimal d => d,
            double d => (decimal)d,
            float f => (decimal)f,
            int i => i,
            long l => l,
            string s => decimal.Parse(s),
            _ => 0m
        };
    }
}
