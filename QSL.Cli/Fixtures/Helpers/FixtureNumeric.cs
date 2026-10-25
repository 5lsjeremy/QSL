using System;
using System.Globalization;

namespace QSL.Cli.Fixtures.Helpers
{
    internal static class FixtureNumeric
    {
        public static bool TryConvert(
            object? value,
            out double result)
        {
            switch (value)
            {
                case byte byteValue:
                    result = byteValue;
                    return true;

                case sbyte signedByteValue:
                    result = signedByteValue;
                    return true;

                case short shortValue:
                    result = shortValue;
                    return true;

                case ushort unsignedShortValue:
                    result = unsignedShortValue;
                    return true;

                case int intValue:
                    result = intValue;
                    return true;

                case uint unsignedIntValue:
                    result = unsignedIntValue;
                    return true;

                case long longValue:
                    result = longValue;
                    return true;

                case ulong unsignedLongValue:
                    result = unsignedLongValue;
                    return true;

                case float floatValue:
                    result = floatValue;
                    return true;

                case double doubleValue:
                    result = doubleValue;
                    return true;

                case decimal decimalValue:
                    result = (double)decimalValue;
                    return true;

                case string text:
                    return double.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out result
                    );

                default:
                    result = default;
                    return false;
            }
        }
    }
}