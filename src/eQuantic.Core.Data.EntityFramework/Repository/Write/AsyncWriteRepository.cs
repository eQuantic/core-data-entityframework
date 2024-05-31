using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write;

public class AsyncWriteRepository<TUnitOfWork, TEntity> : WriteRepository<TUnitOfWork, TEntity>,
    IAsyncWriteRepository<TUnitOfWork, TEntity>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private ISet<TEntity> _dbSet;

    public AsyncWriteRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task AddAsync(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return GetSet().InsertAsync(item);
    }

    public Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return GetSet().DeleteManyAsync(filter, cancellationToken);
    }

    public Task<long> DeleteManyAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return DeleteManyAsync(specification.SatisfiedBy(), cancellationToken);
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

    public Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (updateFactory == null)
        {
            throw new ArgumentNullException(nameof(updateFactory));
        }

        return GetSet().UpdateManyAsync(filter, updateFactory, cancellationToken);
    }

    public Task<long> UpdateManyAsync(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.UpdateManyAsync(specification.SatisfiedBy(), updateFactory, cancellationToken);
    }

    private ISet<TEntity> GetSet()
    {
        return _dbSet ??= UnitOfWork.CreateSet<TEntity>();
    }
}
