# Surface Input Reset App

A Windows WPF utility that recovers frozen Surface Type Cover keyboards and trackpads by programmatically restarting the underlying PnP devices.

## Problem

Surface Pro and Surface Laptop users occasionally experience Type Cover or trackpad lockups where the keyboard and/or touchpad stop responding entirely. The typical fix is to detach and reattach the cover, or reboot. This tool provides a faster, software-only alternative.

## How It Works

1. Enumerates all present `Keyboard`, `Mouse`, and `HIDClass` PnP devices via PowerShell (`Get-PnpDevice`).
2. Scores each device on how likely it is to be a Surface input device (matching on name, manufacturer, and hardware IDs).
3. Presents a sorted list in a DataGrid, with highest-confidence Surface devices pre-selected.
4. On **Restart selected devices**, calls `pnputil.exe` to disable then re-enable each selected device, which forces the driver to reinitialize.

## Requirements

- **OS:** Windows 10 or later
- **Runtime:** .NET 10 (Windows desktop)
- **Privileges:** Must run as Administrator (enforced via app manifest)

## Building

```
dotnet build
```

## Running

```
dotnet run
```

Or launch the compiled executable from `bin/`. Windows will show a UAC prompt because the app requires elevation.

## UI Overview

| Area | Description |
|------|-------------|
| **Candidate devices** | DataGrid listing detected input devices, sorted by relevance score. |
| **Refresh list** | Re-scans PnP devices. |
| **Select suggested** | Re-selects devices with a confidence score of 6 or higher. |
| **Restart selected devices** | Disables then re-enables each selected device to reset the driver. |
| **Copy selected instance IDs** | Copies PnP instance IDs to the clipboard for manual troubleshooting. |
| **Status** | Timestamped log of actions and errors. |

## Project Structure

| File | Purpose |
|------|---------|
| `MainWindow.xaml` / `MainWindow.xaml.cs` | UI layout and interaction logic |
| `DeviceService.cs` | Device enumeration, scoring, and restart logic |
| `DeviceCandidate.cs` | Data model for a candidate device |
| `App.xaml` / `App.xaml.cs` | WPF application entry point |
| `app.manifest` | Requests `requireAdministrator` elevation |
| `SurfaceInputResetApp.csproj` | Project file targeting `net10.0-windows` with WPF |

## License

See repository for license details.
