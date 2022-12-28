using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class AsyncQueryableRepository<TUnitOfWork, TEntity, TKey> : AsyncRepository<TUnitOfWork, TEntity, TKey>, IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    protected readonly IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> asyncReadSpecRepository;

    public AsyncQueryableRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this.asyncReadSpecRepository = new AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.AllMatchingAsync(specification, configuration);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetAllAsync(configuration);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetAllAsync(sortingColumns, configuration);
    }

    public Task<TEntity> GetAsync(TKey id, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetAsync(id, configuration);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, configuration);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, sortColumns, configuration);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, configuration);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, sortingColumns, configuration);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, configuration);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, sortingColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(pageIndex, pageCount, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, pageIndex, pageCount, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, pageIndex, pageCount, sortColumns, configuration);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, configuration);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, sortingColumns, configuration);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, configuration);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, sortingColumns, configuration);
    }
}