using eQuantic.Core.Data.EntityFramework.SqlServer.Repository;
using eQuantic.Core.Data.Repository.Config;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

/// <summary>
///     Guards the fix for the SQL-injection defect: parameter values must be emitted as placeholders,
///     never interpolated into the SQL text.
/// </summary>
public class SqlExecutorParameterizationTests
{
    private const string Malicious = "'; DROP TABLE Users; --";

    [Test]
    public void GetQueryFunction_EmitsPositionalPlaceholders_NotValues()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var sql = SqlExecutor.GetQueryFunction("dbo.GetUser", config);

        Assert.That(sql, Does.Contain("{0}"));
        Assert.That(sql, Does.Contain("{1}"));
        Assert.That(sql, Does.Not.Contain("DROP TABLE"));
        Assert.That(sql, Does.Not.Contain(Malicious));
    }

    [Test]
    public void GetQueryProcedure_EmitsNamedPlaceholders_NotValues()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var sql = SqlExecutor.GetQueryProcedure("dbo.DoWork", config);

        Assert.That(sql, Does.StartWith("EXEC dbo.DoWork"));
        Assert.That(sql, Does.Contain("@Param0"));
        Assert.That(sql, Does.Contain("@Param1"));
        Assert.That(sql, Does.Not.Contain("DROP TABLE"));
        Assert.That(sql, Does.Not.Contain(Malicious));
    }

    [Test]
    public void GetQueryProcedure_UsesProvidedParameterNames()
    {
        var config = new DefaultSqlConfiguration()
            .WithParameters(ParamValueFor("userId", 7), ParamValueFor("state", "active"));

        var sql = SqlExecutor.GetQueryProcedure("dbo.DoWork", config);

        Assert.That(sql, Does.Contain("@userId"));
        Assert.That(sql, Does.Contain("@state"));
    }

    [Test]
    public void GetParameterValues_PreservesValues_ForPositionalBinding()
    {
        var config = new DefaultSqlConfiguration().WithParameters(Malicious, 42);

        var values = SqlExecutor.GetParameterValues(config);

        Assert.That(values, Has.Length.EqualTo(2));
        Assert.That(values, Does.Contain(Malicious));
        Assert.That(values, Does.Contain(42));
    }

    [Test]
    public void GetPositionalPlaceholders_EmptyForNoParameters()
    {
        var sql = SqlExecutor.GetQueryFunction("dbo.NoArgs", new DefaultSqlConfiguration());

        Assert.That(sql, Does.Not.Contain("{0}"));
    }

    private static eQuantic.Core.Data.Repository.Sql.ParamValue ParamValueFor(string name, object value)
        => eQuantic.Core.Data.Repository.Sql.ParamValue.Create(name, value);
}
