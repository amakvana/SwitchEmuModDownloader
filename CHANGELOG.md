# Changelog 

All notable changes to this project will be documented in this file.

<br>

## [1.1.0.0] - 2022-04-09
### Added
* Option to clear Downloaded Mod Archives after being unpacked - checked by default
* Option to `Check for Updates` within Yuzu Mod Downloader
### Changed 
* Yuzu Mod Downloader update module will now allow overlapping of versions before older versions are no longer supported.
* Rewrote method to identify Mod Directory root - now reads `qt-config.ini` and uses the `load_directory` key value
* UI Tweaks
### Fixed
* Bug where mods did not extract when filepaths contained spaces 
* Rare error parsing GameTitleIDs.xml
* Other minor bug fixes 

<br>

## [1.0.0.0] - 2021-03-21
### Added
* Initial release 
### Changed 
### Fixed