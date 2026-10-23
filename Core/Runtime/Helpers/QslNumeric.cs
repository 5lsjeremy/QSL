using System;

namespace QSL.Core.Runtime.Helpers
{
    internal static class QslNumeric
    {
        public static bool IsNumeric(
            object? value)
        {
            return value is sbyte ||
                   value is byte ||
                   value is short ||
                   value is ushort ||
                   value is int ||
                   value is uint ||
                   value is long ||
                   value is ulong ||
                   value is float ||
                   value is double ||
                   value is decimal;
        }

        public static bool TryConvert(
            object? value,
            out double number)
        {
            if (!IsNumeric(value))
            {
                number = 0.0;
                return false;
            }

            number = Convert.ToDouble(
                value
            );

            return true;
        }

        public static bool TryConvertPair(
            object? left,
            object? right,
            out double leftNumber,
            out double rightNumber)
        {
            if (!TryConvert(
                    left,
                    out leftNumber))
            {
                rightNumber = 0.0;
                return false;
            }

            if (!TryConvert(
                    right,
                    out rightNumber))
            {
                return false;
            }

            return true;
        }
    }
}