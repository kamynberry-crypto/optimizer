@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-GrimaceOptimizer.ps1"
if errorlevel 1 (
  echo.
  echo BUILD FAILED. See the error above.
  pause
  exit /b 1
)
echo.
echo Build complete.
start "" "%~dp0bin\Release\net8.0-windows\win-x64\publish"
pause
