using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class AsyncRelationalRepository<TUnitOfWork, TEntity, TKey> : AsyncRepository<TUnitOfWork, TEntity, TKey>, IAsyncRelationalRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    protected readonly IAsyncRelationalReadRepository<TUnitOfWork, TEntity, TKey> asyncReadSpecRepository;

    public AsyncRelationalRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this.asyncReadSpecRepository = new AsyncRelationalReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.AllMatchingAsync(specification, loadProperties);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.AllMatchingAsync(specification, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAllAsync(loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAllAsync(loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAllAsync(sortingColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAllAsync(sortingColumns, loadProperties);
    }

    public Task<TEntity> GetAsync(TKey id, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAsync(id, loadProperties);
    }

    public Task<TEntity> GetAsync(TKey id, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetAsync(id, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFilteredAsync(filter, sortColumns, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(filter, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetFirstAsync(specification, sortingColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(specification, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetPagedAsync(filter, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(filter, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, sortingColumns, loadProperties);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return this.asyncReadSpecRepository.GetSingleAsync(specification, sortingColumns, loadProperties);
    }
}