using Android.Runtime;
using Application = Android.App.Application;

namespace Plugin.Maui.LatestVersion;

/// <summary>
/// <see cref="ILatestVersion"/> implementation for Android.
/// </summary>
[Preserve(AllMembers = true)]
public class LatestVersionImplementation : ILatestVersion
{
    string _packageName => Application.Context.PackageName;
    string _versionName => Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;

    /// <inheritdoc />
    public string CountryCode { get; set; }

    /// <inheritdoc />
    public string InstalledVersionNumber
    {
        get => _versionName;
    }

    /// <inheritdoc />
    public async Task<bool> IsUsingLatestVersion()
    {
        var latestVersion = string.Empty;

        try
        {
            latestVersion = await GetLatestVersionNumber();

            return Version.Parse(latestVersion).CompareTo(Version.Parse(_versionName)) <= 0;
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error comparing current app version number with latest. Version name={_versionName} and lastest version={latestVersion}", e);
        }
    }

    /// <inheritdoc />
    public async Task<string> GetLatestVersionNumber()
    {
        var storeApp = await GooglePlayStore.GetGooglePlayStoreAppAsync(_packageName);
        return storeApp.Version;
    }

    /// <inheritdoc />
    public async Task OpenAppInStore()
    {
        // try
        // {
        //     var intent = new Intent(Intent.ActionView, Net.Uri.Parse($"market://details?id={_packageName}"));
        //     intent.SetPackage("com.android.vending");
        //     intent.SetFlags(ActivityFlags.NewTask);
        //     Application.Context.StartActivity(intent);
        // }
        // catch (ActivityNotFoundException)
        // {
        //     var intent = new Intent(Intent.ActionView, Net.Uri.Parse($"https://play.google.com/store/apps/details?id={_packageName}"));
        //     Application.Context.StartActivity(intent);
        // }

        await Launcher.OpenAsync(new Uri($"https://play.google.com/store/apps/details?id={_packageName}"));
    }
}
