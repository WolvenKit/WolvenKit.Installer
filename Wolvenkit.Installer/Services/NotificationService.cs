using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Wolvenkit.Installer.Services;

[ObservableObject]
public partial class NotificationService : INotificationService
{
    public NotificationService()
    {
        BannerNotification = new();
        Progress = new();
    }

    [ObservableProperty]
    private BannerNotification bannerNotification;

    [ObservableProperty]
    private Progress progress;


    // Banners
    public void DisplayBanner(string title, string message, InfoBarSeverity severity)
    {
        BannerNotification.IsOpen = true;

        BannerNotification.Message = message;
        BannerNotification.Severity = severity;
        BannerNotification.Title = title;
    }

    public void DisplayError(string message)
    {
        BannerNotification.IsOpen = true;

        BannerNotification.Message = message;
        BannerNotification.Severity = InfoBarSeverity.Error;
        BannerNotification.Title = "Error";
    }

    public void CloseBanner() => BannerNotification.IsOpen = false;

    public void StartIndeterminate() => Progress.IsIndeterminate = true;
    public void StopIndeterminate() => Progress.IsIndeterminate = false;
    public void Report(double v) => Progress.Value = (int)(v * 100);
    public void Completed()
    {
        Progress.Value = 100;
        Progress.IsIndeterminate = false;
    }
}
