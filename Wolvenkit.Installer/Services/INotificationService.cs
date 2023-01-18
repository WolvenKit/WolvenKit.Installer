using Microsoft.UI.Xaml.Controls;

namespace Wolvenkit.Installer.Services;
public interface INotificationService
{
    BannerNotification BannerNotification { get; set; }
    Progress Progress { get; set; }

    void CloseBanner();
    void DisplayBanner(string title, string message, InfoBarSeverity severity);
    void DisplayError(string message);


    void StartIndeterminate();
    void StopIndeterminate();
    /// <summary>
    /// Report Progress
    /// </summary>
    /// <param name="v"></param>
    void Report(double v);
    /// <summary>
    /// Report full progress and halt indeterminate
    /// </summary>
    void Completed();
}
