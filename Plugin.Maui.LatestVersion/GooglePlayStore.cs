using System.Text.Json;
using System.Text.RegularExpressions;

namespace Plugin.Maui.LatestVersion;

public static class GooglePlayStore
{
    private static readonly Regex VersionPattern = new("^\\d+(?:\\.\\d+){1,3}$", RegexOptions.Compiled);

    public static async Task<StoreApp> GetGooglePlayStoreAppAsync(string packageName)
    {
        try
        {
            // Build the URL.  hl=en and gl=US help ensure English output.
            string url = $"https://play.google.com/store/apps/details?id={packageName}&hl=en&gl=US";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/118.0 Safari/537.36");

            // Download the HTML.
            string html = await client.GetStringAsync(url);

            // Regex to capture the AF_initDataCallback for ds:5 and extract the data array.
            var regex = new Regex(@"key:\s*'ds:5'.*?data:([\s\S]*?)\}\);", RegexOptions.Compiled | RegexOptions.Singleline);
            var match = regex.Match(html);
            if (!match.Success)
            {
                throw new InvalidOperationException("Failed to locate AF_initDataCallback data.");
            }

            string rawData = match.Groups[1].Value;

            // The payload sometimes appends a sideChannel section—trim it off before parsing.
            int sideChannelIndex = rawData.IndexOf(", sideChannel", StringComparison.Ordinal);
            if (sideChannelIndex > 0)
            {
                rawData = rawData[..sideChannelIndex];
            }

            using JsonDocument doc = JsonDocument.Parse(rawData);
            JsonElement data = doc.RootElement;

            string version = FindVersion(data);
            
            if (version == null)
                throw new InvalidOperationException("Failed to locate version string in the parsed data.");

            return new StoreApp
            {
                Version = version,
                Url = url
            };
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error fetching latest version number for package {packageName}", e);
        }
    }

    private static string FindVersion(JsonElement element)
    {
        string best = null;
        Traverse(element, ref best);
        return best;
    }

    private static void Traverse(JsonElement element, ref string best)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                string text = element.GetString() ?? string.Empty;
                if (IsVersionCandidate(text) && IsBetterCandidate(text, best))
                {
                    best = text;
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    Traverse(item, ref best);
                }
                break;

            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    Traverse(prop.Value, ref best);
                }
                break;
        }
    }

    private static bool IsVersionCandidate(string text)
    {
        return VersionPattern.IsMatch(text);
    }

    private static bool IsBetterCandidate(string candidate, string best)
    {
        if (best == null) return true;

        var candidateSegments = ParseSegments(candidate);
        var bestSegments = ParseSegments(best);

        if (candidateSegments.Length != bestSegments.Length)
        {
            return candidateSegments.Length > bestSegments.Length;
        }

        for (int i = 0; i < candidateSegments.Length; i++)
        {
            if (candidateSegments[i] == bestSegments[i]) continue;
            return candidateSegments[i] > bestSegments[i];
        }

        return candidate.Length > best.Length;
    }

    private static int[] ParseSegments(string version)
    {
        var parts = version.Split('.');
        var segments = new int[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            segments[i] = int.TryParse(parts[i], out int value) ? value : 0;
        }

        return segments;
    }
}

