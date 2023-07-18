using System.Diagnostics.CodeAnalysis;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

[ExcludeFromCodeCoverage]
public class AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> :
    AsyncReadRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>,
    IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : class, IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public AsyncQueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
