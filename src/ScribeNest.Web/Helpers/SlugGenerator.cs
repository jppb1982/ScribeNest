using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ScribeNest.Web.Helpers;

public static class SlugGenerator
{
    private static readonly Regex InvalidCharsRegex = new("[^a-z0-9\\s-]", RegexOptions.Compiled);
    private static readonly Regex SeparatorRegex = new("[\\s-]+", RegexOptions.Compiled);

    public static string Generate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "post";

        var normalized = RemoveDiacritics(value.Trim().ToLowerInvariant());
        normalized = InvalidCharsRegex.Replace(normalized, " ");
        normalized = SeparatorRegex.Replace(normalized, "-").Trim('-');

        return string.IsNullOrWhiteSpace(normalized) ? "post" : normalized;
    }

    public static string MakeUnique(string baseSlug, IEnumerable<string> existingSlugs)
    {
        var existing = existingSlugs
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!existing.Contains(baseSlug))
            return baseSlug;

        var counter = 2;
        string candidate;
        do
        {
            candidate = $"{baseSlug}-{counter}";
            counter++;
        }
        while (existing.Contains(candidate));

        return candidate;
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
