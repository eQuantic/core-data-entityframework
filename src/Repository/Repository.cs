using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class Repository<TUnitOfWork, TConfig, TEntity, TKey> : IRepository<TUnitOfWork, TConfig, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
    where TConfig : Configuration<TEntity>
{
    private readonly IReadRepository<TUnitOfWork, TConfig, TEntity, TKey> _readRepository;
    private readonly IWriteRepository<TUnitOfWork, TEntity> _writeRepository;
    private bool _disposed;

    /// <summary>
    /// Create a new instance of repository
    /// </summary>
    /// <param name="unitOfWork">Associated Unit Of Work</param>
    public Repository(TUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
        this._readRepository = new ReadRepository<TUnitOfWork, TConfig, TEntity, TKey>(unitOfWork);
        this._writeRepository = new WriteRepository<TUnitOfWork, TEntity>(unitOfWork);
    }

    public TUnitOfWork UnitOfWork { get; private set; }

    public void Add(TEntity item)
    {
        this._writeRepository.Add(item);
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.AllMatching(specification, configuration);
    }

    public long Count()
    {
        return this._readRepository.Count();
    }

    public long Count(ISpecification<TEntity> specification)
    {
        return this._readRepository.Count(specification);
    }

    public long Count(Expression<Func<TEntity, bool>> filter)
    {
        return this._readRepository.Count(filter);
    }

    public bool All(ISpecification<TEntity> specification, Action<TConfig> configuration = default)
    {
        return this._readRepository.All(specification, configuration);
    }

    public bool All(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default)
    {
        return this._readRepository.All(filter, configuration);
    }

    public bool Any(Action<TConfig> configuration = default)
    {
        return this._readRepository.Any(configuration);
    }

    public bool Any(ISpecification<TEntity> specification, Action<TConfig> configuration = default)
    {
        return this._readRepository.Any(specification, configuration);
    }

    public bool Any(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default)
    {
        return this._readRepository.Any(filter, configuration);
    }

    public long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        return this._writeRepository.DeleteMany(filter);
    }

    public long DeleteMany(ISpecification<TEntity> specification)
    {
        return this._writeRepository.DeleteMany(specification);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public TEntity Get(TKey id, Action<TConfig> configuration = default)
    {
        return this._readRepository.Get(id, configuration);
    }

    public IEnumerable<TEntity> GetAll(Action<TConfig> configuration = default)
    {
        return this._readRepository.GetAll(configuration);
    }

    public IEnumerable<TResult> GetMapped<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetMapped(filter, map, configuration);
    }

    public IEnumerable<TResult> GetMapped<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetMapped(specification, map, configuration);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.GetFiltered(filter, configuration);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetFirst(filter, configuration);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetFirst(specification, configuration);
    }

    public TResult GetFirstMapped<TResult>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetFirstMapped(filter, map, configuration);
    }

    public TResult GetFirstMapped<TResult>(ISpecification<TEntity> specification, Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetFirstMapped(specification, map, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int limit, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(specification, limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(filter, limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageSize, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(pageIndex, pageSize, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageSize,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(specification, pageIndex, pageSize, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageSize,
        Action<TConfig> configuration = default)
    {
        return this._readRepository.GetPaged(filter, pageIndex, pageSize, configuration);
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetSingle(filter, configuration);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, Action<TConfig> configuration = default)
    {
        return this._readRepository.GetSingle(specification, configuration);
    }

    public void Merge(TEntity persisted, TEntity current)
    {
        this._writeRepository.Merge(persisted, current);
    }

    public void Modify(TEntity item)
    {
        this._writeRepository.Modify(item);
    }

    public void Remove(TEntity item)
    {
        this._writeRepository.Remove(item);
    }

    public void TrackItem(TEntity item)
    {
        this._writeRepository.TrackItem(item);
    }

    public long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return this._writeRepository.UpdateMany(filter, updateFactory);
    }

    public long UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return this._writeRepository.UpdateMany(specification, updateFactory);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            this._readRepository?.Dispose();
            this._writeRepository?.Dispose();
            UnitOfWork?.Dispose();
        }

        _disposed = true;
    }
}

public class Repository<TUnitOfWork, TEntity, TKey> : Repository<TUnitOfWork, Configuration<TEntity>, TEntity, TKey>,
    IRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public Repository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
