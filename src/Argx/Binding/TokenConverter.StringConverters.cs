using System.Globalization;

namespace Argx.Binding;

internal static partial class TokenConverter
{
    private delegate bool TryConvertString(string token, out object? value);

    private static Dictionary<Type, TryConvertString> StringConverters
    {
        get => field ??= new Dictionary<Type, TryConvertString>
        {
            [typeof(string)] = (input, out value) =>
            {
                value = input;
                return true;
            },

            [typeof(bool)] = (token, out value) =>
            {
                if (bool.TryParse(token, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(int)] = (token, out value) =>
            {
                if (int.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Int,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var intValue))
                {
                    value = intValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(long)] = (token, out value) =>
            {
                if (long.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Long,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var longValue))
                {
                    value = longValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(short)] = (token, out value) =>
            {
                if (short.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Short,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var shortValue))
                {
                    value = shortValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(uint)] = (token, out value) =>
            {
                if (uint.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Int,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var uintValue))
                {
                    value = uintValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(ulong)] = (token, out value) =>
            {
                if (ulong.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Long,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var ulongValue))
                {
                    value = ulongValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(ushort)] = (token, out value) =>
            {
                if (ushort.TryParse(token,
                    style: ArgumentConversionDefaults.NumberStyles.Short,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var ushortValue))
                {
                    value = ushortValue;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(decimal)] = (input, out value) =>
            {
                if (decimal.TryParse(input,
                    style: ArgumentConversionDefaults.NumberStyles.Decimal,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(double)] = (input, out value) =>
            {
                if (double.TryParse(input,
                    style: ArgumentConversionDefaults.NumberStyles.Double,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(float)] = (input, out value) =>
            {
                if (float.TryParse(input,
                    style: ArgumentConversionDefaults.NumberStyles.Float,
                    provider: ArgumentConversionDefaults.FormatProvider,
                    result: out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(Guid)] = (input, out value) =>
            {
                if (Guid.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(DateTime)] = (input, out value) =>
            {
                if (ArgumentConversionDefaults.DateTimeFormat != null)
                {
                    if (DateTime.TryParseExact(input,
                        format: ArgumentConversionDefaults.DateTimeFormat,
                        provider: ArgumentConversionDefaults.FormatProvider,
                        style: DateTimeStyles.None,
                        result: out var parsedExact))
                    {
                        value = parsedExact;
                        return true;
                    }

                    value = null;
                    return false;
                }

                if (DateTime.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(DateTimeOffset)] = (input, out value) =>
            {
                if (ArgumentConversionDefaults.DateTimeFormat != null)
                {
                    if (DateTimeOffset.TryParseExact(input,
                        format: ArgumentConversionDefaults.DateTimeFormat,
                        formatProvider: ArgumentConversionDefaults.FormatProvider,
                        styles: DateTimeStyles.AllowWhiteSpaces,
                        result: out var parsedExact))
                    {
                        value = parsedExact;
                        return true;
                    }

                    value = null;
                    return false;
                }

                if (DateTimeOffset.TryParse(input, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = null;
                return false;
            },

            [typeof(TimeSpan)] = (input, out value) =>
            {
                if (ArgumentConversionDefaults.TimeSpanFormat != null)
                {
                    if (TimeSpan.TryParseExact(input,
                        format: ArgumentConversionDefaults.TimeSpanFormat,
                        formatProvider: ArgumentConversionDefaults.FormatProvider,
                        result: out var timeSpanExact))
                    {
                        value = timeSpanExact;
                        return true;
                    }

                    value = null;
                    return false;
                }

                if (TimeSpan.TryParse(input, out var timeSpan))
                {
                    value = timeSpan;
                    return true;
                }

                value = null;
                return false;
            },
        };
    }
}