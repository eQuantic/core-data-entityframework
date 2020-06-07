using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Core.Linq.Sorter;
using eQuantic.Core.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository
{
    public class Repository<TUnitOfWork, TEntity, TKey> : IRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        protected readonly IReadRepository<TUnitOfWork, TEntity, TKey> readRepository;
        protected readonly IWriteRepository<TUnitOfWork, TEntity, TKey> writeRepository;
        private bool disposed = false;

        /// <summary>
        /// Create a new instance of repository
        /// </summary>
        /// <param name="unitOfWork">Associated Unit Of Work</param>
        public Repository(TUnitOfWork unitOfWork)
        {
            this.UnitOfWork = unitOfWork;
            this.readRepository = new ReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
            this.writeRepository = new WriteRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
        }

        public TUnitOfWork UnitOfWork { get; private set; }

        public void Add(TEntity item)
        {
            this.writeRepository.Add(item);
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
        {
            return this.readRepository.AllMatching(specification, sortingColumns);
        }

        public long Count()
        {
            return this.readRepository.Count();
        }

        public long Count(ISpecification<TEntity> specification)
        {
            return this.readRepository.Count(specification);
        }

        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            return this.readRepository.Count(filter);
        }

        public long DeleteMany(Expression<Func<TEntity, bool>> filter)
        {
            return this.writeRepository.DeleteMany(filter);
        }

        public long DeleteMany(ISpecification<TEntity> specification)
        {
            return this.writeRepository.DeleteMany(specification);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TEntity Get(TKey id)
        {
            return this.readRepository.Get(id);
        }

        public IEnumerable<TEntity> GetAll(params ISorting[] sortingColumns)
        {
            return this.readRepository.GetAll(sortingColumns);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
        {
            return this.readRepository.GetFiltered(filter, sortColumns);
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
        {
            return this.readRepository.GetFirst(filter, sortColumns);
        }

        public TEntity GetFirst(ISpecification<TEntity> specification, params ISorting[] sortColumns)
        {
            return this.readRepository.GetFirst(specification, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(int limit, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(specification, limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(filter, limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(pageIndex, pageCount, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(specification, pageIndex, pageCount, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, params ISorting[] sortColumns)
        {
            return this.readRepository.GetPaged(filter, pageIndex, pageCount, sortColumns);
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
        {
            return this.readRepository.GetSingle(filter, sortColumns);
        }

        public TEntity GetSingle(ISpecification<TEntity> specification, params ISorting[] sortColumns)
        {
            return this.readRepository.GetSingle(specification, sortColumns);
        }

        public void Merge(TEntity persisted, TEntity current)
        {
            this.writeRepository.Merge(persisted, current);
        }

        public void Modify(TEntity item)
        {
            this.writeRepository.Modify(item);
        }

        public void Remove(TEntity item)
        {
            this.writeRepository.Remove(item);
        }

        public void TrackItem(TEntity item)
        {
            this.writeRepository.TrackItem(item);
        }

        public long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return this.writeRepository.UpdateMany(filter, updateFactory);
        }

        public long UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return this.writeRepository.UpdateMany(specification, updateFactory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                this.readRepository?.Dispose();
                this.writeRepository?.Dispose();
                UnitOfWork?.Dispose();
            }

            disposed = true;
        }
    }
}