using NLog.Layouts;

namespace StudyItBot.Utilities;

public static class StringExtension
{
    public static string SubstringAfter(this string value, char delimiter, string? missingDelimiterValue = null)
    {
        var index = value.IndexOf(delimiter);
        if (index == -1)
        {
            return missingDelimiterValue ?? value;
        }

        return value.Substring(index + 1, value.Length - (index + 1));
    }

    public static string SubstringAfter(this string value, string delimiter, string? missingDelimiterValue = null)
    {
        var index = value.IndexOf(delimiter, StringComparison.Ordinal);
        if (index == -1)
        {
            return missingDelimiterValue ?? value;
        }

        return value.Substring(index + delimiter.Length, value.Length - (index + delimiter.Length));
    }

    public static string SubstringBefore(this string value, char delimiter, string? missingDelimiterValue = null)
    {
        var index = value.IndexOf(delimiter);
        if (index == -1)
        {
            return missingDelimiterValue ?? value;
        }

        return value[..index];
    }
}