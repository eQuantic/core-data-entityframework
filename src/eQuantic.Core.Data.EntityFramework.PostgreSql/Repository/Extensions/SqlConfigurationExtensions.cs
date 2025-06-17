using eQuantic.Core.Data.Repository.Config;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Repository.Extensions;

public static class SqlConfigurationExtensions
{
    private const int DefaultCommandTimeout = 60;
    
    public static int GetCommandTimeout(this SqlConfiguration config, DbContext context)
    {
        return config?.CommandTimeout ?? context?.Database.GetCommandTimeout() ?? DefaultCommandTimeout;
    }
}
