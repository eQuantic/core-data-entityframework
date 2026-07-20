using System;
using eQuantic.Core.Data.EntityFramework.Relational.Repository;
using eQuantic.Core.Data.EntityFramework.Relational.Sql;
using eQuantic.Core.Data.EntityFramework.SqlServer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

/// <summary>
///     Guards the fix for the SQL-injection defect: parameter values must be emitted as placeholders,
///     never interpolated into the SQL text. The implementation now lives in the shared
///     <see cref="RelationalSqlExecutor" />; SQL Server supplies the <c>EXEC</c> dialect.
/// </summary>
public class SqlExecutorParameterizationTests
{
    private const string Malicious = "'; DROP TABLE Users; --";

    private sealed class TestDbContext(DbContextOptions options) : DbContext(options);

    private static DefaultUnitOfWork NewSqlServerUnitOfWork()
    {
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase($"sqlexec-{Guid.NewGuid():N}")
            .Options;
        return new DefaultUnitOfWork(new ServiceCollection().BuildServiceProvider(), new TestDbContext(options));
    }

    [Test]
    public void GetQueryFunction_EmitsPositionalPlaceholders_NotValues()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var sql = RelationalSqlExecutor.GetQueryFunction("dbo.GetUser", config);

        Assert.That(sql, Does.Contain("{0}"));
        Assert.That(sql, Does.Contain("{1}"));
        Assert.That(sql, Does.Not.Contain("DROP TABLE"));
        Assert.That(sql, Does.Not.Contain(Malicious));
    }

    [Test]
    public void BuildProcedureSql_SqlServer_UsesExecWithNamedPlaceholders_NotValues()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var sql = NewSqlServerUnitOfWork().BuildProcedureSql("dbo.DoWork", config);

        Assert.That(sql, Does.StartWith("EXEC dbo.DoWork"));
        Assert.That(sql, Does.Contain("@Param0"));
        Assert.That(sql, Does.Contain("@Param1"));
        Assert.That(sql, Does.Not.Contain("DROP TABLE"));
        Assert.That(sql, Does.Not.Contain(Malicious));
    }

    [Test]
    public void BuildProcedureSql_UsesProvidedParameterNames()
    {
        var config = new DefaultSqlConfiguration()
            .WithParameters(ParamValue.Create("userId", 7), ParamValue.Create("state", "active"));

        var sql = NewSqlServerUnitOfWork().BuildProcedureSql("dbo.DoWork", config);

        Assert.That(sql, Does.Contain("@userId"));
        Assert.That(sql, Does.Contain("@state"));
    }

    [Test]
    public void GetParameterValues_PreservesValues_ForPositionalBinding()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var values = RelationalSqlExecutor.GetParameterValues(config);

        Assert.That(values, Has.Length.EqualTo(2));
        Assert.That(values, Does.Contain(Malicious));
        Assert.That(values, Does.Contain(42));
    }

    [Test]
    public void GetQueryFunction_EmptyForNoParameters()
    {
        var sql = RelationalSqlExecutor.GetQueryFunction("dbo.NoArgs", new DefaultSqlConfiguration());

        Assert.That(sql, Does.Not.Contain("{0}"));
    }
}
