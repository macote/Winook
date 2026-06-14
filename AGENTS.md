# Winook

## Project Shape

Winook is a Windows-only library with a managed .NET package and native C++ support binaries. Changes may need to
account for both `src/Winook` and the native projects under `src/Winook.Lib*`.

## Build and Verification

Use `dotnet build Winook.slnx` for managed build validation when possible.

For changes to native hook behavior, host process behavior, injection, or support binaries, also build both x86 and x64
native outputs before considering the change verified. Prefer `scripts/build-msys.ps1` for native verification; it builds
`src/Winook.Lib` and `src/Winook.Lib.Host` for both architectures and copies the outputs into
`src/Winook/winook.support/` by default.

The MSYS build script expects MSYS2 at `C:\msys64`, `MSYS2_ROOT` to be set, or `-MsysRoot` to be passed explicitly.
Use `-Clean` when stale native objects could affect the result, and `-SkipCopy` only when intentionally checking native
compilation without refreshing packaged support binaries.

Desktop test projects under `test/` are Windows UI/manual-style test apps. Do not assume they are automated unit tests
unless their project files or code clearly show otherwise.

When changing the desktop test apps, keep the WinForms and WPF variants behaviorally aligned: hook setup, message
filters, target process selection, and hook/unhook lifecycle should match unless a change intentionally targets one UI
framework. For WinForms layout changes, prefer updating `*.Designer.cs` so the UI remains inspectable in the designer.
For high-volume hook messages, do not post one WinForms UI callback per message; coalesce UI updates or otherwise avoid
flooding the WinForms message queue, because that can make hook/unhook appear flaky.

## Native Support Binaries

The files under `src/Winook/winook.support/` are packaged with the managed library. When native C++ projects change in a
way that affects runtime behavior, make sure the corresponding x86/x64 support binaries are refreshed and included in
the same change.

The native code no longer depends on the vendored ASIO submodule. Do not reintroduce `3rd/asio` or `.gitmodules` for
ordinary native changes.

Keep the architecture suffixes consistent:

- `.x86` maps to Win32 native builds.
- `.x64` maps to x64 native builds.

## Version and Release Metadata

When preparing a release, keep package/version metadata synchronized across `src/Winook/Winook.csproj`, `README.md`,
`CHANGELOG.md`, native resource files, and native manifests.

Check these native metadata files during release updates:

- `src/Winook.Lib/Winook.Lib.rc`
- `src/Winook.Lib.Host/Winook.Lib.Host.rc`
- `src/Winook.Lib.Host/application.manifest`

Do not update version numbers or changelog entries for ordinary implementation changes unless the task is explicitly
release-related.

## Scope Discipline

Keep managed API changes, native hook changes, support binary refreshes, and documentation updates grouped
intentionally. Avoid unrelated modernization or formatting churn while fixing behavior.

## Commits

Use the repository's existing commit subject style: short, plain-English summaries in imperative or sentence case.

Examples:

- `Use Character Map for desktop hook tests`
- `Modernize solution for latest .NET and C++ toolsets`
- `Bump version; update README`

Do not use Conventional Commits prefixes such as `feat:`, `fix:`, or `chore:`.
