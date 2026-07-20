using eQuantic.Core.Data.EntityFramework.Relational.Sql;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Relational.Repository.Extensions;

internal static class SqlConfigurationExtensions
{
    private const int DefaultCommandTimeout = 60;

    public static int GetCommandTimeout(this SqlConfiguration config, DbContext context)
    {
        return config?.CommandTimeout ?? context?.Database.GetCommandTimeout() ?? DefaultCommandTimeout;
    }
}
