GRIMACE OPTIMIZER V6 — SAFE UNIVERSAL / HONE-INSPIRED

Windows gaming optimizer with a universal game selector and a safety-first one-click OPTIMIZE ANY GAME workflow.

SAFETY-FIRST V6
- One-click optimization now stops if a verified Windows restore point + registry snapshot cannot be created.
- One-click mode uses a conservative Windows Balanced power plan, Game Mode, Game DVR capture reduction, temp cleanup and selected-game priority only.
- HAGS, visual-effect changes, timer tuning, network tuning, MSI, telemetry, Xbox/OneDrive changes and other deeper modules are NOT applied automatically.
- Advanced modules require explicit confirmation and another verified safety snapshot.
- Registry backups are stored under %LOCALAPPDATA%\GrimaceOptimizer\Backups.
- Restore-last-registry-backup is available in the Performance Center.

FEATURES
- Universal game selector and custom .exe support
- Fortnite, VALORANT, Roblox, CS2, Apex Legends, Rocket League and other presets
- Temporary low-latency timer request
- Optional MSI optimization for supported Display/Network devices
- Safe TCP/RSS network baseline and optional adapter offload tuning
- Optional telemetry, OneDrive and Xbox background reductions
- Boost-Up maintenance: temp cleanup + Windows Optimize Drives
- Optional DISM + SFC repair
- Existing hardware detection and endpoint latency testing
- Anti-cheat-safe design: no DLL injection, game-memory editing, packet routing or anti-cheat modification

WHAT V6 DELIBERATELY DOES NOT DO
The default workflow does not disable Windows Defender, Windows Update, audio, SysMain, Windows Search, core networking, or other critical daily-use services. It also does not claim that registry/network tweaks guarantee FPS or ping improvements.

IMPORTANT SAFETY NOTE
Restore points and registry backups are rollback aids, not absolute guarantees. Some changes require a restart, driver behavior varies, and Windows may remove old restore points. If an advanced change causes problems, use the restore controls and/or Windows System Restore.

HONE REFERENCE
HONE-MASTER-PLAN.md documents the Hone-inspired feature mapping and safe deployment order. Grimace is an independent implementation and does not copy Hone proprietary code.

BUILD
Build-GrimaceOptimizer.cmd
or
 dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

GITHUB ACTIONS
.github/workflows/build-windows.yml builds the Windows x64 self-contained artifact.

REQUIREMENTS
Windows 10/11. The app requests administrator privileges because some optional Windows and registry modules require elevation.
