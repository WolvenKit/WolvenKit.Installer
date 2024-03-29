﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Semver;
using Wolvenkit.Installer.Helper;
using Wolvenkit.Installer.Models;
using Wolvenkit.Installer.Pages;
using Wolvenkit.Installer.ViewModel;

namespace Wolvenkit.Installer.Services;

public class LibraryService : ILibraryService
{
    private readonly HttpClient _httpClient = new();
    private readonly ILogger<LibraryService> _logger;
    private readonly INotificationService _notificationService;

    public const string FileName = "library.json";

    public LibraryService(ILogger<LibraryService> logger, INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService;
    }

    public ObservableCollection<PackageViewModel> InstalledPackages { get; set; } = new();

    public ObservableCollection<RemotePackageViewModel> RemotePackages { get; set; } = new();

    #region Lifetime

    private static string GetAppData()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "REDModding", "WolvenKit.Installer");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir;
    }

    /// <summary>
    /// Load and initialize
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    public async Task InitAsync()
    {
        // get remote info from static db
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Wolvenkit.Installer.Resources.AvailableApps.json");
        var available = await JsonSerializer.DeserializeAsync<List<RemotePackageModel>>(stream,
            new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

        // create available viewmodels
        foreach (var item in available)
        {
            var latest = await GetLatestVersionAsync(item);
            if (string.IsNullOrEmpty(latest))
            {
                // todo logging
                continue;
            }
            RemotePackages.Add(new(item, latest));
        }

        // load installed packages
        await LoadAsync();
    }

    /// <summary>
    /// Serialize
    /// </summary>
    /// <returns></returns>
    public async Task SaveAsync()
    {
        var models = InstalledPackages.Select(x => x.GetModel()).ToArray();
        var json = JsonSerializer.Serialize(models, new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var file = Path.Combine(GetAppData(), FileName);
        await File.WriteAllTextAsync(file, json);
    }

    /// <summary>
    /// Load installed packages
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    private async Task LoadAsync()
    {
        var file = Path.Combine(GetAppData(), FileName);

        if (File.Exists(file))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var installedApps = JsonSerializer.Deserialize<List<PackageModel>>(json, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                if (installedApps is not null)
                {
                    // check for updates
                    foreach (var item in installedApps)
                    {
                        // check if exists
                        if (!Directory.Exists(item.Path))
                        {
                            continue;
                        }

                        // check for updates
                        if (TryGetRemote(item.Name, out var remote))
                        {
                            var installedVersion = SemVersion.Parse(item.Version, SemVersionStyles.OptionalMinorPatch);
                            var remoteVersion = SemVersion.Parse(remote.Version, SemVersionStyles.OptionalMinorPatch);
                            var updateAvailable = remoteVersion.CompareSortOrderTo(installedVersion) > 0;

                            InstalledPackages.Add(new PackageViewModel(
                            item,
                            updateAvailable ? EPackageStatus.UpdateAvailable : EPackageStatus.Installed,
                            "ms-appx:///Assets/ControlImages/Acrylic.png"
                            ));
                        }
                    }
                }
                else
                {
                    InstalledPackages = new();
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not deserialize library");
                throw;
            }
        }
        else
        {
            InstalledPackages = new();
        }

        // check if new app needs registering
        if (App.TargetAppSettings != null)
        {
            var found = false;
            foreach (var item in InstalledPackages)
            {
                if (item.Name == App.TargetAppSettings.TargetId)
                {
                    var targetPath = App.TargetAppSettings.TargetLocation.FullName.ToUpper();
                    var installedPath = item.Location.ToUpper();
                    if (string.Equals(targetPath, installedPath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                // do nothing, already installed
            }
            else
            {

                // register
                var installedPackage = new PackageModel(
                    App.TargetAppSettings.TargetId,
                    App.TargetAppSettings.TargetVersion,
                    App.TargetAppSettings.TargetLocation.FullName);

                // check for updates
                if (TryGetRemote(installedPackage.Name, out var remote))
                {
                    var installedVersion = SemVersion.Parse(installedPackage.Version, SemVersionStyles.OptionalMinorPatch);
                    var remoteVersion = SemVersion.Parse(remote.Version, SemVersionStyles.OptionalMinorPatch);
                    var updateAvailable = remoteVersion.CompareSortOrderTo(installedVersion) > 0;

                    InstalledPackages.Add(new PackageViewModel(
                    installedPackage,
                    updateAvailable ? EPackageStatus.UpdateAvailable : EPackageStatus.Installed,
                    "ms-appx:///Assets/ControlImages/Acrylic.png"
                    ));
                }
            }
        }

        await SaveAsync();

        (App.StartupWindow as MainWindow).Navigate(typeof(InstalledPage), "");
    }

    #endregion

    #region Dictionary

    private bool TryGetInstalled(string id, out PackageViewModel installed)
    {
        installed = InstalledPackages.FirstOrDefault(x => x.Name == id);
        return installed != null;
    }

    public bool TryGetRemote(PackageModel model, out RemotePackageViewModel remote) => TryGetRemote(model.Name, out remote);

    public bool TryGetRemote(string id, out RemotePackageViewModel remote)
    {
        remote = RemotePackages.FirstOrDefault(x => x.Name == id);
        return remote != null;
    }

    #endregion

    #region Install

    public async Task<string> GetLatestVersionAsync(RemotePackageModel model, bool prerelease = false)
    {
        if (prerelease)
        {
            throw new NotImplementedException();
        }

        var releaseUrl = $@"{model.Url}/releases/latest";
        var response = await _httpClient.GetAsync(new Uri(releaseUrl));

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not find release");
            return null;
        }

        // get tag
        return response?.RequestMessage.RequestUri.LocalPath.Split('/').Last();
    }

    public async Task<bool> RemoveAsync(PackageModel model)
    {
        var installed = InstalledPackages.FirstOrDefault(x => x.GetModel() == model);
        if (installed is not null)
        {
            InstalledPackages.Remove(installed);
            await SaveAsync();
            return true;
        }
        else
        {
            _logger.LogError("Could not find {model}", model.Name);
            return false;
        }
    }

    public async Task<bool> InstallAsync(PackageModel package, string installPath) => TryGetRemote(package.Name, out var remote) && await InstallAsync(remote.GetModel(), installPath);

    public async Task<bool> InstallAsync(RemotePackageModel package, string installPath)
    {
        if (string.IsNullOrEmpty(installPath))
        {
            _logger.LogError("Install location does not exist: {installPath}", installPath);
            return false;
        }

        _notificationService.StartIndeterminate();
        _notificationService.DisplayBanner("Installing", $"Downloading {package.Name}. Please wait ...", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational);

        // get remote version
        var version = await GetLatestVersionAsync(package);
        if (string.IsNullOrEmpty(version))
        {
            _logger.LogError("No remote version found");
            return false;
        }

        // check if versioned file is installed
        var testInstallerPath = Path.Combine(Path.GetTempPath(), package.AssetPattern.Replace(@".*", version));
        var installerPath = testInstallerPath;

        if (!File.Exists(testInstallerPath))
        {
            // query github api
            var header = $"wolvenkit";
            var ghClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(header));
            IEnumerable<Octokit.ReleaseAsset> asset = new List<Octokit.ReleaseAsset>();
            try
            {
                var owner = package.Url.Split('/')[^2];
                var repo = package.Url.Split('/').Last();

                var releases = await ghClient.Repository.Release.GetAll(owner, repo);
                var latest = releases[0];
                var assets = latest.Assets.ToList();
                asset = assets.Where(x => Regex.IsMatch(x.Name, package.AssetPattern));
            }
            catch (Octokit.ApiException)
            {
                // Prior to first API call, this will be null, because it only deals with the last call.
                var apiInfo = ghClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;
                var howManyRequestsCanIMakePerHour = rateLimit?.Limit;
                var howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
                var whenDoesTheLimitReset = rateLimit?.Reset; // UTC time

                _logger.LogInformation($"[Update] {howManyRequestsDoIHaveLeft}/{howManyRequestsCanIMakePerHour} - reset: {whenDoesTheLimitReset ?? whenDoesTheLimitReset.Value.ToLocalTime()}");
                _logger.LogError("API rate limit exceeded");

                return false;
            }

            if (!asset.Any())
            {
                _logger.LogError("No assets found to download");
                return false;
            }

            // download 
            var contentUrl = asset.First().BrowserDownloadUrl;
            installerPath = Path.Combine(Path.GetTempPath(), contentUrl.Split('/').Last());

            // TODO
            var remoteHash = "";
            string localHash = null;
            if (File.Exists(installerPath))
            {
                localHash = CalculateFileHash(installerPath);
            }

            if (localHash != remoteHash)
            {
                var response = await _httpClient.GetAsync(new Uri(contentUrl));
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Failed to respond to url: {contentUrl}", contentUrl);
                    return false;
                }

                await using var fs = new FileStream(installerPath, System.IO.FileMode.Create);
                // TODO report progress here
                await response.Content.CopyToAsync(fs);
            }
        }

        // install
        _notificationService.DisplayBanner("Installing", $"Installing {package.Name}. Please wait ...", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational);

        if (SettingsHelper.GetUseZipInstallers())
        {
            var installedFiles = await Task.Run(() => ExtractZip(installerPath, installPath));

            var installedPackage = new PackageModel(
            package.Name,
            version,
            installPath);

            InstalledPackages.Add(new(installedPackage, EPackageStatus.Installed, package.ImagePath));

        }
        else
        {
            // run installer
            try
            {
                using var p = new Process();
                p.StartInfo.FileName = installerPath;
                p.StartInfo.Arguments = $"/SILENT /DIR=\"{installPath}\"";
                p.Start();
                p.WaitForExit();

                var installedPackage = new PackageModel(
                   package.Name,
                   version,
                   installPath);

                InstalledPackages.Add(new(installedPackage, EPackageStatus.Installed, package.ImagePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing Wolvenkit.Installer.Package");
            }



            _ = App.StartupWindow.DispatcherQueue.TryEnqueue(_notificationService.Completed);
        }

        // save
        await SaveAsync();

        return true;
    }

    /// <summary>
    /// Calculates the SHA256 hash of a physical file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string CalculateFileHash(string filePath)
    {
        using var mySHA256 = SHA256.Create();
        var fInfo = new FileInfo(filePath);
        using var fileStream = fInfo.Open(FileMode.Open);
        try
        {
            fileStream.Position = 0;
            return string.Concat(mySHA256.ComputeHash(fileStream).Select(b => b.ToString("X2")));
        }
        catch (IOException e)
        {
            _logger.LogWarning("I/O Exception: {msg}", e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning("Access Exception: {msg}", e.Message);
        }

        return null;
    }

    private List<string> ExtractZip(string zipPath, string extractPath)
    {
        // extract from temp path
        var files = new List<string>();

        if (!Directory.Exists(extractPath))
        {
            Directory.CreateDirectory(extractPath);
        }

        try
        {
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                extractPath += Path.DirectorySeparatorChar;
            }

            using var archive = ZipFile.OpenRead(zipPath);

            var progress = 0;
            _ = App.StartupWindow.DispatcherQueue.TryEnqueue(() =>
            {
                _notificationService.StopIndeterminate();
                _notificationService.Report(0.1);
            });

            var total = archive.Entries.Count;
            foreach (var entry in archive.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                if (destinationPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    if (!Directory.Exists(destinationPath))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                }
                else
                {
                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that are case-insensitive.
                    if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    {
                        var folder = Path.GetDirectoryName(destinationPath);
                        if (folder != null)
                        {
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }
                        }

                        entry.ExtractToFile(destinationPath, true);
                        files.Add(destinationPath);
                    }
                }

                progress++;
                _ = App.StartupWindow.DispatcherQueue.TryEnqueue(() => _notificationService.Report(progress / (float)total));

            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to extract plugin zip: {zipPath}", zipPath);

            files.Clear();
        }

        _ = App.StartupWindow.DispatcherQueue.TryEnqueue(_notificationService.Completed);
        return files;
    }

    #endregion

}

