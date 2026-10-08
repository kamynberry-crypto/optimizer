$ErrorActionPreference = "Stop"

Write-Host "GRIMACE OPTIMIZER V4 BUILD" -ForegroundColor Cyan
Write-Host "Checking .NET SDK..."

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK was not found." -ForegroundColor Red
    Write-Host "Install the .NET 8 SDK, reopen PowerShell, and run this script again."
    Write-Host "Then the EXE will be: bin\Release\net8.0-windows\win-x64\publish\GrimaceTweaks.exe"
    exit 1
}

dotnet --version

dotnet restore
if ($LASTEXITCODE -ne 0) { throw "Restore failed." }

dotnet publish .\GrimaceTweaks.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }

$exe = Join-Path $PWD "bin\Release\net8.0-windows\win-x64\publish\GrimaceTweaks.exe"
if (-not (Test-Path $exe)) { throw "Build reported success but the EXE was not found at $exe" }

Write-Host ""
Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host $exe -ForegroundColor Green
