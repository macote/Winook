![](doc/Winook_readme.png)

[![nuget][nuget-badge]][nuget-url]

[nuget-badge]: https://img.shields.io/badge/nuget-v0.1.0-blue.svg
[nuget-url]: https://www.nuget.org/packages/Winook

Winook is a Windows library that let you install thread-level hooks inside processes. This library offers an alternative to solutions that use global hooks. With thread-level hooks, performance and management issues can be avoided.

# Information

Winook uses host processes and dll injection to setup hooks. Both 32-bit and 64-bit support host applications and dlls are included. Bitness awareness is supported dynamically.

# Installation

## NuGet

```
Install-Package Winook
```

# Usage

``` csharp
_process = Process.Start(@"c:\windows\notepad.exe");
//_process = Process.Start(@"c:\windows\syswow64\notepad.exe"); // works also with 32-bit

_mouseHook = new MouseHook(_process.Id);
_mouseHook.MessageReceived += MouseHook_MessageReceived;
_mouseHook.InstallAsync();

...

private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
{
    Debug.WriteLine($"Mouse Message Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Delta: {e.Delta}");
}
```

Keyboard hooking works in a similar way.

# License

MIT

# Acknowledgements

Thanks to [C1rdec](https://github.com/C1rdec) for the inspiration and motivation. I created this initially to help him with his [project](https://github.com/C1rdec/Poe-Lurker).