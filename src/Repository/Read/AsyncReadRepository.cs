using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Sql;
using eQuantic.Core.Linq;
using eQuantic.Core.Linq.Extensions;
using eQuantic.Core.Linq.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read
{
    public class AsyncReadRepository<TUnitOfWork, TEntity, TKey> : ReadRepository<TUnitOfWork, TEntity, TKey>, IAsyncReadRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        public AsyncReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification)
        {
            return await GetSet().Where(specification.SatisfiedBy()).ToListAsync();
        }

        public async Task<long> CountAsync()
        {
            return await GetSet().LongCountAsync();
        }

        public async Task<long> CountAsync(ISpecification<TEntity> specification)
        {
            return await GetSet().LongCountAsync(specification.SatisfiedBy());
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await GetSet().LongCountAsync(filter);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await GetSet().ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns)
        {
            return await GetSet().OrderBy(sortingColumns).ToListAsync();
        }

        public async  Task<TEntity> GetAsync(TKey id)
        {
            return id != null ? await GetSet().FindAsync(id) : null;
        }

        public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await GetSet().Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns)
        {
            return await GetSet().Where(filter).OrderBy(sortColumns).ToListAsync();
        }

        public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await GetSet().FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns)
        {
            return await GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns)
        {
            return await GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns)
        {
            return await GetPagedAsync(filter, 1, limit, sortColumns);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            return await GetPagedAsync((Expression<Func<TEntity, bool>>) null, pageIndex, pageCount, sortColumns);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            return await GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns)
        {
            IQueryable<TEntity> query = GetSet();
            if (filter != null) query = query.Where(filter);

            if (sortColumns != null && sortColumns.Length > 0)
            {
                query = query.OrderBy(sortColumns);
            }
            if (pageCount > 0)
                return await query.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToListAsync();

            return await query.ToListAsync();
        }

        public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().SingleOrDefaultAsync(filter);
        }

        public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns)
        {
            return GetSet().OrderBy(sortingColumns).SingleOrDefaultAsync(filter);
        }
    }
}
