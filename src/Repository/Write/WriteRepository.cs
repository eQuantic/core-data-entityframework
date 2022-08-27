﻿using System;
using System.Linq.Expressions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Write;

public class WriteRepository<TUnitOfWork, TEntity, TKey> : IWriteRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;
    private bool disposed = false;

    public WriteRepository(TUnitOfWork unitOfWork)
    {
        if (unitOfWork == null)
        {
            throw new ArgumentNullException(nameof(unitOfWork));
        }

        UnitOfWork = unitOfWork;
    }

    public TUnitOfWork UnitOfWork { get; private set; }

    public void Add(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        GetSet().Add(item);
    }

    public long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return GetSet().DeleteMany(filter);
    }

    public long DeleteMany(ISpecification<TEntity> specification)
    {
        return DeleteMany(specification.SatisfiedBy());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Merge(TEntity persisted, TEntity current)
    {
        GetSet().ApplyCurrentValues(persisted, current);
    }

    public void Modify(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        GetSet().SetModified(item);
    }

    public void Remove(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        //attach item if not exist
        GetSet().Attach(item);

        //set as "removed"
        GetSet().Remove(item);
    }

    public void TrackItem(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        GetSet().Attach(item);
    }

    public long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (updateFactory == null)
        {
            throw new ArgumentNullException(nameof(updateFactory));
        }

        return GetSet().UpdateMany(filter, updateFactory);
    }

    public long UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return UpdateMany(specification.SatisfiedBy(), updateFactory);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            UnitOfWork?.Dispose();
        }

        disposed = true;
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}