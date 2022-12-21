# Changelog

All notable changes to this project will be documented in this file.

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
