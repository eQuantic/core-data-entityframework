using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write;

public class AsyncWriteRepository<TEntity> : WriteRepository<TEntity>,
    IAsyncWriteRepository<TEntity>
    where TEntity : class, IEntity
{
    public AsyncWriteRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task AddAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return GetSet().InsertAsync(item, cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return GetSet().AddRangeAsync(items, cancellationToken);
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
}
