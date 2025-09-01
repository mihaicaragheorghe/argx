namespace Argx.Binding;

internal sealed class ArgumentConversionResult
{
    internal bool IsSuccess { get; }

    internal bool IsError { get; }

    internal object? Value { get; }

    internal string? Error { get; }

    private ArgumentConversionResult(bool success, object? value, string? error)
    {
        IsSuccess = success;
        IsError = !success;
        Value = value;
        Error = error;
    }

    internal static ArgumentConversionResult Success(object? value)
    {
        return new ArgumentConversionResult(success: true, value: value, error: null);
    }

    internal static ArgumentConversionResult Failure(string err)
    {
        return new ArgumentConversionResult(success: false, value: null, error: err);
    }
}