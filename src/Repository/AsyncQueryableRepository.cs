using System.Diagnostics.CodeAnalysis;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;

namespace eQuantic.Core.Data.EntityFramework.Repository;

[ExcludeFromCodeCoverage]
public class AsyncQueryableRepository<TUnitOfWork, TEntity, TKey> :
    AsyncRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>,
    IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public AsyncQueryableRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
