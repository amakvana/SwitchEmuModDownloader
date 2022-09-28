# Changelog

All notable changes to this project will be documented in this file.

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

- Unhandled Exception error for some users when downloading 7-Zip prerequisite

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
