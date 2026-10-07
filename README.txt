GRIMACE OPTIMIZER V5 - UNIVERSAL GAME OPTIMIZER

This version keeps the Windows gaming optimizations from V4 and adds a truly game-agnostic game selector.

NEW IN V5
- Big dashboard button: OPTIMIZE ANY GAME
- Game selector with common PC games
- Browse any game's .exe for custom games
- Optimize selected game with one click
- Session-only High process priority for the selected game
- Launch selected game when an executable path is known
- Open the selected game's folder
- Universal optimization no longer depends on Fortnite/Epic endpoints
- Fortnite-specific tools are no longer required for the universal optimizer

GAME PROFILES INCLUDED
- Fortnite
- VALORANT
- Counter-Strike 2
- Apex Legends
- Overwatch 2
- Rocket League
- Grand Theft Auto V
- Roblox
- Minecraft Java
- Custom Game / any .exe

WINDOWS OPTIMIZATIONS
- High-performance power plan
- Windows Game Mode
- Background Game DVR capture disabled
- Hardware-accelerated GPU scheduling request
- Windows visual-effects performance setting
- User temp cleanup
- Selected game process priority

IMPORTANT
- Some Windows settings may require administrator rights or a restart.
- HAGS depends on Windows, GPU and driver support.
- Process priority is session-only.
- The optimizer does not edit game configuration files.
- The built-in game list contains process names for common games; use BROWSE .EXE when a game is installed differently or not listed.
- Network tab remains an Epic endpoint test and is not used by the universal optimizer.

BUILD
GitHub Actions workflow: .github/workflows/build-windows.yml

Manual publish command:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
