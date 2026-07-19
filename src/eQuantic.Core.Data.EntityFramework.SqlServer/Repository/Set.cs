using eQuantic.Core.Data.EntityFramework.Relational.Repository;
using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Repository;

/// <summary>
///     SQL Server entity set. The implementation lives in <see cref="RelationalSet{TEntity}" />; this
///     type is preserved for source compatibility.
/// </summary>
public class Set<TEntity> : RelationalSet<TEntity> where TEntity : class, IEntity, new()
{
    public Set(DbContext context) : base(context)
    {
    }
}
