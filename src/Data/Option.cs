using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable CA1000 // Do not declare static members on generic types
public readonly struct Option<T> where T : notnull
{
	public static Option<T> None => default;
	public static Option<T> Some(T value) => new(value);

	private bool IsSomeCached { get; }
	private T Value { get; }

	public Option(T value)
	{
		Value = value;
		IsSomeCached = Value is { };
	}

	public bool IsSome([MaybeNullWhen(false)] out T value)
	{
		value = Value;
		return IsSomeCached;
	}
}

