# Yuzu Mod Downloader Guide

## Requirements
- Latest [7-Zip](https://www.7-zip.org/a/7z2201-x64.msi) installed.
- Latest [Microsoft .NET Framework](https://go.microsoft.com/fwlink/?linkid=2088631) installed.
- Latest [Visual C++ X64 Redistributable](https://aka.ms/vs/16/release/vc_redist.x64.exe) installed.
- Latest [Yuzu](https://yuzu-emu.org/downloads/) installed, setup and [configured](https://youtu.be/kSVlTC1mO9w).

## Methodology 
* Reads current games imported into Yuzu
* Reads https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods
* Scans game library to see available mods
* Fetches the mod URL's for current games
* Downloads & extracts it into the Yuzu Mod folder

## Initial Setup  
1. Ensure Yuzu is up-to-date and [fully configured](https://www.youtube.com/watch?v=93xsKERji60) (gamepaths set)
2. Extract contents of the YuzuModDownloader.zip file into your Yuzu root folder
3. Run YuzuModDownloader.exe

## Usage 
1. Run YuzuModDownloader.exe
2. Configure your preferred Options
3. Choose a Download Server
4. Click on ```Download Yuzu Game Mods``` 
