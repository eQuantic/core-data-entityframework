using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Linq.Sorter;
using eQuantic.Core.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository
{
    public class RelationalRepository<TUnitOfWork, TEntity, TKey> : Repository<TUnitOfWork, TEntity, TKey>, IRelationalRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        private readonly IRelationalReadRepository<TUnitOfWork, TEntity, TKey> readSpecRepository;

        public RelationalRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
        {
            this.readSpecRepository = new RelationalReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params string[] loadProperties)
        {
            return this.readSpecRepository.AllMatching(specification, loadProperties);
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.AllMatching(specification, loadProperties);
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.AllMatching(specification, sortingColumns, loadProperties);
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.AllMatching(specification, sortingColumns, loadProperties);
        }

        public TEntity Get(TKey id, params string[] loadProperties)
        {
            return this.readSpecRepository.Get(id, loadProperties);
        }

        public TEntity Get(TKey id, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.Get(id, loadProperties);
        }

        public IEnumerable<TEntity> GetAll(params string[] loadProperties)
        {
            return this.readSpecRepository.GetAll(loadProperties);
        }

        public IEnumerable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetAll(loadProperties);
        }

        public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetAll(sortingColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetAll(sortingColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFiltered(filter, loadProperties);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFiltered(filter, loadProperties);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFiltered(filter, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFiltered(filter, sortColumns, loadProperties);
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(filter, loadProperties);
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(filter, loadProperties);
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(filter, sortingColumns, loadProperties);
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(filter, sortingColumns, loadProperties);
        }

        public TEntity GetFirst(ISpecification<TEntity> specification, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(specification, loadProperties);
        }

        public TEntity GetFirst(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(specification, loadProperties);
        }

        public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(specification, sortingColumns, loadProperties);
        }

        public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetFirst(specification, sortingColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(specification, limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(specification, limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(filter, limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(filter, limit, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(pageIndex, pageCount, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(pageIndex, pageCount, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(specification, pageIndex, pageCount, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(specification, pageIndex, pageCount, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(filter, pageIndex, pageCount, sortColumns, loadProperties);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetPaged(filter, pageIndex, pageCount, sortColumns, loadProperties);
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(filter, loadProperties);
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(filter, loadProperties);
        }

        public TEntity GetSingle(ISpecification<TEntity> specification, params string[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(specification, loadProperties);
        }

        public TEntity GetSingle(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(specification, loadProperties);
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(filter, sortingColumns, loadProperties);
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(filter, sortingColumns, loadProperties);
        }

        public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(specification, sortingColumns, loadProperties);
        }

        public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
        {
            return this.readSpecRepository.GetSingle(specification, sortingColumns, loadProperties);
        }
    }
}