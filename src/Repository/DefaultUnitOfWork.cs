using System;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Sql;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class DefaultUnitOfWork : UnitOfWork<DbContext>
{
    public DefaultUnitOfWork(IServiceProvider serviceProvider, DbContext context) : base(serviceProvider, context)
    {
    }
}
