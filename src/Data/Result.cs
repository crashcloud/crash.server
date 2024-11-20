using System.Diagnostics.CodeAnalysis;

namespace Crash.Server.Data;

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable CA1000 // Do not declare static members on generic types
#pragma warning disable CA2201 // Do not raise reserved exception types

public struct Result
{
	public static Result<T> Ok<T>(T value) => new(value);
	public static Result<T> Err<T>(Exception error) => new(error);
	public static Result<T> Err<T>(string error) => new(new Exception(error));

}


public readonly struct Result<T>
{
	public T ResultValue { get; }
	public Exception ResultError { get; }
	public bool IsSuccess { get; }

	internal Result(T value)
	{
		ResultValue = value;
		IsSuccess = true;
	}

	internal Result(Exception error)
	{
		ResultError = error;
		IsSuccess = false;
	}

	public bool IsOk([MaybeNullWhen(false)] out T value)
	{
		value = ResultValue;
		return IsSuccess;
	}

	public bool IsErr([MaybeNullWhen(false)] out Exception error)
	{
		error = ResultError;
		return !IsSuccess;
	}
}
