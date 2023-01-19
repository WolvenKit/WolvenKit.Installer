using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Wolvenkit.Installer.Services;

public partial class BannerNotification : ObservableObject
{
#pragma warning disable IDE0044 // Add readonly modifier
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool isOpen;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private InfoBarSeverity severity;
}
