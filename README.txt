GrimaceTweaks v1.0.0
====================

A Windows WPF gaming optimizer inspired by the layout and workflow of modern PC tweak utilities.

IMPORTANT: This project is written in C#/.NET WPF. It does not use Python.

Highlights
- EMTweaks-style dark/neon-purple desktop interface
- Safe one-click optimization with a required verified safety snapshot
- Universal game selector and custom .exe support
- Game profiles for Fortnite, VALORANT, CS2, Apex Legends, Rocket League and Roblox
- Hardware detection
- Game launch/folder helpers
- Region latency testing
- Optional advanced Windows/network modules with confirmation gates
- Restore-last-backup support
- No DLL injection, game-memory editing, anti-cheat modification, or Defender disabling

Safety philosophy
The default Optimize My PC path is intentionally conservative. Advanced changes such as HAGS, high-performance power, network adapter changes, service changes, and timer-resolution changes are not silently applied.

Build
Use GitHub Actions: Actions -> Build Windows x64 EXE -> Run workflow.
The workflow publishes a self-contained Windows x64 EXE named GrimaceTweaks.exe.
