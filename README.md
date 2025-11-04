<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Maui.LatestVersion

`Plugin.Maui.LatestVersion` provides the ability to detect if you are running the latest version of your iOS, Android, or Windows app and open it in the App Store, Play Store, or Microsoft Store to update it.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.LatestVersion.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.LatestVersion/)

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.LatestVersion).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.LatestVersion`, or through the NuGet Package Manager in Visual Studio.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 15.0+                       |
| Android  | 24.0 (API 24)              |
| Windows  | 10.0.17763.0+   |

## API Usage

Check if the current running app is the latest version available in the public store:

`bool isLatest = await CrossLatestVersion.Current.IsUsingLatestVersion();`

Get latest version number

`string latestVersionNumber = await CrossLatestVersion.Current.GetLatestVersionNumber();`
Warning: On Windows, this returns null due to platform limitations.