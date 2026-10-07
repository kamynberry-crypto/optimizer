GRIMACE OPTIMIZER V4 - BLUE GAMING CENTER

UI updates:
- Hone-inspired dark blue dashboard layout with left navigation
- Animated startup splash/loading sequence
- Large one-click "OPTIMIZE MY PC" button
- Existing optimization profiles and individual controls preserved
- Dashboard hardware summary and optimization status
- Network region testing remains available

One-click optimization applies the existing full performance set and then tests Epic endpoint latency.

Build:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

Important:
Some Windows changes require administrator rights or a restart. HAGS support depends on Windows/GPU/driver. The region test measures endpoint latency; it cannot guarantee Epic matchmaking uses a specific physical server.

GITHUB ACTIONS WINDOWS BUILD
============================

This project includes a GitHub Actions workflow at:
.github/workflows/build-windows.yml

To build without installing .NET on your PC:

1. Create a GitHub repository.
2. Upload all project files, including the .github folder.
3. Open the repository's Actions tab.
4. Select "Build Grimace Optimizer (Windows)".
5. Click "Run workflow".
6. When it finishes, open the completed workflow run.
7. Under Artifacts, download "GrimaceOptimizer-Windows-x64".
8. Extract the ZIP and run GrimaceOptimizer.exe.

The workflow also runs automatically when changes are pushed to main or master.
It uses a Windows GitHub runner, installs the .NET 8 SDK, publishes a self-contained
win-x64 single-file EXE, verifies that the EXE exists, and uploads a ZIP artifact.

No .NET SDK is required on the computer that downloads and runs the published EXE.
