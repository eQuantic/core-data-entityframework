using System.Diagnostics.CodeAnalysis;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

[ExcludeFromCodeCoverage]
public class QueryableReadRepository<TUnitOfWork, TEntity, TKey> :
    ReadRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>,
    IQueryableReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public QueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
