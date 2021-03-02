![](doc/Winook_readme.png)

[![nuget][nuget-badge]][nuget-url]

[nuget-badge]: https://img.shields.io/badge/nuget-v1.2.0-blue.svg
[nuget-url]: https://www.nuget.org/packages/Winook

Winook is a Windows library that let you install thread-level hooks inside processes. This library offers an alternative to solutions that use global hooks. With thread-level hooks, performance and management issues can be avoided.

# Information
Winook uses host processes and dll injection to setup hooks. Both 32-bit and 64-bit support host applications and dlls are included.

## Features
- Works in .NET Framework and .NET Core applications
- 32-bit and 64-bit dynamic support
- Mouse and keyboard specific message handlers
  - Extensive keyboard modifiers support
- Mouse message filtering at dll level
  - Ability to ignore mouse move messages to improve performance

## Application
- Game Helpers 
  - Winook can be used in game helpers. In-game keyboard and mouse events can be tied to event handlers in your application. For example, Winook is used in [Poe Lurker](https://github.com/C1rdec/Poe-Lurker) to provide additional trading functionality.
- Application Inspection
  - Winook can be used as a telemetry inspection tool.
- Other Uses
  - Please share how you use Winook!

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
//_mouseHook = new MouseHook(_process.Id, MouseMessageTypes.IgnoreMove);
_mouseHook.MessageReceived += MouseHook_MessageReceived;
//_mouseHook.LeftButtonUp += MouseHook_LeftButtonUp;
//_mouseHook.AddHandler(MouseMessageCode.NCLeftButtonUp, MouseHook_NCLButtonUp);
_mouseHook.InstallAsync();

_keyboardHook = new KeyboardHook(_process.Id);
_keyboardHook.MessageReceived += KeyboardHook_MessageReceived;
//_keyboardHook.AddHandler(KeyCode.Y, Modifiers.ControlShift, KeyboardHook_ControlShiftY);
_keyboardHook.InstallAsync();

...

private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
{
    Debug.WriteLine($"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Delta: {e.Delta}; XButtons: {e.XButtons}");
}

private void KeyboardHook_MessageReceived(object sender, KeyboardMessageEventArgs e)
{
    Debug.Write($"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; ");
    Debug.WriteLine($"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}");
}

```

# License
MIT

# Acknowledgements
Thanks to [C1rdec](https://github.com/C1rdec) for the inspiration and motivation. I created this initially to help him with his [project](https://github.com/C1rdec/Poe-Lurker).
