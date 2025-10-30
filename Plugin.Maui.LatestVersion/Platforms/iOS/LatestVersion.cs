using Foundation;
using System.Text.Json;

namespace Plugin.Maui.LatestVersion;

internal class App
{
    public string Version { get; set; }
    public string Url { get; set; }
}

/// <summary>
/// <see cref="ILatestVersion"/> implementation for iOS.
/// </summary>
public class LatestVersionImplementation : ILatestVersion
{
    App _app;

    string _bundleIdentifier => NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleIdentifier").ToString();
    string _bundleVersion => NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();

    /// <inheritdoc />
    public string CountryCode { get; set; } = "us";

    /// <inheritdoc />
    public string InstalledVersionNumber
    {
        get => _bundleVersion;
    }

    /// <inheritdoc />
    public async Task<bool> IsUsingLatestVersion()
    {
        var latestVersion = string.Empty;

        try
        {
            latestVersion = await GetLatestVersionNumber();

            return Version.Parse(latestVersion).CompareTo(Version.Parse(_bundleVersion)) <= 0;
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error comparing current app version number with latest. Bundle version={_bundleVersion} and lastest version={latestVersion} .", e);
        }
    }

    /// <inheritdoc />
    public async Task<string> GetLatestVersionNumber()
    {
        _app = await LookupApp();

        return _app?.Version;
    }

    /// <inheritdoc />
    public async Task OpenAppInStore()
    {
        try
        {
            if (_app == null)
            {
                _app = await LookupApp();
            }

#if __IOS__
            await Launcher.OpenAsync(new Uri(_app.Url));
            //UIKit.UIApplication.SharedApplication.OpenUrl(new NSUrl($"{_app.Url}"));
            //UIKit.UIApplication.SharedApplication.OpenUrl(new NSUrl($"{_app.Url}"), new UIKit.UIApplicationOpenUrlOptions(), null);

#elif __MACOS__
            await Launcher.OpenAsync(new Uri(_app.Url));
            //AppKit.NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl($"{_app.Url}"));
#endif
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Unable to open app in App Store.", e);
        }
    }

    async Task<App> LookupApp()
    {
        var countryCode = string.IsNullOrWhiteSpace(CountryCode) ? "us" : CountryCode;

        try
        {
            using var http = new HttpClient();
            var response = await http.GetAsync($"http://itunes.apple.com/{countryCode}/lookup?bundleId={_bundleIdentifier}");
            var content = response.Content == null ? null : await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var results = root.GetProperty("results");
            if (results.GetArrayLength() == 0)
                throw new LatestVersionException("No results found in App Store lookup.");

            var appInfo = results[0];

            return new App
            {
                Version = appInfo.GetProperty("version").GetString(),
                Url = appInfo.GetProperty("trackViewUrl").GetString()
            };
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error looking up app details in App Store. BundleIdentifier={_bundleIdentifier}.", e);
        }
    }
}
