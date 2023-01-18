using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Wolvenkit.Installer.Services;

[ObservableObject]
public partial class BannerNotification
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool isOpen;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private InfoBarSeverity severity;
}
