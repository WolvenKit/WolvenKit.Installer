@echo off

rem publish as self-contained
dotnet publish .\Wolvenkit.Installer\Wolvenkit.Installer.csproj -o publish -c Release --self-contained true -r win10-x64 -p:Platform=x64

pause