using System;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Repository;

public class DefaultUnitOfWork(IServiceProvider serviceProvider, DbContext context)
    : UnitOfWork<DbContext>(serviceProvider, context);
