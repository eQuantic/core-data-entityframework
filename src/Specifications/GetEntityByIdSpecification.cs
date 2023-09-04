using System;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Specifications;

public class GetEntityByIdSpecification<TEntity, TKey> : Specification<TEntity>
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly TKey _id;
    private readonly UnitOfWork _unitOfWork;

    public GetEntityByIdSpecification(TKey id, UnitOfWork unitOfWork)
    {
        _id = id;
        _unitOfWork = unitOfWork;
    }
    public override Expression<Func<TEntity, bool>> SatisfiedBy()
    {
        return _unitOfWork.GetDbContext().GetFindByKeyExpression<TEntity, TKey>(_id);
    }
}