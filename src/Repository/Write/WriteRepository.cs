using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Sql;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Core.Linq.Specification;
using System;
using System.Linq;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write
{
    public class WriteRepository<TUnitOfWork, TEntity, TKey> : IWriteRepository<TUnitOfWork, TEntity, TKey>
        where TUnitOfWork : IQueryableUnitOfWork
        where TEntity : class, IEntity, new()
    {
        public TUnitOfWork UnitOfWork { get; set; }

        public WriteRepository(TUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));

            UnitOfWork = unitOfWork;
        }

        public void Add(TEntity item)
        {
            if (item != (TEntity)null)
                GetSet().Add(item);
        }

        public int DeleteMany(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().Where(filter).Delete();
        }

        public int DeleteMany(ISpecification<TEntity> specification)
        {
            return DeleteMany(specification.SatisfiedBy());
        }

        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            UnitOfWork?.Dispose();
        }

        public void Merge(TEntity persisted, TEntity current)
        {
            UnitOfWork.ApplyCurrentValues(persisted, current);
        }

        public void Modify(TEntity item)
        {
            if (item != (TEntity)null)
                UnitOfWork.SetModified(item);
        }

        public void Remove(TEntity item)
        {
            if (item != (TEntity)null)
            {
                //attach item if not exist
                UnitOfWork.Attach(item);

                //set as "removed"
                GetSet().Remove(item);
            }
        }

        public void TrackItem(TEntity item)
        {
            if (item != (TEntity)null)
                UnitOfWork.Attach<TEntity>(item);
        }

        public int UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return GetSet().Where(filter).Update(updateFactory);
        }

        public int UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
        {
            return UpdateMany(specification.SatisfiedBy(), updateFactory);
        }

        private Set<TEntity> _dbset = null;

        protected Set<TEntity> GetSet()
        {
            return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
        }
    }
}
