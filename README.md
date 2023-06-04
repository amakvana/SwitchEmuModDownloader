![GitHub OS](https://img.shields.io/static/v1?label=OS&message=WINDOWS%20|%20LINUX%20|%20STEAMOS&style=for-the-badge&color=brightgreen)
![GitHub Release](https://img.shields.io/github/v/release/amakvana/YuzuModDownloader?style=for-the-badge)
<br>
![GitHub license](https://img.shields.io/github/license/amakvana/YuzuModDownloader?style=for-the-badge)
![GitHub repo size](https://img.shields.io/github/repo-size/amakvana/YuzuModDownloader?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/amakvana/YuzuModDownloader/total?style=for-the-badge)

# Yuzu Mod Downloader

A One-Click Yuzu Game Mod downloader for Switch games.

Perfect for those who need to download Switch enhancement/workaround mods from the [Yuzu Switch-Mods Wiki](https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods) and Alternative Mirrors for their games.

Compatible with both the standalone and installed versions of Yuzu.

![YuzuModDownloaderAnimated](images/ymd-1400.gif)

## Table of Contents

- [Overview](#overview)
  - [Methodology](#methodology)
  - [Usage](#usage)
- [Download](#downloads)
- [Installation - Windows Tutorial](#windows)
- [Installation - Linux Tutorial](#linux)
- [Installation - YouTube Tutorial](#installation---youtube-guide)
- [User Guide](https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md)
- [Acknowledgements](#acknowledgements)

## Overview

### Methodology

1. Reads current games imported into Yuzu.
2. Reads selected `Download Server`
3. Scans game library to see available mods.
4. Fetches the mod URL's for current games.
5. Downloads & extracts it into the defined Yuzu Mod folder.

### Usage

See [GUIDE](https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md)

https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md

## Downloads

https://github.com/amakvana/YuzuModDownloader/releases/latest

Requires:

- Latest [7-Zip](https://www.7-zip.org/a/7z2201-x64.msi) installed.
- Latest .NET 7 Desktop Runtime for [Windows](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.5-windows-x64-installer) or [Linux](https://learn.microsoft.com/en-gb/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) installed.
- Latest [Visual C++ X64 Redistributable](https://aka.ms/vs/16/release/vc_redist.x64.exe) installed.
- Latest [Yuzu](https://yuzu-emu.org/downloads/) installed, setup and [configured](https://youtu.be/kSVlTC1mO9w).

## Installation

YuzuModDownloader does not require Administrator privileges to run.

Refer to the [GUIDE](https://github.com/amakvana/YuzuModDownloader/blob/main/GUIDE.md) for usage instructions.

### Windows

Extract the entire contents of the `YuzuModDownloader-X.X.X.X-Windows-x64.zip` file and place it into your `Yuzu Root Folder` (this is the folder `yuzu.exe` resides).

![YuzuModDownloaderSetupWindowsAnimated](images/ymd-setup-windows.gif)
![YuzuModDownloaderSetupWindows](images/ymd-setup-windows-2.png)

### Linux

Extract the entire contents of the `YuzuModDownloader-X.X.X.X-Linux-x64.zip` file onto your desktop then run the following commands within Terminal:

```
cd ~/Desktop
chmod +x YuzuModDownloader
./YuzuModDownloader
```

You can double click on YuzuModDownloader to execute it, once the first two commands above have been ran (3rd is to run it from Terminal).

![YuzuModDownloaderSetupLinuxAnimated](images/ymd-setup-linux.gif)

## Installation - YouTube Guide

[![Watch the video](images/ymd-youtube.jpg)](https://youtu.be/q_2ivWN07Kw)

## Acknowledgements

Special thanks to the following:

- [Yuzu Team](https://yuzu-emu.org/) - Nintendo Switch Emulator Developers
- [TheBoy181](https://github.com/theboy181/) - [Alternative Mods Download Mirror](https://github.com/theboy181/switch-ptchtxt-mods)
- [HolographicWings](https://github.com/HolographicWings) - [Alternative Mods Download Mirror](https://github.com/HolographicWings/TOTK-Mods-collection)
- [Agus Raharjo](https://www.iconfinder.com/agusraharj) - Icons
- [Mr. Sujano](https://www.youtube.com/watch?v=q_2ivWN07Kw) - YouTube Installation Guide
