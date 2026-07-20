using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

[ExcludeFromCodeCoverage]
public class QueryableRepository<TEntity, TKey> :
    QueryableReadRepository<TEntity, TKey>,
    IQueryableRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly IWriteRepository<TEntity> _writeRepository;

    /// <summary>
    /// Create a new instance of repository
    /// </summary>
    /// <param name="unitOfWork">Associated Unit Of Work</param>
    public QueryableRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _writeRepository = new WriteRepository<TEntity>(unitOfWork);
    }

    public void Add(TEntity item)
    {
        _writeRepository.Add(item);
    }

    public void AddRange(IEnumerable<TEntity> items)
    {
        _writeRepository.AddRange(items);
    }

    public long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        return _writeRepository.DeleteMany(filter);
    }

    public long DeleteMany(ISpecification<TEntity> specification)
    {
        return _writeRepository.DeleteMany(specification);
    }

    public void Merge(TEntity persisted, TEntity current)
    {
        _writeRepository.Merge(persisted, current);
    }

    public void Modify(TEntity item)
    {
        _writeRepository.Modify(item);
    }

    public void Remove(TEntity item)
    {
        _writeRepository.Remove(item);
    }

    public void TrackItem(TEntity item)
    {
        _writeRepository.TrackItem(item);
    }

    public long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return _writeRepository.UpdateMany(filter, updateFactory);
    }

    public long UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return _writeRepository.UpdateMany(specification, updateFactory);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writeRepository?.Dispose();
        }

        base.Dispose(disposing);
    }
}
