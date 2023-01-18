using Windows.Storage;

namespace Wolvenkit.Installer.Helper;
internal static class SettingsHelper
{
    private const string s_useZipInstallers = "useZipInstallers";

    // Booleans
    public static bool GetUseZipInstallers() => ApplicationData.Current.LocalSettings.Values.TryGetValue(s_useZipInstallers, out var value) && value is bool b && b;
    public static void SetUseZipInstallers(bool value) => ApplicationData.Current.LocalSettings.Values[s_useZipInstallers] = value;
}
