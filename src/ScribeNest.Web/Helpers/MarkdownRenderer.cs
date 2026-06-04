using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ScribeNest.Web.Helpers;

public static class MarkdownRenderer
{
    public static string ToHtml(string? markdown)
    {
        var normalized = (markdown ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        var lines = normalized.Split('\n');
        var html = new StringBuilder();
        var inList = false;

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (inList)
                {
                    html.AppendLine("</ul>");
                    inList = false;
                }

                continue;
            }

            if (trimmed.StartsWith("- "))
            {
                if (!inList)
                {
                    html.AppendLine("<ul>");
                    inList = true;
                }

                html.Append("<li>")
                    .Append(Inline(trimmed[2..]))
                    .AppendLine("</li>");

                continue;
            }

            if (inList)
            {
                html.AppendLine("</ul>");
                inList = false;
            }

            if (trimmed.StartsWith("### "))
            {
                html.Append("<h3>").Append(Inline(trimmed[4..])).AppendLine("</h3>");
            }
            else if (trimmed.StartsWith("## "))
            {
                html.Append("<h2>").Append(Inline(trimmed[3..])).AppendLine("</h2>");
            }
            else if (trimmed.StartsWith("# "))
            {
                html.Append("<h1>").Append(Inline(trimmed[2..])).AppendLine("</h1>");
            }
            else
            {
                html.Append("<p>").Append(Inline(trimmed)).AppendLine("</p>");
            }
        }

        if (inList)
        {
            html.AppendLine("</ul>");
        }

        return html.ToString();
    }

    private static string Inline(string value)
    {
        var encoded = WebUtility.HtmlEncode(value);

        encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        encoded = Regex.Replace(encoded, @"`(.+?)`", "<code>$1</code>");

        return encoded;
    }
}
