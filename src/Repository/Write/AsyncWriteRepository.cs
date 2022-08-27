using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write;

public class AsyncWriteRepository<TUnitOfWork, TEntity, TKey> : WriteRepository<TUnitOfWork, TEntity, TKey>, IAsyncWriteRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public AsyncWriteRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task AddAsync(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return GetSet().AddAsync(item);
    }

    public Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return GetSet().DeleteManyAsync(filter);
    }

    public Task<long> DeleteManyAsync(ISpecification<TEntity> specification)
    {
        return DeleteManyAsync(specification.SatisfiedBy());
    }

    public Task MergeAsync(TEntity persisted, TEntity current)
    {
        this.Merge(persisted, current);
        return Task.CompletedTask;
    }

    public Task ModifyAsync(TEntity item)
    {
        this.Modify(item);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity item)
    {
        this.Remove(item);
        return Task.CompletedTask;
    }

    public Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (updateFactory == null)
        {
            throw new ArgumentNullException(nameof(updateFactory));
        }

        return GetSet().UpdateManyAsync(filter, updateFactory);
    }

    public Task<long> UpdateManyAsync(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return this.UpdateManyAsync(specification.SatisfiedBy(), updateFactory);
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}