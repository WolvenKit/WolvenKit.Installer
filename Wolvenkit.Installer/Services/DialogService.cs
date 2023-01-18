using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Wolvenkit.Installer.Services;

public class DialogService : IDialogService
{
    public async Task DisplayAlert(string title, string message, string cancel)
    {
        ContentDialog dlg = new()
        {
            Title = title,
            Content = message,
            CloseButtonText = cancel,
            XamlRoot = App.MainRoot.XamlRoot,
        };

        await dlg.ShowAsync();
    }
}