# Changelog

All notable changes to this project will be documented in this file.

<br>

## [1.5.1.0] - 2024-03-08

### Added

- New Download Server - [LexouilleTM](https://github.com/LexouilleTM/yuzu-mods-archive) - [special thanks](https://github.com/LexouilleTM/yuzu-mods-archive/issues/1)
  - Forked the Switch-Mods Wiki with all Mods reuploaded.

### Changed

- Small code refactor to improve code readability

<br>

## [1.5.0.0] - 2024-03-06

### Changed

- `YuzuModDownloader` has now been rebranded to `SwitchEmuModDownloader`
  - All relevant code has been refactored
- UI tweaks
- All previous versions of `YuzuModDownloader` will be invalidated and going forward, 1.5.0.0+ will be supported.
- Updated Avalonia framework from `11.0.9` to `11.0.10` to fix some bugs

### Fixed

- `Official Switch-Mods Repo` is now working again.

<br>

## [1.4.2.0] - 2024-02-18

### Added

- YuzuModDownloader now skips downloading a particular mod if it cannot connect to the remote server

### Changed

- Improved `theboy181` download server choice performance
- Upgraded all project packages to their latest versions for even better performance
- Rewrote several classes & methods for improved performance
- Removed redundant code & shrunk overall codebase
- Small rewrite of `AppUpdater.cs` to improve startup performance
- Optimised several methods further to utilise await/async and increase performance

### Fixed

- Crashing when download URL host times out - thanks [@brolio](https://github.com/amakvana/YuzuModDownloader/issues/37)

<br>

## [1.4.1.0] - 2024-02-07

### Added

- Show Game Titles in Download Confirmation Dialog - thanks [@kathyrollo](https://github.com/amakvana/YuzuModDownloader/issues/36)
  - If Games have been detected, the `Show Details` button will be enabled and games which mods have been downloaded for will appear in here.
- Detection for Flatpak installations of Yuzu - thanks [@soufrabi](https://github.com/amakvana/YuzuModDownloader/pull/35), [@rotanigroc](https://github.com/amakvana/YuzuModDownloader/issues/29)

### Changed

- Upgraded YuzuModDownloader from .NET 7 to .NET 8 - LTS and much better performance
- Upgraded 7z.zip from 22.01 to 23.01 - better performance
- Upgraded all project packages to their latest versions for better performance and to fix some underlying framework bugs
- Rewrote `GetUserDirectoryPath()` and `GetModPath()` within `ModDownloader.cs` to better handle paths for Linux and Windows - thanks [@soufrabi](https://github.com/amakvana/YuzuModDownloader/pull/35)
- GUI tweaks to improve UX
- Removed redundant code from codebase
- Optimised several methods further to utilise await/async and increase performance.
- Latest [Microsoft Edge Chromium](https://www.microsoft.com/en-us/edge/download) or [Google Chrome](https://www.google.com/chrome/) is recommended to be installed (Linux - deb only) for increased `PuppeteerSharp` performance

### Fixed

- Error in parsing `load_directory` within `qt-config.ini` - thanks [@anzix, @garbear](https://github.com/amakvana/YuzuModDownloader/issues/34)
- `theboy181` download server is working again, now uses `PuppeteerSharp` to scrape repo as GitHub now uses `React.JS` - thanks [@Pacomss, @SebinaLukawa97, @spyro2000, @W13N3N](https://github.com/amakvana/YuzuModDownloader/issues/33)

<br>

## [1.4.0.0] - 2023-06-04

### Added

- New Download Server - [HolographicWings TOTK](https://github.com/HolographicWings/TOTK-Mods-collection/)
  - Automatically detects current game version and pulls game version mod package
- Complete rewrite in .NET 7 & AvaloniaUI. Goodbye .NET Framework 4.8.1 - cross-compatible UI, better performance, newer c# features and increased support for await/async model
- Now supports Linux and SteamOS without needing Proton - thanks [@ssorgatem](https://github.com/amakvana/YuzuModDownloader/issues/19), [@mymeyers03](https://github.com/amakvana/YuzuModDownloader/issues/19), [@MGThePro](https://github.com/amakvana/YuzuModDownloader/issues/19), [@Redhawk18](https://github.com/amakvana/YuzuModDownloader/issues/19), [@ProNoob135](https://github.com/amakvana/YuzuModDownloader/issues/19), [@Rikj000](https://github.com/amakvana/YuzuModDownloader/issues/19) and [@cimba007](https://github.com/amakvana/YuzuModDownloader/issues/19)

### Changed

- `YuzuModDownloader` now bundles all .dll dependencies inside the executable - no need to drag any additional .dll files into `YuzuModDownloader`'s working directory
- `AppUpdater.cs` now handles disconnections and VPN killswitches, terminates the app
- Optimised several methods to utilise await/async.
- Retired `WebClient`, now utilises `IHttpClientFactory` to pull data - faster and memory efficient
- Further GUI tweaks to improve UX
  - ProgressText now streamlined inside ProgressBar
- Removed redundant code from codebase

### Fixed

- Better error handling

<br>

## [1.3.1.0] - 2022-12-21

### Changed

- Tweaked `ReadGameTitlesDatabaseAsync` method within `TheBoy181ModDownloader.cs` and `OfficialYuzuModDownloader.cs`
  - Reads XML files asynchronously - improves reading performance and stops GUI main thread locking up
  - Ignores XML comments within XML files - prevents comment tags causing parsing errors

### Fixed

- [Issue #21](https://github.com/amakvana/YuzuModDownloader/issues/21) Unhandled Exception error when downloading mods with URLs containing certain special characters from `theboy181`'s server

<br>

## [1.3.0.0] - 2022-12-18

### Added

- New Download Server - [theboy181](https://github.com/theboy181/switch-ptchtxt-mods/) - [special thanks](https://github.com/theboy181/switch-ptchtxt-mods/issues/15)
- Game version detection for Mod Downloader - currently only compatible with [theboy181](https://github.com/theboy181/switch-ptchtxt-mods/)'s Download Server

### Changed

- Complete refactor of `ModDownloader.cs` to allow modularity via Inheritance
  - `OfficialYuzuModDownloader.cs` handles [Yuzu Switch-Mods Wiki](https://github.com/yuzu-emu/yuzu/wiki/Switch-Mods) mods
  - `TheBoy181ModDownloader.cs` handles [theboy181](https://github.com/theboy181/switch-ptchtxt-mods/) mods
- Merged `DirectoryUtilies.cs` into `ModDownloader.cs` as the former became redundant Utility class
- `Game.cs` now used to pass Game information between methods rather than singular method parameters
- UI Tweaks
  - Moved `General Options` from main GUI, into MenuStrip
  - General options can now be found under `Options` > `General`

### Fixed

- Race condition between Extracting Mods and Deleting Mod Archives - now waits for all 7z processes to exit before processing archive deletion
- Total Games downloaded MessageBox dialog fixed - now only counts the directories containing mods, rather than total directories within `/load/`

<br>

## [1.2.0.0] - 2022-09-28

### Added

- YuzuModDownloader now uses locally installed 7-Zip if present

### Changed

- Migrated 7z.zip hosting from CloudMe to GitHub
- Upgraded 7z.zip from 19.00 to 22.01 - better performance
- Rewrote `ReadGameTitleIdDatabase` method to improve performance - now uses `XmlReader`
- Updated `HtmlAgilityPack` binaries from `1.11.42` to `1.11.46`

### Fixed

- [Issue #17](https://github.com/amakvana/YuzuModDownloader/issues/17) Unhandled Exception error for some users when downloading 7-Zip prerequisite

<br>

## [1.1.0.0] - 2022-04-09

### Added

- Option to clear Downloaded Mod Archives after being unpacked - checked by default
- Option to `Check for Updates` within Yuzu Mod Downloader

### Changed

- Yuzu Mod Downloader update module will now allow overlapping of versions before older versions are no longer supported.
- Rewrote method to identify Mod Directory root - now reads `qt-config.ini` and uses the `load_directory` key value
- UI Tweaks

### Fixed

- Bug where Mods did not extract properly when filepaths contained spaces
- Rare error parsing `GameTitleIDs.xml`
- Other minor bug fixes

<br>

## [1.0.0.0] - 2021-03-21

### Added

- Initial release
