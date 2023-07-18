using System.Diagnostics.CodeAnalysis;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;

namespace eQuantic.Core.Data.EntityFramework.Repository;

[ExcludeFromCodeCoverage]
public class QueryableRepository<TUnitOfWork, TEntity, TKey> :
    Repository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>,
    IQueryableRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public QueryableRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
