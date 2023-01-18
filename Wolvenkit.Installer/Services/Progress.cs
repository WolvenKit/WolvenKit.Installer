using CommunityToolkit.Mvvm.ComponentModel;

namespace Wolvenkit.Installer.Services;

[ObservableObject]
public partial class Progress
{
    [ObservableProperty]
    private int value;

    [ObservableProperty]
    private bool isIndeterminate;

}