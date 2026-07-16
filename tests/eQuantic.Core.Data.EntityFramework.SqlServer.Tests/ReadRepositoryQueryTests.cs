using System;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.SqlServer.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

/// <summary>
///     Integration coverage (EF Core InMemory) for the read-repository query fixes:
///     A1 — <c>All</c>/<c>Any</c> with a specification must honour the caller's configuration.
///     A2 — <c>Get</c> must not reject a default-valued key (e.g. <c>0</c>) as a null argument.
/// </summary>
public class ReadRepositoryQueryTests
{
    private sealed class Product : eQuantic.Core.Data.Repository.IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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

    private static QueryableReadRepository<DefaultUnitOfWork, Product, int> NewRepository(out TestDbContext context)
        => new(NewUnitOfWork(out context));

    private static AsyncQueryableReadRepository<DefaultUnitOfWork, Product, int> NewAsyncRepository(out TestDbContext context)
        => new(NewUnitOfWork(out context));

    [Test]
    public void All_WithSpecification_HonoursConfiguration()
    {
        var repository = NewRepository(out var context);
        context.Products.AddRange(
            new Product { Id = 1, Name = "active" },
            new Product { Id = 2, Name = "inactive" });
        context.SaveChanges();

        // The "inactive" row does not satisfy the specification, so without the configuration being
        // applied All() would evaluate over both rows and return false. The configuration narrows the
        // query to the "active" row, so the fixed code returns true.
        var result = repository.All(
            new NameSpecification("active"),
            cfg => cfg.WithAfterCustomization(q => q.Where(p => p.Name == "active")));

        Assert.That(result, Is.True);
    }

    [Test]
    public void Any_WithSpecification_HonoursConfiguration()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 1, Name = "active" });
        context.SaveChanges();

        // The configuration filters out every row, so Any() must return false once it is applied.
        var result = repository.Any(
            new NameSpecification("active"),
            cfg => cfg.WithAfterCustomization(q => q.Where(_ => false)));

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
    public void Get_WithConfiguration_UsesKeyExpression_ReturnsEntity()
    {
        var repository = NewRepository(out var context);
        context.Products.Add(new Product { Id = 7, Name = "seven" });
        context.SaveChanges();

        // A non-null configuration routes Get through GetFindByKeyExpression instead of DbSet.Find.
        var found = repository.Get(7, _ => { });

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

        var all = await repository.GetAllAsync(System.Threading.CancellationToken.None);

        Assert.That(all.Count(), Is.EqualTo(3));
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
