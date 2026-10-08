# GrimaceTweaks — Hone-Inspired Master Plan

This document describes the Hone-inspired capabilities added to GrimaceTweaks V5. It is an independent implementation and does not copy Hone's proprietary code.

## 1. Pre-Optimization & Safety — V6 Safe Mode

Grimace V6 makes safety the gate for optimization. One-click mode refuses to proceed unless it can verify a Windows System Restore point command succeeded and all tracked registry exports were created. The registry areas are exported to:

`%LOCALAPPDATA%\\GrimaceTweaks\\Backups\\<timestamp>`

The UI also provides a restore-last-registry-backup action. Every advanced module asks for explicit confirmation and creates another verified snapshot before making its change. A restore point is a safety net, not a guarantee: Windows can remove old restore points and some changes require a restart. Hone's own support documentation similarly recommends creating a restore point before advanced optimizations.

## 2. Latency & Input Delay Reduction

The Performance Center includes:

- A temporary 1 ms Windows timer-resolution request while Grimace is running. It is released when Grimace closes instead of permanently changing a global timer policy.
- MSI support for present Display and Network devices where a MessageSignaledInterruptProperties key already exists.
- A Windows gaming baseline designed to reduce avoidable background work without injecting into games.
- Network I/O and adapter tuning modules.

Advanced interrupt/device changes are opt-in because driver behavior varies. Grimace does not modify game memory, inject DLLs, or hook anti-cheat processes.

## 3. System Debloating & Background Activity

Optional modules cover:

- Selected Windows telemetry reduction
- OneDrive startup/background process reduction without deleting files
- Xbox service pause/demand-start changes
- Game DVR/background capture disabled by the core gaming profile
- Windows Game Mode enabled by the core gaming profile

SysMain, Windows Search, Windows Update, Defender, audio services, and browser background processes are deliberately not disabled by the one-click profile. Those services can be important for stability, updates, security, or normal daily use and their gaming impact varies by machine.

## 4. Network & Ping Tuning

The safe network baseline keeps TCP auto-tuning at Windows `normal` and enables RSS. An advanced adapter module can disable supported Large Send Offload settings.

Grimace also retains endpoint latency testing. Ping cannot be guaranteed by registry tweaks: ISP routing, Wi-Fi quality, server distance, congestion, and game-server load remain outside the optimizer's control.

The project intentionally does not use the common “disable 20% reserved bandwidth” claim as a guaranteed FPS/ping fix.

## 5. Storage & System Maintenance

The Boost-Up-style maintenance section provides:

- User temp cleanup
- Windows Optimize Drives (`defrag C: /O`) so Windows chooses the appropriate SSD/HDD optimization
- DISM `/Online /Cleanup-Image /RestoreHealth`
- SFC `/scannow`

These are maintenance tools rather than guaranteed FPS boosters. Run repairs when troubleshooting system corruption or stability issues.

## 6. Game-Specific Integration

The Game Presets section mirrors the useful concept of a game library:

- Fortnite
- VALORANT
- Roblox
- Counter-Strike 2
- Apex Legends
- Rocket League
- Overwatch 2
- Grand Theft Auto V
- Minecraft Java
- Any custom `.exe`

Selecting a preset chooses the game profile and applies Windows-side performance controls. Grimace does not edit anti-cheat components or game memory.

## Free vs Premium: Hone's Current Model

Hone's current official pricing page says Free includes 10 optimization slots, with one additional slot per referral up to 15. Free includes Balanced Game Mode and standard Boost-Ups. Premium provides unlimited optimizations, premium-only optimizations, Custom/Performance Game Mode, premium game presets and Boost-Ups, no ads, and priority support.

These are Hone's product-plan details, not Grimace licensing tiers. Grimace remains an independent optimizer.

## V6 Safety Rules

- **Safe by default:** one-click mode uses conservative Windows settings only.
- **Backup before change:** advanced modules require a verified restore point and registry snapshot.
- **Opt-in risk:** HAGS, high-performance power mode, network adapter changes, MSI, telemetry/service changes and repair operations are never silently applied.
- **No security weakening:** Windows Defender, Windows Update and core security infrastructure are not disabled by the default profile.
- **No game tampering:** no DLL injection, game-memory editing, anti-cheat modification or packet routing.
- **No guaranteed results:** FPS, latency and stability vary by hardware, drivers, game and network conditions.
- **Rollback:** registry backups can be imported from the UI; Windows System Restore can be used for system-level rollback.

## V6 Safe Deployment Order

1. Create a verified Safety Snapshot.
2. Apply the Balanced/Universal gaming profile.
3. Test the game and record FPS/1% lows and latency.
4. Enable temporary timer tuning if input latency is the priority.
5. Apply the safe network baseline and retest.
6. Use storage cleanup/Optimize Drives as maintenance.
7. Run DISM/SFC when Windows integrity is suspect.
8. Add telemetry/OneDrive/Xbox reductions only if those features are not needed.
9. Use MSI and advanced network offload changes one at a time, restart when requested, and benchmark each change.
10. If a change causes instability, use the registry backup and/or Windows System Restore.

## Anti-Cheat Safety Rule

Never optimize by injecting into a game, changing game memory, replacing anti-cheat files, or intercepting game network packets. Keep performance work at the Windows, driver, adapter, and supported game-settings level.
