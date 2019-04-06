using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.Data.EntityFramework.Repository
{
    public class Set<TEntity> : Data.Repository.ISet<TEntity>
        where TEntity : class, IEntity, new()
    {
        private readonly DbSet<TEntity> internalDbSet;

        public Set(DbContext context)
        {
            this.internalDbSet = context.Set<TEntity>();
        }

        public EntityEntry<TEntity> Add(TEntity entity)
        {
            return this.internalDbSet.Add(entity);
        }

        public Task<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return this.internalDbSet.AddAsync(entity, cancellationToken);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            this.internalDbSet.AddRange(entities);
        }

        public void AddRange(params TEntity[] entities)
        {
            this.internalDbSet.AddRange(entities);
        }

        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return this.internalDbSet.AddRangeAsync(entities, cancellationToken);
        }

        public Task AddRangeAsync(params TEntity[] entities)
        {
            return this.internalDbSet.AddRangeAsync(entities);
        }

        public EntityEntry<TEntity> Attach(TEntity entity)
        {
            return this.internalDbSet.Attach(entity);
        }

        public void AttachRange(IEnumerable<TEntity> entities)
        {
            this.internalDbSet.AttachRange(entities);
        }

        public void AttachRange(params TEntity[] entities)
        {
            this.internalDbSet.AttachRange(entities);
        }

        public override bool Equals(object obj)
        {
            return this.internalDbSet.Equals(obj);
        }

        public TEntity Find(params object[] keyValues)
        {
            return this.internalDbSet.Find(keyValues);
        }

        public Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return this.internalDbSet.FindAsync(keyValues, cancellationToken);
        }

        public Task<TEntity> FindAsync(params object[] keyValues)
        {
            return this.internalDbSet.FindAsync(keyValues);
        }

        public override int GetHashCode()
        {
            return this.internalDbSet.GetHashCode();
        }

        public LocalView<TEntity> Local => this.internalDbSet.Local;

        public Expression Expression => ((IQueryable<TEntity>)this.internalDbSet).Expression;

        public Type ElementType => ((IQueryable<TEntity>)this.internalDbSet).ElementType;

        public IQueryProvider Provider => ((IQueryable<TEntity>)this.internalDbSet).Provider;

        public EntityEntry<TEntity> Remove(TEntity entity)
        {
            return this.internalDbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            this.internalDbSet.RemoveRange(entities);
        }

        public void RemoveRange(params TEntity[] entities)
        {
            this.internalDbSet.RemoveRange(entities);
        }

        public override string ToString()
        {
            return this.internalDbSet.ToString();
        }

        public EntityEntry<TEntity> Update(TEntity entity)
        {
            return this.internalDbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            this.internalDbSet.UpdateRange(entities);
        }

        public void UpdateRange(params TEntity[] entities)
        {
            this.internalDbSet.UpdateRange(entities);
        }

        public IEnumerable<TEntity> Execute()
        {
            return this.internalDbSet.ToList();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IQueryable<TEntity>)this.internalDbSet).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IQueryable<TEntity>)this.internalDbSet).GetEnumerator();
        }
    }
}
