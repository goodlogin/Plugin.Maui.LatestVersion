using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Services.Store;

namespace Plugin.Maui.LatestVersion;
public class LatestVersionImplementation : ILatestVersion
{
    string _packageVersion => PackageVersionToString(Package.Current.Id.Version);

    /// <inheritdoc />
    public string CountryCode { get; set; }

    /// <inheritdoc />
    public string InstalledVersionNumber
    {
        get => _packageVersion;
    }

    /// <inheritdoc />
    public async Task<bool> IsUsingLatestVersion()
    {
        try
        {
            var context = StoreContext.GetDefault();
            var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();

            return updates.Count <= 0;
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error checking for updates available in the Store.", e);
        }
    }

    // Helper method to convert PackageVersion to string
    private static string PackageVersionToString(PackageVersion version)
    {
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    /// <inheritdoc />
    public Task<string> GetLatestVersionNumber()
    {
        try
        {

            return Task.FromResult("");

            // TODO: Disabled due to Microsoft Store limitations/reliability issues.
            //var version = _packageVersion;
            //var context = StoreContext.GetDefault();
            //var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();

            //if (updates.Count > 0)
            //{
            //    var pkgVersion = updates[0]?.Package?.Id?.Version;
            //    if (pkgVersion != null)
            //    {
            //        version = PackageVersionToString(pkgVersion.Value);
            //    }
            //}

            //return version;

        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Error getting the latest version number available in the Store.", e);
        }
    }

    /// <inheritdoc />
    public async Task OpenAppInStore()
    {
        try
        {
            var context = StoreContext.GetDefault();
            var product = await context.GetStoreProductForCurrentAppAsync();
            var storeId = product.Product.StoreId;

            if (Guid.TryParse(storeId, out Guid appId))
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://pdp/?AppId={storeId}"));
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://pdp/?ProductId={storeId}"));
            }
        }
        catch (Exception e)
        {
            throw new LatestVersionException($"Unable to open the current app in the Store.", e);
        }
    }
}
