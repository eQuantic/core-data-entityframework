using System;
using System.Collections.Generic;
using System.Linq;

namespace eQuantic.Core.Data.EntityFramework.Relational.Sql;

/// <summary>
///     Configuration for a raw SQL / stored-procedure / function invocation performed through the
///     relational SQL executor.
/// </summary>
/// <remarks>
///     Rehomed into the Relational provider from the removed <c>eQuantic.Core.Data.Repository.Config</c>
///     namespace (dropped in eQuantic.Core.Data v5).
/// </remarks>
public class SqlConfiguration
{
    public HashSet<ParamValue> Parameters { get; protected set; } = new();
    public string Tag { get; protected set; }
    public int? CommandTimeout { get; protected set; }
}

public class SqlConfiguration<TConfig> : SqlConfiguration where TConfig : SqlConfiguration<TConfig>
{
    public TConfig WithParameters(params object[] parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        Parameters = new HashSet<ParamValue>(parameters.Select(ParamValue.Create));
        return (TConfig)this;
    }

    public TConfig WithParameters(params ParamValue[] parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        Parameters.UnionWith(parameters);
        return (TConfig)this;
    }

    public TConfig WithTag(string tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        return (TConfig)this;
    }

    public TConfig WithCommandTimeout(int commandTimeout)
    {
        CommandTimeout = commandTimeout;
        return (TConfig)this;
    }
}

public class DefaultSqlConfiguration : SqlConfiguration<DefaultSqlConfiguration>
{
}
