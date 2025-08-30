namespace Argx.Binding;

internal static partial class ArgumentConverter
{
    private delegate bool TryConvertString(string token, out object? value);

    private static Dictionary<Type, TryConvertString>? s_stringConverters;

    private static Dictionary<Type, TryConvertString> StringConverters
        => s_stringConverters ??= new()
        {
            [typeof(bool)] = (string token, out object? value) =>
            {
                if (bool.TryParse(token, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(string)] = (string input, out object? value) =>
            {
                value = input;
                return true;
            },

            [typeof(int)] = (string token, out object? value) =>
            {
                if (int.TryParse(token, out var intValue))
                {
                    value = intValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(long)] = (string token, out object? value) =>
            {
                if (long.TryParse(token, out var longValue))
                {
                    value = longValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(short)] = (string token, out object? value) =>
            {
                if (short.TryParse(token, out var shortValue))
                {
                    value = shortValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(uint)] = (string token, out object? value) =>
            {
                if (uint.TryParse(token, out var uintValue))
                {
                    value = uintValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(ulong)] = (string token, out object? value) =>
            {
                if (ulong.TryParse(token, out var ulongValue))
                {
                    value = ulongValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(ushort)] = (string token, out object? value) =>
            {
                if (ushort.TryParse(token, out var ushortValue))
                {
                    value = ushortValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(decimal)] = (string input, out object? value) =>
            {
                if (decimal.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(double)] = (string input, out object? value) =>
            {
                if (double.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(float)] = (string input, out object? value) =>
            {
                if (float.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(Guid)] = (string input, out object? value) =>
            {
                if (Guid.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DateTime)] = (string input, out object? value) =>
            {
                if (DateTime.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DateTimeOffset)] = (string input, out object? value) =>
            {
                if (DateTimeOffset.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(TimeSpan)] = (string input, out object? value) =>
            {
                if (TimeSpan.TryParse(input, out var timeSpan))
                {
                    value = timeSpan;
                    return true;
                }

                value = default;
                return false;
            },
        };
}