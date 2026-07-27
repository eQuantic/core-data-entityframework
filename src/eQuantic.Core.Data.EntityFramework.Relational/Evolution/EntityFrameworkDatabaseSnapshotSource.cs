using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Evolution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eQuantic.Core.Data.EntityFramework.Relational.Evolution;

/// <summary>
///     Describes both sides of a drift check for an Entity Framework model: the tables it maps, as it says they
///     should be, and those same tables as the database actually has them.
///     <para>
///         This is the one question Entity Framework does not answer for itself. Its history table records which
///         migrations <em>ran</em>; <c>has-pending-model-changes</c> compares the model to the migrations. Neither
///         looks at the schema, so neither can report a column altered by hand on staging, a migration that stopped
///         halfway, or an environment restored from a backup older than the last release. Only looking answers
///         those, and this looks.
///     </para>
///     <para>
///         Generating migrations is deliberately not here: Entity Framework already does that, from the same model,
///         and better than a second generator could. What is added is the part it leaves out.
///     </para>
/// </summary>
public sealed class EntityFrameworkDatabaseSnapshotSource(DbContext context) : IDatabaseSnapshotSource
{
    /// <summary>
    ///     How each provider is asked what it holds. Each query returns the same four values in order — table,
    ///     column, type, nullability — and spells the type the way its own provider spells it in DDL, which is
    ///     also how Entity Framework's <c>GetColumnType()</c> spells it. That alignment is the whole reason a
    ///     healthy schema compares silently.
    /// </summary>
    private static readonly Dictionary<string, string> Catalogues = new(StringComparer.Ordinal)
    {
        ["Npgsql.EntityFrameworkCore.PostgreSQL"] =
            """
            SELECT c.relname, a.attname, format_type(a.atttypid, a.atttypmod), NOT a.attnotnull
            FROM pg_attribute a
            JOIN pg_class c ON c.oid = a.attrelid
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = current_schema() AND c.relkind = 'r' AND a.attnum > 0 AND NOT a.attisdropped
            """,
        ["Pomelo.EntityFrameworkCore.MySql"] =
            """
            SELECT table_name, column_name, column_type, is_nullable = 'YES'
            FROM information_schema.columns
            WHERE table_schema = database()
            """,
        ["Microsoft.EntityFrameworkCore.SqlServer"] =
            """
            SELECT t.name, c.name,
                   ty.name + CASE
                       WHEN ty.name IN ('nvarchar', 'nchar')
                           THEN '(' + IIF(c.max_length = -1, 'max', CAST(c.max_length / 2 AS varchar(11))) + ')'
                       WHEN ty.name IN ('varchar', 'char', 'varbinary', 'binary')
                           THEN '(' + IIF(c.max_length = -1, 'max', CAST(c.max_length AS varchar(11))) + ')'
                       WHEN ty.name IN ('decimal', 'numeric')
                           THEN '(' + CAST(c.precision AS varchar(11)) + ',' + CAST(c.scale AS varchar(11)) + ')'
                       ELSE '' END,
                   c.is_nullable
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = SCHEMA_NAME()
            """,
    };

    /// <inheritdoc />
    public string Provider => context.Database.ProviderName ?? "unknown";

    /// <inheritdoc />
    public DatabaseSnapshot Expect() =>
        new(Provider, Mapped()
            .Select(entity => new DatabaseCollection(entity.Table,
                entity.EntityType.ClrType.FullName ?? entity.EntityType.ClrType.Name,
                entity.EntityType.GetProperties()
                    .Select(property => new DatabaseField(
                        property.GetColumnName(),
                        Normalize(property.GetColumnType()),
                        property.IsNullable))
                    .ToList()))
            .ToList());

    /// <inheritdoc />
    public async Task<DatabaseSnapshot> ObserveAsync(CancellationToken cancellationToken = default)
    {
        if (!Catalogues.TryGetValue(Provider, out var sql))
        {
            throw new NotSupportedException(
                $"'{Provider}' does not read its own catalogue here, so there is nothing to compare the model " +
                "against — which is not the same as there being no drift. PostgreSQL, MySQL and SQL Server do.");
        }

        var columns = await ReadAsync(sql, cancellationToken).ConfigureAwait(false);

        // Only the tables the model maps: a database is usually shared, and reporting every table an application
        // does not know about would bury the findings that matter.
        return new DatabaseSnapshot(Provider, Mapped()
            .Where(entity => columns.ContainsKey(entity.Table))
            .Select(entity => new DatabaseCollection(entity.Table,
                entity.EntityType.ClrType.FullName ?? entity.EntityType.ClrType.Name,
                columns[entity.Table]))
            .ToList());
    }

    /// <summary>The entity types that land in a table of their own — owned types share their owner's.</summary>
    private IEnumerable<(string Table, IEntityType EntityType)> Mapped() =>
        context.Model.GetEntityTypes()
            .Where(entity => !entity.IsOwned())
            .Select(entity => (Table: entity.GetTableName(), EntityType: entity))
            .Where(entity => !string.IsNullOrEmpty(entity.Table))
            .GroupBy(entity => entity.Table!, StringComparer.Ordinal)
            // Table-per-hierarchy puts several types in one table; it is still one table to check.
            .Select(group => (group.Key, group.First().EntityType));

    /// <summary>
    ///     One spelling for a type. Only case and the space after a facet's comma are flattened —
    ///     <c>numeric(18, 2)</c> and <c>numeric(18,2)</c> are the same type, while <c>timestamp with time zone</c>
    ///     needs its spaces and must survive.
    /// </summary>
    private static string Normalize(string? storedType) =>
        (storedType ?? string.Empty).Trim().ToLowerInvariant().Replace(", ", ",");

    private async Task<Dictionary<string, List<DatabaseField>>> ReadAsync(string sql,
        CancellationToken cancellationToken)
    {
        var byTable = new Dictionary<string, List<DatabaseField>>(StringComparer.Ordinal);

        var connection = context.Database.GetDbConnection();
        var opened = connection.State != System.Data.ConnectionState.Open;
        if (opened)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var table = reader.GetString(0);
                if (!byTable.TryGetValue(table, out var fields))
                {
                    byTable[table] = fields = [];
                }

                fields.Add(new DatabaseField(reader.GetString(1), Normalize(reader.GetString(2)),
                    reader.GetBoolean(3)));
            }
        }
        finally
        {
            // Left as it was found: the context may own this connection and still be using it.
            if (opened)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        return byTable;
    }
}
