using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Plugin.Maui.LatestVersion.Tests;

[TestClass]
public class IOSAppStoreTests
{
    // Popular iOS app bundle identifiers (US store)
    // These are well-known apps that should consistently resolve via the iTunes Lookup API.
    // Source: public app bundle identifiers for major apps.

    [TestMethod]
    [DataRow("com.facebook.Facebook")] // Facebook
    [DataRow("com.burbn.instagram")] // Instagram
    [DataRow("com.toyopagroup.picaboo")] // Snapchat
    [DataRow("net.whatsapp.WhatsApp")] // WhatsApp
    [DataRow("com.google.ios.youtube")] // YouTube
    [DataRow("com.google.Maps")] // Google Maps
    [DataRow("com.spotify.client")] // Spotify
    [DataRow("com.netflix.Netflix")] // Netflix
    public async Task GetiOSAppStoreAppAsync_ReturnsVersionAndUrl_ForPopularApps(string bundleId)
    {
        var app = await iOSAppStore.GetiOSAppStoreAppAsync(bundleId, "us");

        Assert.IsNotNull(app, "Lookup returned null app");
        Assert.IsFalse(string.IsNullOrWhiteSpace(app.Version), "Version should not be empty");
        Assert.IsFalse(string.IsNullOrWhiteSpace(app.Url), "Url should not be empty");
        StringAssert.StartsWith(app.Url, "http", "Url should be an http/https link");
        StringAssert.Contains(app.Url.ToLowerInvariant(), "apple.com", "Url should point to the App Store");
    }

    [TestMethod]
    public async Task GetiOSAppStoreAppAsync_InvalidBundle_Throws()
    {
        try
        {
            await iOSAppStore.GetiOSAppStoreAppAsync("com.thisdoesnotexist._____");
            Assert.Fail("Expected LatestVersionException was not thrown.");
        }
        catch (LatestVersionException)
        {
            // Expected
        }
    }
}
