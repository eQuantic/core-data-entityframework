namespace eQuantic.Core.Data.EntityFramework.Relational.Sql;

/// <summary>
///     A named (or positional) parameter value carried through the relational SQL executor. Values
///     flow into commands as <see cref="System.Data.Common.DbParameter" />s and are never interpolated
///     into the SQL text.
/// </summary>
/// <remarks>
///     Rehomed into the Relational provider from the removed <c>eQuantic.Core.Data.Repository.Sql</c>
///     namespace (dropped in eQuantic.Core.Data v5).
/// </remarks>
public sealed class ParamValue
{
    public string Name { get; }
    public object Value { get; }

    private ParamValue(string name, object value) : this(value)
    {
        Name = name;
    }

    private ParamValue(object value)
    {
        Value = value;
    }

    public static ParamValue Create(string name, object value) => new ParamValue(name, value);
    public static ParamValue Create(object value) => new ParamValue(value);

    public override bool Equals(object obj)
    {
        return obj is ParamValue paramValue && (
            !string.IsNullOrEmpty(paramValue.Name)
                ? paramValue.Name == Name
                : paramValue.Name == Name && paramValue.Value.Equals(Value)
        );
    }

    public override int GetHashCode()
    {
        return (Name, Value).GetHashCode();
    }
}
