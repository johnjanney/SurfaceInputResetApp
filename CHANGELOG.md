# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-03-22

### Added

- WPF application targeting .NET 10 (Windows desktop).
- Automatic enumeration of `Keyboard`, `Mouse`, and `HIDClass` PnP devices via PowerShell.
- Confidence-based scoring system that prioritizes Surface, Type Cover, and touchpad devices.
- One-click device restart (disable/re-enable) using `pnputil.exe`.
- "Select suggested" button to auto-select highest-confidence devices (score >= 6).
- "Copy selected instance IDs" for manual troubleshooting.
- Timestamped status log panel.
- Application manifest requiring administrator elevation.
