using CommunityToolkit.Mvvm.ComponentModel;

namespace Wolvenkit.Installer.Services;

public partial class Progress : ObservableObject
{
    [ObservableProperty]
#pragma warning disable IDE0044 // Add readonly modifier
    private int value;

    [ObservableProperty]
    private bool isIndeterminate;
#pragma warning restore IDE0044 // Add readonly modifier
}