# WolvenKit.Installer
Standalone Installer for WolvenKit 


## Building

### Self-contained
To publish as self-contained use:
`dotnet publish .\Wolvenkit.Installer\Wolvenkit.Installer.csproj -o publish -c Release --self-contained true -r win10-x64 -p:Platform=x64`

Why? See these obscure appsdk bugs: https://github.com/dotnet/maui/issues/5886, https://github.com/microsoft/WindowsAppSDK/discussions/3026
