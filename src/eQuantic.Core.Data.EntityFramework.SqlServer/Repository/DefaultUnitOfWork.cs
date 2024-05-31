using System;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Repository;

public class DefaultUnitOfWork : UnitOfWork<DbContext>
{
    public DefaultUnitOfWork(IServiceProvider serviceProvider, DbContext context) : base(serviceProvider, context)
    {
    }
}
