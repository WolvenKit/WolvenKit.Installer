﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Wolvenkit.Installer.Models;
using Wolvenkit.Installer.Pages;
using Wolvenkit.Installer.Services;

namespace Wolvenkit.Installer.ViewModel;

[ObservableObject]
public partial class RemotePackageViewModel
{
    private readonly ILibraryService _libraryService;
    private readonly INotificationService _notificationService;
    private readonly RemotePackageModel _model;

    public RemotePackageViewModel(
        RemotePackageModel model,
        string version
        )
    {
        _libraryService = App.Current.Services.GetService<ILibraryService>();
        _notificationService = App.Current.Services.GetService<INotificationService>();
        _model = model;

        Version = version;

        // default
        var appName = Name.Split('/').Last();
        InstallPath = Path.Combine(@"c:\ModdingTools", appName);
    }

    public string Name => _model.Name;

    public string Description => _model.Description;

    public string ImagePath => _model.ImagePath;

    public string NavigateUri => $"{_model.Url}/releases/latest";

    // Local
    public string Version { get; }


    [ObservableProperty]
    private string installPath;

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task Install()
    {
        await _libraryService.InstallAsync(_model, InstallPath);

        _notificationService.CloseBanner();
        _notificationService.StopIndeterminate();

        //var rootPage = App.StartupWindow as MainWindow;
        //rootPage.Navigate(typeof(InstalledPage), "");
    }

    [RelayCommand()]
    private async Task PickFolder()
    {
        // Create a folder picker.
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();

        // Initialize the folder picker with the window handle (HWND).
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.StartupWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        // Use the folder picker as usual.
        folderPicker.FileTypeFilter.Add("*");
        var storageFolder = await folderPicker.PickSingleFolderAsync();

        if (storageFolder is not null)
        {
            var folder = storageFolder.Path;
            if (Directory.Exists(folder))
            {
                InstallPath = folder;
            }
        }
    }

    private bool CanInstall() => !Directory.Exists(InstallPath) || (Directory.Exists(InstallPath) && !Directory.GetFiles(InstallPath).Any());

    public RemotePackageModel GetModel() => _model;
}

