﻿using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;
using Wolvenkit.Installer.Services;

namespace Wolvenkit.Installer.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly ILibraryService _libraryService;

    public MainViewModel(
        IDialogService dialogService,
        ILibraryService libraryService,
        INotificationService notificationService)
    {
        _dialogService = dialogService;
        _libraryService = libraryService;
        Notifications = notificationService;

        Init();
        Version = GetAppVersion();
    }

    [ObservableProperty]
    private INotificationService notifications;

    [ObservableProperty]
    private bool loaded;

    [ObservableProperty]
    private string version;

    //https://stackoverflow.com/a/28635481
    public static string GetAppVersion()
    {
        var package = Package.Current;
        var packageId = package.Id;
        var version = packageId.Version;

        return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
    }

    /// <summary>
    /// Load everything expensive and unlock the app afterwards
    /// </summary>
    private async void Init()
    {
        Notifications.StartIndeterminate();
        Notifications.DisplayBanner("Initializing", "Checking for updates. Please wait.", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);

        await InitAsync();

        Notifications.CloseBanner();
        Notifications.StopIndeterminate();

        Loaded = true;
    }

    /// <summary>
    /// Get remote versions
    /// </summary>
    /// <returns></returns>
    private async Task InitAsync() => await _libraryService.InitAsync();

    [RelayCommand]
    private async Task Refresh()
    {
        Notifications.StartIndeterminate();
        Notifications.DisplayBanner("Test", "test msg", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);

        await InitAsync();

        Notifications.CloseBanner();
        Notifications.Completed();
    }

    [RelayCommand]
    private void About()
    {
        Process.Start("explorer.exe", @"https://github.com/WolvenKit/WolvenKit");
    }

}
