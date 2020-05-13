![](doc/Winook_readme.png)

Winook is a Windows library that let you install thread-level hooks inside processes.

# Information

Winook uses a host process and dll injection to setup hooks.

# Installation

## NuGet

Winook is currently in pre-release stage.
```
Install-Package -IncludePrerelease Winook
```

# Usage

``` csharp
_process = Process.Start(@"c:\windows\notepad.exe");
//_process = Process.Start(@"c:\windows\syswow64\notepad.exe"); // works also with 32-bit

_mouseHook = new MouseHook(_process.Id);
_mouseHook.MessageReceived += MouseHook_MessageReceived;
_mouseHook.Install();

...

private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
{
    Debug.WriteLine($"Mouse Message Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Delta: {e.Delta}");
}
```

Keyboard hooking works in a similar way.

# Known Issues

- The process bitness detection may fail. If the process bitness is known in advance, it is possible to specify it using an overloaded constructor.
- Keyboard and mouse hooking cannot be used simultaneously.

# License

MIT
