# PulseForge

PulseForge is a Windows x64 C#/.NET WPF gaming optimizer focused on Fortnite first and universal Windows games second.

## Design philosophy

- Safe by default
- Reversible registry snapshots
- No Python
- No DLL injection
- No game-memory editing
- No anti-cheat modification
- No disabling Microsoft Defender or Windows Update
- Advanced system changes are not part of the one-click safe profile

## Fortnite-first features

- Game Mode
- Background capture reduction
- Safe temp cleanup
- Session-only High process priority
- Epic endpoint latency testing
- Fortnite launcher and config shortcuts
- Universal EXE selection for other games

## Build

GitHub Actions builds a self-contained Windows x64 executable.

The application requests administrator privileges because some Windows settings require elevation.
