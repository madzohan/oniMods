# Changelog

All notable changes to RePrint mod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2022-12-19

### Added

- Re-roll blueprints from telepad on "reject all" confirming

## [1.1.0] - 2022-12-21

### Added

- Suppress the confirmation popup on "reject all"

## [1.2.0] - 2022-12-26

### Added

- Standard "Reroll" button above each suggested duplicant

## [1.2.1] - 2023-01-08

### Fixed

- "reject all" behaviour: now it keeps blueprints after all rejected and ESC pressed, but unpause is required (in case game was paused)
- CLRF endings replaced with LF

## [1.3.0] - 2023-02-02

### Added
- added Reroll button for care packages
- reverted vanilla "Reject All" button behavior (but with suppressed confirmation)

## [1.3.1] - 2023-02-03

### Fixed
- "Tried to add minions beyond the allowed limit" exception

## [1.4.0] - 2023-02-08

### Added
- added another button "Reshuffle All" which reshuffles all containers
- overridden ImmigrantScreen.InitializeContainers to set number of dup&package containers

## [1.4.1] - 2023-02-22

### Fixed
- "mod adds carepackages, even if you disabled them in your game"
  Now mod respects this setting on game creation step

## [1.4.2] - 2024-08-04

### Fixed
- fixed reshuffleAllBtn via changing reflection binding attribute, since game devs changed CharacterContainer.Reshuffle method from private to public