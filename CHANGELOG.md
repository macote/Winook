# Changelog

## 1.4.0 (2026-06-14)

### Breaking

- Make `MessageReceiver` internal. This type was implementation detail for native hook message transport and is no longer part of the public API.

### Added

- Add send timestamps to mouse and keyboard message event args.

### Changed

- Remove the native asio dependency and replace TCP message transport with named pipes.
- Package native support binaries under runtime native asset folders and copy them through build targets.
- Add NuGet package deployment and smoke test scripts.
- Add a NuGet package readme.
- Update desktop test diagnostics with message latency and recent message lists.

### Fixed

- Fix native hook teardown so hooks are uninstalled reliably without blocking DLL unload.

## 1.3.2 (2021-05-08)

### Fixed

- #31: Fix Dispose() issue in MouseHook

### Added

- #30: Add RemoveAllHandlers()

## 1.3.1 (2021-04-30)

### Fixed

- #28: Fix Dispose() issue in KeyboardHook

## 1.3.0 (2021-04-23)

### Added

- #26: Add Shift, Control, Alt state to mouse messages

## 1.2.1 (2021-04-15)

### Fixed

- #24: Fix Shift, Control and Alt handling

## 1.2.0 (2021-03-02)

### Added

- #22: Add code to identify XButtons

## 1.1.1 (2021-02-09)

### Fixed

- #21: Restore asio code

## 1.1.0 (2020-06-24)

### Changed

- #20: Remove asio dependency

## 1.0.1 (2020-06-24)

### Added

- #17: Support .NET Standard 2.0
