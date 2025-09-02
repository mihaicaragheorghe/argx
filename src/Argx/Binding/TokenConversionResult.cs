namespace Argx.Binding;

internal sealed class TokenConversionResult
{
    internal bool IsSuccess { get; }

    internal bool IsError { get; }

    internal object? Value { get; }

    internal string? Error { get; }

    private TokenConversionResult(bool success, object? value, string? error)
    {
        IsSuccess = success;
        IsError = !success;
        Value = value;
        Error = error;
    }

    internal static TokenConversionResult Success(object? value)
    {
        return new TokenConversionResult(success: true, value: value, error: null);
    }

    internal static TokenConversionResult Failure(string err)
    {
        return new TokenConversionResult(success: false, value: null, error: err);
    }
}