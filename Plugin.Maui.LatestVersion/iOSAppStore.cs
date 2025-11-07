using System.Text.Json;

namespace Plugin.Maui.LatestVersion;

public static class iOSAppStore
{
    public static async Task<StoreApp> GetiOSAppStoreAppAsync(string bundleIdentifier, string countryCode = "us")
    {
        try
        {
            using var http = new HttpClient();
            var response = await http.GetAsync($"https://itunes.apple.com/{countryCode}/lookup?bundleId={bundleIdentifier}");
            var content = response.Content == null ? null : await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var results = root.GetProperty("results");
            if (results.GetArrayLength() == 0)
                throw new LatestVersionException("No results found in App Store lookup.");

            var appInfo = results[0];

            return new StoreApp
            {
                Version = appInfo.GetProperty("version").GetString(),
                Url = appInfo.GetProperty("trackViewUrl").GetString()
            };
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error looking up app details in App Store. BundleIdentifier={bundleIdentifier}.", e);
        }
    }
}