using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace eQuantic.Core.Data.EntityFramework.Repository
{
    public abstract class UnitOfWork : IQueryableUnitOfWork
    {
        private static int IsMigrating = 0;
        private readonly DbContext _context;
        private IDbContextTransaction _transaction;
        private bool disposed = false;

        protected UnitOfWork(DbContext context)
        {
            _context = context;
        }

        public void BeginTransaction()
        {
            _transaction?.Dispose();
            _transaction = _context.Database.BeginTransaction();
        }

        public int Commit()
        {
            var affectedRecords = _context.SaveChanges();

            _transaction?.Commit();

            return affectedRecords;
        }

        public int CommitAndRefreshChanges()
        {
            int changes = 0;
            bool saveFailed = false;

            do
            {
                try
                {
                    changes = _context.SaveChanges();

                    saveFailed = false;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    ex.Entries.ToList()
                              .ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
                }
            } while (saveFailed);

            return changes;
        }

        public async Task<int> CommitAndRefreshChangesAsync()
        {
            int changes = 0;
            bool saveFailed = false;

            do
            {
                try
                {
                    changes = await _context.SaveChangesAsync();

                    saveFailed = false;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    ex.Entries.ToList()
                        .ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
                }
            } while (saveFailed);

            return changes;
        }

        public async Task<int> CommitAsync()
        {
            var affectedRecords = await _context.SaveChangesAsync();

            _transaction?.Commit();

            return affectedRecords;
        }

        Data.Repository.ISet<TEntity> IQueryableUnitOfWork.CreateSet<TEntity>() => InternalCreateSet<TEntity>();

        public virtual Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity, new() => InternalCreateSet<TEntity>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<string> GetPendingMigrations()
        {
            return _context.Database.GetPendingMigrations();
        }

        public abstract TRepository GetRepository<TRepository>() where TRepository : IRepository;

        public abstract TRepository GetRepository<TRepository>(string name) where TRepository : IRepository;

        public void LoadCollection<TEntity, TElement>(TEntity item, Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty, Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
        {
            if (filter != null)
            {
                _context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).Load();
            }
            else
            {
                _context.Entry<TEntity>(item).Collection(navigationProperty).Load();
            }
        }

        public async Task LoadCollectionAsync<TEntity, TElement>(TEntity item, Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty, Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
        {
            if (filter != null)
            {
                await _context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).LoadAsync();
            }
            else
            {
                await _context.Entry<TEntity>(item).Collection(navigationProperty).LoadAsync();
            }
        }

        public void Reload<TEntity>(TEntity item) where TEntity : class
        {
            var entry = _context.Entry(item);
            entry.CurrentValues.SetValues(entry.OriginalValues);
            entry.Reload();
        }

        public void RollbackChanges()
        {
            // set all entities in change tracker
            // as 'unchanged state'
            _context?.ChangeTracker.Entries()
                .ToList()
                .ForEach(entry => entry.State = EntityState.Unchanged);

            _transaction?.Rollback();
        }

        public void SetModified<TEntity>(TEntity item) where TEntity : class
        {
            //this operation also attach item in object state manager
            _context.Entry<TEntity>(item).State = EntityState.Modified;
        }

        public void UpdateDatabase()
        {
            if (0 == Interlocked.Exchange(ref IsMigrating, 1))
            {
                try
                {
                    _context.Database.Migrate();
                }
                finally
                {
                    Interlocked.Exchange(ref IsMigrating, 0);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _transaction?.Dispose();
                _context?.Dispose();
            }

            disposed = true;
        }

        private Data.Repository.ISet<TEntity> InternalCreateSet<TEntity>() where TEntity : class, IEntity, new() => new Set<TEntity>(_context);
    }
}