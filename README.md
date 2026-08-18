# mouse-nudge

A standalone Windows system tray app that periodically nudges the mouse cursor to keep the machine from being considered idle — no installer, no admin rights, just run the `.exe`.

## What it does

mouse-nudge sits in the system tray and, at a configurable interval, moves the mouse cursor a few pixels in a random direction (and back). This prevents:

- the screen from locking
- the machine from going to sleep
- presence status (Teams, Slack, …) from switching to "away"

## Goals

- **Portable / non-installable** — a single `.exe` that runs from any folder (Downloads, USB stick, network share). No setup, no registry entries, no admin rights.
- **Tray-only** — no window cluttering the desktop. All interaction happens through the tray icon's context menu.
- **Simple controls** — start / stop, configurable interval, exit.
- **Two modes**
  - **Nudge mode**: synthesizes real mouse input via the Win32 `SendInput` API, moving the cursor a few pixels in a random direction. Needed when an app specifically watches for input events.
  - **Keep-awake mode**: calls `SetThreadExecutionState` to suppress idle detection without moving the cursor at all. The cleaner option when the goal is only "don't lock / don't sleep".

## Tech

- C# / WinForms, targeting `net10.0-windows`
- Win32 interop (`user32.dll` / `kernel32.dll`) for input synthesis and power state
- Published as a self-contained single-file executable, so the target machine needs no .NET runtime installed

## Development

Run from source:

```
dotnet run
```

Build the portable single-file exe:

```
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
```

The resulting `.exe` lands in `bin\Release\net10.0-windows\win-x64\publish\` and can be copied anywhere and started directly.

## Status

- [x] Blank WinForms project scaffolded and verified to run
- [x] Tray icon with context menu (start / stop / exit)
- [x] Timer-based random cursor nudge via `SendInput`
- [x] Configurable interval
- [x] Keep-awake-only mode via `SetThreadExecutionState`
- [ ] Single-file publish verified on a clean machine
