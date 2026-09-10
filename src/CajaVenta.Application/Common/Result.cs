namespace CajaVenta.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = new();
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
        Errors = new() { error };
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Error = string.Join("; ", errors);
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(List<string> errors) => new(errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    private Result(bool success, string? error = null)
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}