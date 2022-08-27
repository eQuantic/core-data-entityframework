using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class AsyncRepository<TUnitOfWork, TEntity, TKey> : Repository<TUnitOfWork, TEntity, TKey>, IAsyncRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    protected readonly IAsyncReadRepository<TUnitOfWork, TEntity, TKey> asyncReadRepository;
    protected readonly IAsyncWriteRepository<TUnitOfWork, TEntity, TKey> asyncWriteRepository;
    private bool disposed = false;

    public AsyncRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this.asyncReadRepository = new AsyncReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
        this.asyncWriteRepository = new AsyncWriteRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
    }

    public Task AddAsync(TEntity item)
    {
        return this.asyncWriteRepository.AddAsync(item);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification)
    {
        return this.asyncReadRepository.AllMatchingAsync(specification);
    }

    public Task<long> CountAsync()
    {
        return this.asyncReadRepository.CountAsync();
    }

    public Task<long> CountAsync(ISpecification<TEntity> specification)
    {
        return this.asyncReadRepository.CountAsync(specification);
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
    {
        return this.asyncReadRepository.CountAsync(filter);
    }

    public Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter)
    {
        return this.asyncWriteRepository.DeleteManyAsync(filter);
    }

    public Task<long> DeleteManyAsync(ISpecification<TEntity> specification)
    {
        return this.asyncWriteRepository.DeleteManyAsync(specification);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(params ISorting[] sortingColumns)
    {
        return this.asyncReadRepository.GetAllAsync(sortingColumns);
    }

    public Task<TEntity> GetAsync(TKey id)
    {
        return this.asyncReadRepository.GetAsync(id);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetFilteredAsync(filter, sortColumns);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortingColumns)
    {
        return this.asyncReadRepository.GetFirstAsync(filter, sortingColumns);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
    {
        return this.asyncReadRepository.GetFirstAsync(specification, sortingColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(specification, limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(filter, limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(pageIndex, pageCount, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(specification, pageIndex, pageCount, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return this.asyncReadRepository.GetPagedAsync(filter, pageIndex, pageCount, sortColumns);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortingColumns)
    {
        return this.asyncReadRepository.GetSingleAsync(filter, sortingColumns);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
    {
        return this.asyncReadRepository.GetSingleAsync(specification, sortingColumns);
    }

    public Task MergeAsync(TEntity persisted, TEntity current)
    {
        return this.asyncWriteRepository.MergeAsync(persisted, current);
    }

    public Task ModifyAsync(TEntity item)
    {
        return this.asyncWriteRepository.ModifyAsync(item);
    }

    public Task RemoveAsync(TEntity item)
    {
        return this.asyncWriteRepository.RemoveAsync(item);
    }

    public Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return this.asyncWriteRepository.UpdateManyAsync(filter, updateFactory);
    }

    public Task<long> UpdateManyAsync(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return this.asyncWriteRepository.UpdateManyAsync(specification, updateFactory);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            this.asyncReadRepository?.Dispose();
            this.asyncWriteRepository?.Dispose();
            UnitOfWork?.Dispose();
        }

        disposed = true;
    }
}