using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Sql;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Core.Linq.Specification;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write
{
    public class AsyncWriteRepository<TUnitOfWork, TEntity, TKey> : WriteRepository<TUnitOfWork, TEntity, TKey>, IAsyncWriteRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        public AsyncWriteRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<int> DeleteManyAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await GetSet().Where(filter).DeleteAsync();
        }

        public async Task<int> DeleteManyAsync(ISpecification<TEntity> specification)
        {
            return await DeleteManyAsync(specification.SatisfiedBy());
        }

        public async Task<int> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return await GetSet().Where(filter).UpdateAsync(updateFactory);
        }

        public async Task<int> UpdateManyAsync(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return await UpdateManyAsync(specification.SatisfiedBy(), updateFactory);
        }
    }
}
