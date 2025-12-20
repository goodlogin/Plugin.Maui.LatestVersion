using System.Text.Json;
using System.Text.RegularExpressions;

namespace Plugin.Maui.LatestVersion;

public static class GooglePlayStore
{
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
            var match = Regex.Match(
                html,
                @"AF_initDataCallback\({key:\s*'ds:5'.*?data:(\[[\s\S]*?\]),\s*?sideChannel:",
                RegexOptions.Singleline);
            if (!match.Success)
            {
                throw new InvalidOperationException("Failed to locate AF_initDataCallback data.");
            }

            string rawData = match.Groups[1].Value;

            // Convert the unquoted JavaScript object/array into valid JSON.
            // Replace unquoted keys with quoted keys and single quotes with double quotes.
            rawData = Regex.Replace(rawData, @"([{,])\s*([A-Za-z_]\w*):", "$1\"$2\":");
            rawData = rawData.Replace("'", "\"");

            // Parse the JSON.
            using JsonDocument doc = JsonDocument.Parse(rawData);
            JsonElement data = doc.RootElement;

            Console.WriteLine(data.ToString());

            // // The version string resides at [1][2][140][0][0][0] in the ds:5 data array.
            // // This path is based on reverse‑engineering of Google’s structure:contentReference[oaicite:2]{index=2}.
            // version = data[1][2][140][0][0][0].GetString();

            // Recursively search for a version-like string
            string version = FindVersion(data);
            if (version == null)
            {
                throw new InvalidOperationException("Failed to locate version string in the parsed data.");
            }

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
        if (element.ValueKind == JsonValueKind.String)
        {
            string text = element.GetString() ?? string.Empty;
            // Simple regex: one or more digits followed by at least one dot and more digits
            if (Regex.IsMatch(text, @"^\d+\.\d+(\.\d+)?$"))
            {
                return text;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = FindVersion(item);
                if (result != null) return result;
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var result = FindVersion(prop.Value);
                if (result != null) return result;
            }
        }

        return null;
    }
}
