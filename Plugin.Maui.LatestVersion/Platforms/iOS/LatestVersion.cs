using Foundation;

namespace Plugin.Maui.LatestVersion;

/// <summary>
/// <see cref="ILatestVersion"/> implementation for iOS.
/// </summary>
public class LatestVersionImplementation : ILatestVersion
{
    StoreApp _app;

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
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
                throw new LatestVersionException("Cannot open App Store on a simulator.");

            _app ??= await LookupApp();

            // itms://	Old iTunes Store links	Deprecated
            // itmss://	Secure iTunes Store links	Deprecated
            // itms-apps://	App Store links	✅ Current and recommended
            var appStoreUrl = _app.Url.Replace("https://", "itms-apps://");
            await Launcher.OpenAsync(new Uri(appStoreUrl));
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Unable to open app in App Store.", e);
        }
    }

    async Task<StoreApp> LookupApp()
    {
        var countryCode = string.IsNullOrWhiteSpace(CountryCode) ? "us" : CountryCode;

        return await iOSAppStore.GetiOSAppStoreAppAsync(_bundleIdentifier, countryCode);
    }
}
