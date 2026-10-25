using System.Globalization;

internal static class QslValueFormatter
{
    public static string Format(
        object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is bool boolean)
        {
            return boolean
                ? "true"
                : "false";
        }

        if (value is double number)
        {
            return number.ToString(
                "0.################",
                CultureInfo.InvariantCulture
            );
        }

        if (value is float single)
        {
            return single.ToString(
                "0.################",
                CultureInfo.InvariantCulture
            );
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(
                       null,
                       CultureInfo.InvariantCulture
                   )
                   ?? string.Empty;
        }

        return value.ToString()
               ?? string.Empty;
    }
}