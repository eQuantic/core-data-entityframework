using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class QueryableRepository<TUnitOfWork, TEntity, TKey> : Repository<TUnitOfWork, TEntity, TKey>, IQueryableRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private readonly IQueryableReadRepository<TUnitOfWork, TEntity, TKey> readSpecRepository;

    public QueryableRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this.readSpecRepository = new QueryableReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.AllMatching(specification, configuration);
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.AllMatching(specification, sortingColumns, configuration);
    }

    public TEntity Get(TKey id, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.Get(id, configuration);
    }

    public IEnumerable<TEntity> GetAll(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetAll(configuration);
    }

    public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetAll(sortingColumns, configuration);
    }
    
    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFiltered(filter, configuration);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFiltered(filter, sortColumns, configuration);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFirst(filter, configuration);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFirst(filter, sortingColumns, configuration);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFirst(specification, configuration);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetFirst(specification, sortingColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(specification, limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(filter, limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(pageIndex, pageCount, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(specification, pageIndex, pageCount, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetPaged(filter, pageIndex, pageCount, sortColumns, configuration);
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetSingle(filter, configuration);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetSingle(specification, configuration);
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetSingle(filter, sortingColumns, configuration);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return this.readSpecRepository.GetSingle(specification, sortingColumns, configuration);
    }
}