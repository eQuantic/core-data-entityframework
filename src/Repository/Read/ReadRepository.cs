using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Sql;
using eQuantic.Core.Linq;
using eQuantic.Core.Linq.Extensions;
using eQuantic.Core.Linq.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read
{
    public class ReadRepository<TUnitOfWork, TEntity, TKey> : IReadRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        /// <summary>
        /// <see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/>
        /// </summary>
        public TUnitOfWork UnitOfWork { get; private set; }

        /// <summary>
        /// Create a new instance of repository
        /// </summary>
        /// <param name="unitOfWork">Associated Unit Of Work</param>
        public ReadRepository(TUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));

            UnitOfWork = unitOfWork;
        }

        public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification)
        {
            return GetSet().Where(specification.SatisfiedBy());
        }

        /// <summary>
        /// <see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/>
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return GetSet().LongCount();
        }

        /// <summary>
        /// <see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/>
        /// </summary>
        /// <param name="specification"><see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/></param>
        /// <returns></returns>
        public long Count(ISpecification<TEntity> specification)
        {
            return GetSet().LongCount(specification.SatisfiedBy());
        }

        /// <summary>
        /// <see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/>
        /// </summary>
        /// <param name="filter"><see cref="eQuantic.Core.Data.Repository.Read.IReadRepository{TUnitOfWork, TEntity, TKey}"/></param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().LongCount(filter);
        }

        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            UnitOfWork?.Dispose();
        }

        public TEntity Get(TKey id)
        {
            return id != null ? GetSet().Find(id) : null;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return GetSet();
        }

        public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns)
        {
            return GetSet().OrderBy(sortingColumns);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
        {
            return GetFiltered(filter, null);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns)
        {
            if (filter == null)
                throw new ArgumentException("Filter expression cannot be null", nameof(filter));

            var query = GetSet().Where(filter);
            if (sortColumns != null && sortColumns.Length > 0)
            {
                query = query.OrderBy(sortColumns);
            }
            return query;
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().FirstOrDefault(filter);
        }

        public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns)
        {
            return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns)
        {
            return GetPaged(specification.SatisfiedBy(), 1, limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns)
        {
            return GetPaged(filter, 1, limit, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            return GetPaged(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns);
        }

        public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            IQueryable<TEntity> query = GetSet();
            if(filter != null) query = query.Where(filter);

            if (sortColumns != null && sortColumns.Length > 0)
            {
                query = query.OrderBy(sortColumns);
            }
            if (pageCount > 0)
                return query.Skip((pageIndex - 1) * pageCount).Take(pageCount);

            return query;
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().SingleOrDefault(filter);
        }

        private Set<TEntity> _dbset = null;

        protected Set<TEntity> GetSet()
        {
            return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
        }
    }
}
