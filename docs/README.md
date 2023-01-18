# WolvenKit.Installer
A simple and standalone app for managing (installing, updating, removing) different WolvenKit versions similar to the Visual Studio Installer app.

## Installation

### Winget

> :warning: This feature is currently unavailable

The WolvenKit.Installer app is also available on the [Windows Package Manager ](https://learn.microsoft.com/en-us/windows/package-manager/winget/#use-winget). Open terminal or command prompt and type in:

```cmd
winget install WolvenKit.Installer
```

### Manually

| Name | Latest Release  |
| ------- | ------------ |
| [WolvenKit.Installer](https://github.com/WolvenKit/WolvenKit.Installer/releases/latest/) | [![GitHub release (latest by date)](https://img.shields.io/github/v/release/WolvenKit/WolvenKit.Installer)](https://github.com/WolvenKit/WolvenKit.Installer/releases/latest) | 

- Download the latest `Wolvenkit.Installer.Package_x.x.x.x_x64.msixbundle` from GitHub above
- And double-cick the file to run the installer.

## Screenshots

![wolvenkit installer_01](https://user-images.githubusercontent.com/37657287/212540284-50a43778-8adf-4e26-92bd-397ca8380e6c.png)


## Building

### Self-contained
To publish as self-contained use:
`dotnet publish .\Wolvenkit.Installer\Wolvenkit.Installer.csproj -o publish -c Release --self-contained true -r win10-x64 -p:Platform=x64` (https://github.com/microsoft/WindowsAppSDK/discussions/3026)

and add 
`<EnablePreviewMsixTooling>true</EnablePreviewMsixTooling>` in the projects (https://github.com/dotnet/maui/issues/5886)
