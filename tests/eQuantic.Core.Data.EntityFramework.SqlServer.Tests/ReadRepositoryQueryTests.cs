using System;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.SqlServer.Repository;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

/// <summary>
///     Integration coverage (EF Core InMemory) for the read-repository query fixes, ported to the v5
///     <see cref="QueryOptions{TEntity}" />-based read surface:
///     A1 — <c>All</c>/<c>Any</c> must honour the caller's query options.
///     A2 — <c>Get</c> must not reject a default-valued key (e.g. <c>0</c>) as a null argument.
/// </summary>
public class ReadRepositoryQueryTests
{
    private sealed class Product : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int GetKey() => Id;

        public void SetKey(int key) => Id = key;
    }

    private sealed class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
    }

    private sealed class NameSpecification(string name) : Specification<Product>
    {
        public override Expression<Func<Product, bool>> SatisfiedBy() => p => p.Name == name;
    }

    private static DefaultUnitOfWork NewUnitOfWork(out TestDbContext context)
    {
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase($"read-repo-{Guid.NewGuid():N}")
            .Options;
        context = new TestDbContext(options);
        return new DefaultUnitOfWork(new ServiceCollection().BuildServiceProvider(), context);
    }

    private static QueryableReadRepository<Product, int> NewRepository(out TestDbContext context)
        => new(NewUnitOfWork(out context));

    private static AsyncQueryableReadRepository<Product, int> NewAsyncRepository(out TestDbContext context)
        => new(NewUnitOfWork(out context));

    [Test]
    public void All_WithOptions_HonoursConfiguration()
    {
        var repository = NewRepository(out var context);
        context.Products.AddRange(
            new Product { Id = 1, Name = "active" },
            new Product { Id = 2, Name = "inactive" });
        context.SaveChanges();

        // The "inactive" row does not satisfy the predicate, so without the options being applied All()
        // would evaluate over both rows and return false. The options narrow the query to the "active"
        // row, so the fixed code returns true.
        var spec = new NameSpecification("active");
        var result = repository.All(
            spec.SatisfiedBy(),
            new QueryOptions<Product>().WithAfterCustomization(q => q.Where(p => p.Name == "active")));

        Assert.That(result, Is.True);
    }

    [Test]
    public void Any_WithOptions_HonoursConfiguration()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 1, Name = "active" });
        context.SaveChanges();

        // The options filter out every row, so Any() must return false once they are applied.
        var result = repository.Any(
            new QueryOptions<Product>()
                .Where(new NameSpecification("active"))
                .WithAfterCustomization(q => q.Where(_ => false)));

        Assert.That(result, Is.False);
    }

    [Test]
    public void Get_WithDefaultValuedKey_DoesNotThrow()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 1, Name = "active" });
        context.SaveChanges();

        // Key 0 is a valid (if absent) value for a value-typed key; it must not be treated as null.
        Product? found = null;
        Assert.DoesNotThrow(() => found = repository.Get(0));
        Assert.That(found, Is.Null);
    }

    [Test]
    public void Get_WithExistingKey_ReturnsEntity()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 5, Name = "found" });
        context.SaveChanges();

        var found = repository.Get(5);

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Name, Is.EqualTo("found"));
    }

    [Test]
    public void Get_WithOptions_UsesKeyExpression_ReturnsEntity()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 7, Name = "seven" });
        context.SaveChanges();

        // Non-null options route Get through GetFindByKeyExpression instead of DbSet.Find.
        var found = repository.Get(7, new QueryOptions<Product>());

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Name, Is.EqualTo("seven"));
    }

    [Test]
    public void GetFindByKeyExpression_DoesNotEmbedKeyValueAsLiteral()
    {
        _ = NewUnitOfWork(out var context);

        var expression = context.GetFindByKeyExpression<Product, int>(5);

        Assert.That(expression, Is.Not.Null);
        var literalFinder = new ConstantValueFinder(5);
        literalFinder.Visit(expression);
        Assert.That(literalFinder.Found, Is.False,
            "The key value must be parameterized (held in a closure), not embedded as a literal constant.");
    }

    [Test]
    public async System.Threading.Tasks.Task GetAllAsync_ReturnsAllEntities()
    {
        var repository = NewAsyncRepository(out var context);
        context.Products.AddRange(
            new Product { Id = 1, Name = "a" },
            new Product { Id = 2, Name = "b" },
            new Product { Id = 3, Name = "c" });
        context.SaveChanges();

        var all = await repository.GetAllAsync();

        Assert.That(all.Count(), Is.EqualTo(3));
    }

    [Test]
    public void GetPaged_WithoutSorting_OrdersByPrimaryKeyDeterministically()
    {
        var repository = NewRepository(out var context);
        // Insert out of key order; without a fallback OrderBy the page order would be undefined.
        context.Products.AddRange(
            new Product { Id = 3, Name = "c" },
            new Product { Id = 1, Name = "a" },
            new Product { Id = 2, Name = "b" });
        context.SaveChanges();

        var firstPage = repository.GetPaged(PageRequest.Of(1, 2)).Items;

        Assert.That(firstPage.Select(p => p.Id), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void GetPaged_WithExplicitOrdering_IsPreserved()
    {
        var repository = NewRepository(out var context);
        context.Products.AddRange(
            new Product { Id = 1, Name = "a" },
            new Product { Id = 2, Name = "b" },
            new Product { Id = 3, Name = "c" });
        context.SaveChanges();

        // Caller orders descending; the primary-key fallback must NOT override it.
        var firstPage = repository
            .GetPaged(
                PageRequest.Of(1, 2),
                new QueryOptions<Product>().WithAfterCustomization(q => q.OrderByDescending(p => p.Id)))
            .Items;

        Assert.That(firstPage.Select(p => p.Id), Is.EqualTo(new[] { 3, 2 }));
    }

    [Test]
    public void OrderByPrimaryKeyIfUnordered_AlreadyOrdered_ReturnsSameQuery()
    {
        _ = NewUnitOfWork(out var context);
        var ordered = context.Set<Product>().OrderByDescending(p => p.Name);

        var result = ordered.OrderByPrimaryKeyIfUnordered(context);

        Assert.That(result, Is.SameAs(ordered));
    }

    private sealed class ConstantValueFinder(object target) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (Equals(node.Value, target))
            {
                Found = true;
            }

            return base.VisitConstant(node);
        }
    }
}
