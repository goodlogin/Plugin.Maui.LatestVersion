using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Plugin.Maui.LatestVersion.Tests;

[TestClass]
public class GooglePlayStoreTests
{
    public TestContext TestContext { get; set; } = null!;

    // Popular Android app package names (US store)
    // These are well-known apps that should consistently resolve via the Play Store.

    [TestMethod]
    [DataRow("com.facebook.katana")] // Facebook
    [DataRow("com.instagram.android")] // Instagram
    [DataRow("com.snapchat.android")] // Snapchat
    [DataRow("com.whatsapp")] // WhatsApp
    [DataRow("com.google.android.youtube")] // YouTube
    [DataRow("com.google.android.apps.maps")] // Google Maps
    [DataRow("com.spotify.music")] // Spotify
    [DataRow("com.netflix.mediaclient")] // Netflix
    [DataRow("com.bentas.puma")] // Bentaş Puma
    [DataRow("gr.delhaize")] // Delhaize
    [DataRow("com.dev_metro.odri")] // Odri
    public async Task GetGooglePlayStoreAppAsync_ReturnsVersionAndUrl_ForPopularApps(string packageName)
    {
        var app = await GooglePlayStore.GetGooglePlayStoreAppAsync(packageName);

        Assert.IsNotNull(app, "Lookup returned null app");
        TestContext.WriteLine($"{packageName}: version {app.Version}, url {app.Url}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(app.Version), "Version should not be empty");
        Assert.IsFalse(string.IsNullOrWhiteSpace(app.Url), "Url should not be empty");
        StringAssert.StartsWith(app.Url, "http", "Url should be an http/https link");
        StringAssert.Contains(app.Url.ToLowerInvariant(), "play.google.com", "Url should point to the Play Store");
    }

    [TestMethod]
    public async Task GetGooglePlayStoreAppAsync_InvalidPackage_Throws()
    {
        try
        {
            await GooglePlayStore.GetGooglePlayStoreAppAsync("com.thisdoesnotexist._____");
            Assert.Fail("Expected LatestVersionException was not thrown.");
        }
        catch (LatestVersionException)
        {
            // Expected
        }
    }
}
