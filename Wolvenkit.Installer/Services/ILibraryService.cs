using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Wolvenkit.Installer.Models;
using Wolvenkit.Installer.ViewModel;

namespace Wolvenkit.Installer.Services;

public interface ILibraryService
{
    ObservableCollection<PackageViewModel> InstalledPackages { get; }

    ObservableCollection<RemotePackageViewModel> RemotePackages { get; set; }

    Task<string> GetLatestVersionAsync(RemotePackageModel model, bool prerelease);
    Task<bool> InstallAsync(RemotePackageModel id, string installPath);
    Task<bool> InstallAsync(PackageModel id, string installPath);
    Task InitAsync();
    Task SaveAsync();
    Task<bool> RemoveAsync(PackageModel model);
    bool TryGetRemote(PackageModel model, out RemotePackageViewModel remote);
}

