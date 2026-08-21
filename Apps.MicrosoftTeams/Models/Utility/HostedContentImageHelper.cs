using System.Net;
using System.Text.RegularExpressions;

namespace Apps.MicrosoftTeams.Models.Utility;

internal static partial class HostedContentImageHelper
{
    public static IReadOnlyList<string> GetIds(string? messageBody)
    {
        if (string.IsNullOrWhiteSpace(messageBody))
            return [];

        var result = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match imageMatch in ImageTagRegex().Matches(messageBody))
        {
            var sourceMatch = SourceAttributeRegex().Match(imageMatch.Value);
            if (!sourceMatch.Success)
                continue;

            var source = WebUtility.HtmlDecode(sourceMatch.Groups["source"].Value);
            var hostedContentMatch = HostedContentPathRegex().Match(source);
            if (!hostedContentMatch.Success)
                continue;

            var id = Uri.UnescapeDataString(hostedContentMatch.Groups["id"].Value);
            if (seenIds.Add(id))
                result.Add(id);
        }

        return result;
    }

    public static (string ContentType, string Extension) DetectImageType(Stream content)
    {
        var originalPosition = content.CanSeek ? content.Position : 0;
        Span<byte> header = stackalloc byte[16];
        var bytesRead = content.Read(header);

        if (content.CanSeek)
            content.Position = originalPosition;

        var bytes = header[..bytesRead];

        if (bytes.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ("image/png", ".png");
        if (bytes.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }))
            return ("image/jpeg", ".jpg");
        if (bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8))
            return ("image/gif", ".gif");
        if (bytes.StartsWith("BM"u8))
            return ("image/bmp", ".bmp");
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return ("image/webp", ".webp");
        if (bytes.StartsWith(new byte[] { 0x49, 0x49, 0x2A, 0x00 }) ||
            bytes.StartsWith(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }))
            return ("image/tiff", ".tiff");

        return ("application/octet-stream", ".bin");
    }

    [GeneratedRegex(@"<img\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ImageTagRegex();

    [GeneratedRegex("""\bsrc\s*=\s*["'](?<source>[^"']+)["']""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SourceAttributeRegex();

    [GeneratedRegex(@"/hostedContents/(?<id>[^/?#]+)(?:/\$value)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HostedContentPathRegex();
}
