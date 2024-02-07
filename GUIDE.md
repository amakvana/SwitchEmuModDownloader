# Yuzu Mod Downloader Guide

## Requirements

- Latest [7-Zip](https://www.7-zip.org/a/7z2301-x64.msi) installed.
- Latest .NET 8 Desktop Runtime for [Windows](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.1-windows-x64-installer) or [Linux](https://learn.microsoft.com/en-gb/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) installed.
- Latest [Microsoft Edge Chromium](https://www.microsoft.com/en-us/edge/download) or [Google Chrome](https://www.google.com/chrome/) installed. Linux - deb only.
- Latest [Visual C++ X64 Redistributable](https://aka.ms/vs/16/release/vc_redist.x64.exe) installed.
- Latest [Yuzu](https://yuzu-emu.org/downloads/) installed, setup and [configured](https://youtu.be/kSVlTC1mO9w).

## Methodology

- Reads current games imported into Yuzu
- Reads selected `Download Server`
- Scans game library to see available mods
- Fetches the mod URL's for current games
- Downloads & extracts it into the Yuzu Mod folder

## Initial Setup

Ensure Yuzu is up-to-date and [fully configured](https://www.youtube.com/watch?v=93xsKERji60) (gamepaths set)

### Windows

Extract the entire contents of the `YuzuModDownloader-X.X.X.X-Windows-x64.zip` file and place it into your `Yuzu Root Folder` then refer to the [GUIDE](https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md).

![YuzuModDownloaderSetupWindowsAnimated](images/ymd-setup-windows.gif)
![YuzuModDownloaderSetupWindows](images/ymd-setup-windows-2.png)

### Linux

Extract the entire contents of the `YuzuModDownloader-X.X.X.X-Linux-x64.zip` file onto your desktop.
Run the following commands within Terminal

```
cd ~/Desktop
chmod +x YuzuModDownloader
./YuzuModDownloader
```

You can double click on YuzuModDownloader to execute it, once the first two commands above have been ran (3rd is to run it from Terminal) then refer to the [GUIDE](https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md).

![YuzuModDownloaderSetupLinuxAnimated](images/ymd-setup-linux.gif)

## Usage

1. Run YuzuModDownloader.exe
2. Configure your preferred Options from the MenuStrip
3. Choose a Download Server from the Dropdown
4. Click on `Download Yuzu Game Mods`
