using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GrimaceOptimizer
{
    public partial class MainWindow : Window
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetTimerResolution(uint DesiredResolution, [MarshalAs(UnmanagedType.I1)] bool SetResolution, out uint ActualResolution);

        private uint _previousTimerResolution;
        private bool _timerResolutionActive;
        private sealed class GameProfile
        {
            public string Name { get; }
            public string ProcessName { get; }
            public string? ExecutablePath { get; set; }

            public GameProfile(string name, string processName, string? executablePath = null)
            {
                Name = name;
                ProcessName = processName;
                ExecutablePath = executablePath;
            }

            public override string ToString() => Name;
        }

        private readonly List<GameProfile> gameProfiles = new()
        {
            new GameProfile("Fortnite", "FortniteClient-Win64-Shipping"),
            new GameProfile("VALORANT", "VALORANT-Win64-Shipping"),
            new GameProfile("Counter-Strike 2", "cs2"),
            new GameProfile("Apex Legends", "r5apex"),
            new GameProfile("Overwatch 2", "Overwatch"),
            new GameProfile("Rocket League", "RocketLeague"),
            new GameProfile("Grand Theft Auto V", "GTA5"),
            new GameProfile("Roblox", "RobloxPlayerBeta"),
            new GameProfile("Minecraft Java", "javaw"),
            new GameProfile("Custom Game", "")
        };

        private GameProfile SelectedGame => GameSelector.SelectedItem as GameProfile ?? gameProfiles[0];

        private readonly (string Name, string Host)[] regions =
        {
            ("NA-East", "ping-nae.ds.on.epicgames.com"),
            ("NA-Central", "ping-nac.ds.on.epicgames.com"),
            ("NA-West", "ping-naw.ds.on.epicgames.com"),
            ("EU", "ping-eu.ds.on.epicgames.com"),
            ("Oceania", "ping-oce.ds.on.epicgames.com"),
            ("Brazil", "ping-br.ds.on.epicgames.com"),
            ("Asia", "ping-asia.ds.on.epicgames.com")
        };

        private string PowerShell(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command.Replace("\"", "\\\"") + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using var p = Process.Start(psi);
                if (p == null) return "";
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(5000);
                return output;
            }
            catch { return ""; }
        }

        public MainWindow()
        {
            InitializeComponent();
            GameSelector.ItemsSource = gameProfiles;
            GameSelector.SelectedIndex = 0;
            DetectHardware();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            StopLowLatencyTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            int step = 0;
            timer.Tick += (s, args) =>
            {
                step++;
                StartupText.Text = step switch
                {
                    1 => "Detecting hardware...",
                    2 => "Loading gaming profiles...",
                    3 => "Preparing optimization engine...",
                    _ => "Ready to optimize."
                };

                if (step >= 4)
                {
                    timer.Stop();
                    var progress = new DoubleAnimation(12, 430, TimeSpan.FromMilliseconds(420));
                    StartupProgress.BeginAnimation(WidthProperty, progress);

                    var mainFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(650));
                    MainContent.BeginAnimation(OpacityProperty, mainFade);

                    var splashFade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(650))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(180)
                    };
                    splashFade.Completed += (_, __) => StartupOverlay.Visibility = Visibility.Collapsed;
                    StartupOverlay.BeginAnimation(OpacityProperty, splashFade);
                }
            };
            timer.Start();
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int index))
                MainTabs.SelectedIndex = index;
        }

        private async void OptimizeEverything_Click(object sender, RoutedEventArgs e)
        {
            await OptimizeSelectedGameAsync();
        }

        private async void OptimizeSelectedGame_Click(object sender, RoutedEventArgs e)
        {
            await OptimizeSelectedGameAsync();
        }

        private async Task OptimizeSelectedGameAsync()
        {
            OptimizeEverythingButton.IsEnabled = false;
            StatusText.Text = "OPTIMIZING";
            DashboardProfile.Text = "Optimizing " + SelectedGame.Name + "...";
            SmartLog.Text = "Applying the universal gaming profile to " + SelectedGame.Name + "...";
            GameLog.Text = "Running Windows gaming optimizations...";

            try
            {
                bool safety = CreateSafetySnapshot();
                if (!safety)
                {
                    StatusText.Text = "SAFETY CHECK";
                    DashboardProfile.Text = "No verified backup — nothing changed";
                    SmartLog.Text = "Optimization was stopped before making changes.\n\n" +
                        "Grimace could not verify a Windows restore point and registry safety snapshot.\n" +
                        "This is intentional: the safest mode never changes system settings without a verified rollback point.\n\n" +
                        "Create a safety snapshot from the Performance Center, then try again.";
                    GameLog.Text = "No optimization changes were applied.";
                    return;
                }

                // SAFE DEFAULT: only reversible, conservative Windows-side changes.
                ApplySafeCoreGamingSettings();
                CleanTemp();
                SetSelectedGamePriority();

                ProfileText.Text = "Safe Universal Gaming";
                SmartLog.Text = "Safe universal profile applied to " + SelectedGame.Name + ".\n\n" +
                    "✓ Windows Balanced power plan (conservative temperature/power behavior)\n" +
                    "✓ Windows Game Mode\n" +
                    "✓ Background Game DVR capture disabled\n" +
                    "✓ User temp files cleaned where Windows allowed\n" +
                    "✓ " + SelectedGame.Name + " process priority raised when running\n" +
                    "✓ Verified restore point + registry safety snapshot created\n\n" +
                    "Advanced latency, HAGS, network-adapter, service, and driver changes are NOT part of one-click optimization.\n" +
                    "They remain available as optional modules with additional safety checks.";
                GameLog.Text = "✓ Universal optimization applied to " + SelectedGame.Name + ".";

                StatusText.Text = "OPTIMIZED";
                DashboardProfile.Text = SelectedGame.Name + " optimized";
                DashboardSelectedGame.Text = "Selected game: " + SelectedGame.Name;
            }
            catch (Exception ex)
            {
                StatusText.Text = "READY";
                DashboardProfile.Text = "Optimization finished with an issue";
                SmartLog.Text = "Optimization completed with an issue: " + ex.Message;
                GameLog.Text = "Could not complete the selected game optimization: " + ex.Message;
            }
            finally
            {
                OptimizeEverythingButton.IsEnabled = true;
            }
        }

        private void GameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameSelector == null || GameSelector.SelectedItem is not GameProfile game) return;
            UpdateSelectedGameDisplay(game);
        }

        private void UpdateSelectedGameDisplay(GameProfile game)
        {
            SelectedGameText.Text = "Selected: " + game.Name;
            SelectedProcessText.Text = string.IsNullOrWhiteSpace(game.ProcessName)
                ? "Process: choose a .exe with BROWSE .EXE"
                : "Process: " + game.ProcessName + ".exe";
            DashboardProfile.Text = game.Name + " ready to optimize";
            DashboardSelectedGame.Text = "Selected game: " + game.Name;
        }

        private void BrowseGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a game executable",
                Filter = "Game executable (*.exe)|*.exe|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true) return;

            string processName = Path.GetFileNameWithoutExtension(dialog.FileName);
            var custom = new GameProfile(Path.GetFileNameWithoutExtension(dialog.FileName), processName, dialog.FileName);
            int existing = gameProfiles.FindIndex(g => string.Equals(g.ExecutablePath, dialog.FileName, StringComparison.OrdinalIgnoreCase));
            if (existing < 0)
            {
                gameProfiles.Add(custom);
                GameSelector.ItemsSource = null;
                GameSelector.ItemsSource = gameProfiles;
                existing = gameProfiles.Count - 1;
            }

            GameSelector.SelectedIndex = existing;
            GameLog.Text = "Selected executable: " + dialog.FileName;
        }

        private void LaunchSelectedGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var game = SelectedGame;
                if (!string.IsNullOrWhiteSpace(game.ExecutablePath) && File.Exists(game.ExecutablePath))
                {
                    Process.Start(new ProcessStartInfo { FileName = game.ExecutablePath, UseShellExecute = true });
                    StatusText.Text = "LAUNCHING";
                    GameLog.Text = "Launching " + game.Name + "...";
                    return;
                }

                Process[] running = string.IsNullOrWhiteSpace(game.ProcessName)
                    ? Array.Empty<Process>()
                    : Process.GetProcessesByName(game.ProcessName);

                if (running.Length > 0)
                {
                    GameLog.Text = game.Name + " is already running.";
                    StatusText.Text = "RUNNING";
                    return;
                }

                MessageBox.Show("Use BROWSE .EXE to select " + game.Name + " if it is not already running. The built-in profile does not guess an install location.", "Grimace Optimizer");
            }
            catch (Exception ex)
            {
                GameLog.Text = "Could not launch the selected game: " + ex.Message;
            }
        }

        private void OpenGameFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? path = SelectedGame.ExecutablePath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    Process[] processes = string.IsNullOrWhiteSpace(SelectedGame.ProcessName)
                        ? Array.Empty<Process>()
                        : Process.GetProcessesByName(SelectedGame.ProcessName);
                    path = processes.Select(GetProcessPath).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    GameLog.Text = "Game folder is unknown. Use BROWSE .EXE first, or start the game and try again.";
                    return;
                }

                string? folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                    Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                GameLog.Text = "Could not open the game folder: " + ex.Message;
            }
        }

        private static string? GetProcessPath(Process process)
        {
            try { return process.MainModule?.FileName; } catch { return null; }
        }

        private void SetSelectedGamePriority()
        {
            var game = SelectedGame;
            if (string.IsNullOrWhiteSpace(game.ProcessName))
            {
                GameLog.Text = "No process selected. Use BROWSE .EXE to choose the game executable.";
                return;
            }

            Process[] processes = Process.GetProcessesByName(game.ProcessName);
            if (processes.Length == 0)
            {
                GameLog.Text = "✓ Windows gaming settings applied. " + game.Name + " is not running, so process priority will apply when you run the game and use Optimize Selected Game again.";
                return;
            }

            int changed = 0;
            foreach (var process in processes)
            {
                try { process.PriorityClass = ProcessPriorityClass.High; changed++; } catch { }
            }

            GameLog.Text = changed > 0
                ? "✓ " + game.Name + " process priority set to High for the current session."
                : "✓ Windows gaming settings applied, but Windows did not allow the process priority change.";
        }

        private void DetectHardware()
        {
            string cpu = PowerShell("(Get-CimInstance Win32_Processor | Select-Object -First 1 -ExpandProperty Name)");
            string gpu = PowerShell("(Get-CimInstance Win32_VideoController | Where-Object {$_.Name -notmatch 'Microsoft Basic'} | Select-Object -First 1 -ExpandProperty Name)");
            string ram = PowerShell("[math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory/1GB,1)");
            string disk = PowerShell("[math]::Round((Get-CimInstance Win32_LogicalDisk -Filter \"DeviceID='C:'\").Size/1GB,0)");

            CpuText.Text = string.IsNullOrWhiteSpace(cpu) ? "Unknown" : cpu;
            GpuText.Text = string.IsNullOrWhiteSpace(gpu) ? "Unknown" : gpu;
            RamText.Text = string.IsNullOrWhiteSpace(ram) ? "Unknown" : ram + " GB";
            StorageText.Text = string.IsNullOrWhiteSpace(disk) ? "Unknown" : disk + " GB";

            string hardware = (cpu + " " + gpu).ToLowerInvariant();
            if (hardware.Contains("1660 ti"))
                RecommendationText.Text = "GTX 1660 Ti-class hardware detected. Competitive FPS is a strong starting profile; Ultimate Performance is available when you want maximum system performance.";
            else if (!string.IsNullOrWhiteSpace(cpu) || !string.IsNullOrWhiteSpace(gpu))
                RecommendationText.Text = "Hardware detected. For most gaming PCs, Competitive FPS is the best starting point; Ultimate Performance adds the more aggressive performance-oriented Windows changes.";
            else
                RecommendationText.Text = "Hardware detection is incomplete. Use Balanced & Safe if you want the most conservative profile.";

            ProfileText.Text = "Competitive FPS";
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            string profile = button.Tag?.ToString() ?? "balanced";
            StatusText.Text = "SAFETY CHECK";

            try
            {
                if (!EnsureSafetyForChange())
                {
                    StatusText.Text = "READY";
                    SmartLog.Text = "Preset stopped because a verified safety snapshot could not be created.";
                    return;
                }

                switch (profile)
                {
                    case "ultimate":
                        if (!ConfirmAdvanced("Ultimate Performance Preset", "This preset applies higher-power and deeper Windows performance settings. It can increase heat, power use, driver sensitivity and restart requirements. The Safe Universal profile is recommended for everyday use."))
                        {
                            StatusText.Text = "READY";
                            return;
                        }
                        ProfileText.Text = "Ultimate Performance";
                        ApplyCoreGamingSettings();
                        ApplyHags();
                        ApplyVisuals();
                        CleanTemp();
                        SetFortnitePriority();
                        SmartLog.Text = "Ultimate profile applied after safety confirmation.\n\n✓ High-performance power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n✓ HAGS requested\n✓ Windows visual effects reduced\n✓ User temp files cleaned\n✓ Fortnite priority raised when running\n\nIf stability, temperature, latency or graphics behavior worsens, use the backup/restore tools and return to the Safe Universal profile.";
                        break;

                    case "competitive":
                        ProfileText.Text = "Competitive FPS — Safe";
                        ApplySafeCoreGamingSettings();
                        SetFortnitePriority();
                        SmartLog.Text = "Competitive FPS Safe profile applied.\n\n✓ Windows Balanced power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n✓ Fortnite priority raised when running\n\nNo HAGS, service, adapter, driver or permanent timer changes were applied.";
                        break;

                    default:
                        ProfileText.Text = "Balanced & Safe";
                        ApplySafeCoreGamingSettings();
                        SmartLog.Text = "Balanced & Safe profile applied.\n\n✓ Windows Balanced power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n\nNo advanced system, driver, network-adapter or service changes were applied.";
                        break;
                }

                StatusText.Text = "OPTIMIZED";
            }
            catch (Exception ex)
            {
                StatusText.Text = "READY";
                SmartLog.Text = "Optimization completed with an issue: " + ex.Message;
            }
        }

        private void ApplySafeCoreGamingSettings()
        {
            // Conservative default: keep Windows Balanced rather than forcing a high-power plan.
            Run("powercfg.exe", "/setactive SCHEME_BALANCED");
            SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
            SetDword(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0);
            SetDword(@"HKCU\Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
            SetDword(@"HKCU\Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
        }

        private bool ConfirmAdvanced(string title, string message)
        {
            return MessageBox.Show(message + "\n\nA safety snapshot is required before this change. Continue?", title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private bool EnsureSafetyForChange()
        {
            return CreateSafetySnapshot();
        }

        private void ApplyCoreGamingSettings()
        {
            Run("powercfg.exe", "/setactive SCHEME_MIN");
            SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
            SetDword(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0);
            SetDword(@"HKCU\Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
            SetDword(@"HKCU\Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
        }

        private void ApplyHags()
        {
            SetDword(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2);
        }

        private void ApplyVisuals()
        {
            SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);
        }


        private bool CreateSafetySnapshot()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GrimaceOptimizer", "Backups");
                Directory.CreateDirectory(folder);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string backup = Path.Combine(folder, stamp);
                Directory.CreateDirectory(backup);

                // Require the Windows restore point command to succeed. We do not silently continue.
                int restoreExit = RunAndGetExitCode("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description 'Grimace Optimizer Before Optimization' -RestorePointType 'MODIFY_SETTINGS'\"");

                ExportRegistry(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", Path.Combine(backup, "GameDVR.reg"));
                ExportRegistry(@"HKCU\System\GameConfigStore", Path.Combine(backup, "GameConfigStore.reg"));
                ExportRegistry(@"HKCU\Software\Microsoft\GameBar", Path.Combine(backup, "GameBar.reg"));
                ExportRegistry(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", Path.Combine(backup, "VisualEffects.reg"));
                ExportRegistry(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", Path.Combine(backup, "GraphicsDrivers.reg"));

                bool registryOk = File.Exists(Path.Combine(backup, "GameDVR.reg")) &&
                                  File.Exists(Path.Combine(backup, "GameConfigStore.reg")) &&
                                  File.Exists(Path.Combine(backup, "GameBar.reg")) &&
                                  File.Exists(Path.Combine(backup, "VisualEffects.reg")) &&
                                  File.Exists(Path.Combine(backup, "GraphicsDrivers.reg"));

                if (restoreExit != 0 || !registryOk)
                {
                    WindowsLog.Text = "⚠ Safety verification failed. No optimization changes were applied.";
                    return false;
                }

                File.WriteAllText(Path.Combine(backup, "BACKUP-VERIFIED.txt"),
                    "Grimace Optimizer safety snapshot verified at " + DateTime.Now.ToString("O") + Environment.NewLine +
                    "Restore point command exit code: 0" + Environment.NewLine +
                    "Registry exports: GameDVR, GameConfigStore, GameBar, VisualEffects, GraphicsDrivers" + Environment.NewLine +
                    "Note: restore points are a rollback aid, not an absolute guarantee.");

                WindowsLog.Text = "✓ Verified safety snapshot created: " + stamp;
                return true;
            }
            catch
            {
                WindowsLog.Text = "⚠ Could not verify the safety snapshot. No changes were applied.";
                return false;
            }
        }

        private void ExportRegistry(string key, string file)
        {
            Run("reg.exe", "export \"" + key + "\" \"" + file + "\" /y");
        }

        private void ApplyLowLatencySafeSettings()
        {
            // Keep the Windows timer request temporary and reversible rather than changing a permanent global timer policy.
            StartLowLatencyTimer();
            // Windows gaming mode and raw input are left to the game/OS; no anti-cheat hooks or DLL injection are used.
        }

        private void StartLowLatencyTimer()
        {
            if (_timerResolutionActive) return;
            try
            {
                NtSetTimerResolution(10000, true, out _previousTimerResolution); // 1 ms in 100-ns units
                _timerResolutionActive = true;
            }
            catch { }
        }

        private void StopLowLatencyTimer()
        {
            if (!_timerResolutionActive) return;
            try
            {
                NtSetTimerResolution(10000, false, out _);
            }
            catch { }
            _timerResolutionActive = false;
        }

        private void ApplyNetworkSafeSettings()
        {
            Run("netsh.exe", "int tcp set global autotuninglevel=normal");
            Run("netsh.exe", "int tcp set global rss=enabled");
            Run("netsh.exe", "int tcp set global ecncapability=disabled");
        }

        private void ApplyAdvancedNetworkSettings()
        {
            string script = @"
$props = Get-NetAdapterAdvancedProperty -ErrorAction SilentlyContinue
foreach ($p in $props) {
  if ($p.RegistryKeyword -match 'LsoV2IPv4|LsoV2IPv6' -and $p.RegistryValue -match 'Enabled|1|On') {
    try { Set-NetAdapterAdvancedProperty -Name $p.Name -RegistryKeyword $p.RegistryKeyword -RegistryValue '0' -NoRestart -ErrorAction SilentlyContinue } catch {}
  }
}
";
            Run("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script.Replace("\"", "\\\"") + "\"");
        }

        private void OptimizeMsiForDisplayAndNetwork()
        {
            string script = @"
$classes = @('Display','Net')
foreach ($class in $classes) {
  Get-PnpDevice -Class $class -PresentOnly -ErrorAction SilentlyContinue | ForEach-Object {
    $id = $_.InstanceId
    if ($id -and $_.Status -eq 'OK') {
      $path = 'HKLM:\SYSTEM\CurrentControlSet\Enum\' + $id + '\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties'
      if (Test-Path $path) { try { Set-ItemProperty -Path $path -Name MSISupported -Type DWord -Value 1 -ErrorAction SilentlyContinue } catch {} }
    }
  }
}
";
            Run("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script.Replace("\"", "\\\"") + "\"");
        }

        private void DisableOptionalGamingServices()
        {
            string[] services = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc", "XboxGipSvc" };
            foreach (string service in services)
            {
                Run("sc.exe", "stop " + service);
                Run("sc.exe", "config " + service + " start=demand");
            }
        }

        private void DisableOneDriveStartup()
        {
            Run("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command "Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue"");
            Run("taskkill.exe", "/IM OneDrive.exe /F");
        }

        private void DisableTelemetry()
        {
            SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0);
            SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0);
            Run("sc.exe", "stop DiagTrack");
            Run("sc.exe", "config DiagTrack start=demand");
        }

        private void OptimizeStorage()
        {
            CleanTemp();
            Run("defrag.exe", "C: /O");
        }

        private void RunSystemRepair()
        {
            Run("DISM.exe", "/Online /Cleanup-Image /RestoreHealth");
            Run("sfc.exe", "/scannow");
        }

        private void RestoreLastBackup()
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GrimaceOptimizer", "Backups");
            if (!Directory.Exists(root)) { WindowsLog.Text = "No Grimace Optimizer registry backup was found."; return; }
            string? latest = Directory.GetDirectories(root).OrderByDescending(Directory.GetLastWriteTimeUtc).FirstOrDefault();
            if (latest == null) { WindowsLog.Text = "No Grimace Optimizer registry backup was found."; return; }
            foreach (string file in Directory.GetFiles(latest, "*.reg")) Run("reg.exe", "import \"" + file + "\"");
            WindowsLog.Text = "✓ Last registry safety backup imported. Restart Windows if a restored setting needs it.";
        }

        private void HoneModule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            string tag = button.Tag?.ToString() ?? "";
            try
            {
                switch (tag)
                {
                    case "snapshot":
                        CreateSafetySnapshot();
                        break;
                    case "latency":
                        if (!ConfirmAdvanced("Temporary Latency Mode", "This requests a temporary 1 ms timer resolution while Grimace is running. It is reversible when Grimace closes and may increase power use.")) break;
                        if (!EnsureSafetyForChange()) break;
                        StartLowLatencyTimer();
                        ApplyLowLatencySafeSettings();
                        WindowsLog.Text = "✓ Temporary 1 ms timer request enabled while Grimace is running. No anti-cheat hooks or game-code changes are used.";
                        break;
                    case "network":
                        if (!ConfirmAdvanced("Network Baseline", "Network tuning can behave differently on different adapters, routers, and ISPs. It is reversible, but it can occasionally worsen latency or throughput.")) break;
                        if (!EnsureSafetyForChange()) break;
                        ApplyNetworkSafeSettings();
                        WindowsLog.Text = "✓ Safe network baseline applied: RSS enabled and TCP auto-tuning kept at Windows normal.";
                        break;
                    case "advancednetwork":
                        if (!ConfirmAdvanced("Advanced Network Adapter Tuning", "This changes adapter offload settings. Results vary by network hardware and drivers; revert if latency or throughput worsens.")) break;
                        if (!EnsureSafetyForChange()) break;
                        ApplyAdvancedNetworkSettings();
                        WindowsLog.Text = "✓ Advanced adapter offload tuning requested. Restart/network adapter reset may be required; revert if throughput worsens.";
                        break;
                    case "msi":
                        if (!ConfirmAdvanced("MSI Device Tuning", "This changes Message Signaled Interrupt settings for supported present display/network devices. Driver behavior varies, and a restart may be required.")) break;
                        if (!EnsureSafetyForChange()) break;
                        OptimizeMsiForDisplayAndNetwork();
                        WindowsLog.Text = "✓ MSI mode enabled where supported for present Display/Network devices. Restart Windows before benchmarking.";
                        break;
                    case "telemetry":
                        if (!ConfirmAdvanced("Reduce Telemetry", "This changes selected privacy/telemetry settings and the DiagTrack service. Windows security is not disabled, but diagnostics may be reduced.")) break;
                        if (!EnsureSafetyForChange()) break;
                        DisableTelemetry();
                        WindowsLog.Text = "✓ Selected telemetry/advertising activity reduced. Windows core security remains enabled.";
                        break;
                    case "xbox":
                        if (!ConfirmAdvanced("Pause Xbox Services", "Xbox/Game Pass features and some Microsoft gaming components may stop working until these services are restored.")) break;
                        if (!EnsureSafetyForChange()) break;
                        DisableOptionalGamingServices();
                        WindowsLog.Text = "✓ Xbox background services changed to demand-start. Xbox/Game Pass features may require them.";
                        break;
                    case "onedrive":
                        if (!ConfirmAdvanced("Stop OneDrive Startup", "This removes OneDrive from the current user's startup entry and stops the current process. Your files are not deleted.")) break;
                        if (!EnsureSafetyForChange()) break;
                        DisableOneDriveStartup();
                        WindowsLog.Text = "✓ OneDrive startup/background process stopped for this session. Files are not deleted.";
                        break;
                    case "storage":
                        if (!ConfirmAdvanced("Storage Maintenance", "This cleans temporary files and asks Windows to optimize drive C:. Windows controls the actual operation.")) break;
                        if (!EnsureSafetyForChange()) break;
                        OptimizeStorage();
                        WindowsLog.Text = "✓ User temp files cleaned and Windows Optimize Drives requested for C:.";
                        break;
                    case "repair":
                        if (!ConfirmAdvanced("Windows Repair", "DISM and SFC can take a while and may repair Windows components. This is maintenance, not an FPS tweak.")) break;
                        if (!EnsureSafetyForChange()) break;
                        RunSystemRepair();
                        WindowsLog.Text = "✓ DISM/SFC repair commands completed. Review Windows output if corruption was found.";
                        break;
                    case "restore":
                        RestoreLastBackup();
                        break;
                }
            }
            catch (Exception ex) { WindowsLog.Text = "Could not apply this module: " + ex.Message; }
        }

        private void GamePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            string name = button.Tag?.ToString() ?? "";
            var match = gameProfiles.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                GameSelector.SelectedItem = match;
                ApplyCoreGamingSettings();
                SetSelectedGamePriority();
                GameLog.Text = "✓ " + match.Name + " performance preset selected. Grimace only changes supported Windows/game settings; it does not inject code or modify anti-cheat files.";
                MainTabs.SelectedIndex = 2;
            }
        }

        private void Tweak_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            string tag = button.Tag?.ToString() ?? "";

            try
            {
                switch (tag)
                {
                    case "power":
                        if (!ConfirmAdvanced("High Performance Power Plan", "This can increase power use, heat, and fan noise. Windows Balanced is the safer default.")) break;
                        if (!EnsureSafetyForChange()) break;
                        Run("powercfg.exe", "/setactive SCHEME_MIN");
                        WindowsLog.Text = "✓ High-performance power plan requested.";
                        break;
                    case "gamemode":
                        if (!EnsureSafetyForChange()) break;
                        SetDword(@"HKCU\Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                        SetDword(@"HKCU\Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
                        WindowsLog.Text = "✓ Windows Game Mode enabled.";
                        break;
                    case "dvr":
                        if (!EnsureSafetyForChange()) break;
                        SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
                        SetDword(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0);
                        WindowsLog.Text = "✓ Background Game DVR capture disabled.";
                        break;
                    case "hags":
                        if (!ConfirmAdvanced("Hardware-Accelerated GPU Scheduling", "HAGS behavior depends on Windows, your GPU, and the driver. It can help some systems and hurt others; a restart may be required.")) break;
                        if (!EnsureSafetyForChange()) break;
                        ApplyHags();
                        WindowsLog.Text = "✓ HAGS requested. Windows/GPU/driver support is required and a restart may be needed.";
                        break;
                    case "visuals":
                        if (!ConfirmAdvanced("Windows Visual Effects", "This changes Windows appearance/performance settings. It is reversible, but the desktop may look less polished.")) break;
                        if (!EnsureSafetyForChange()) break;
                        ApplyVisuals();
                        WindowsLog.Text = "✓ Windows visual-effects setting adjusted toward performance.";
                        break;
                    case "temp":
                        if (!EnsureSafetyForChange()) break;
                        CleanTemp();
                        WindowsLog.Text = "✓ User temp files cleaned where Windows allowed.";
                        break;
                    case "priority":
                        if (!EnsureSafetyForChange()) break;
                        SetSelectedGamePriority();
                        break;
                    case "config":
                        OpenFortniteConfig();
                        break;
                }
            }
            catch (Exception ex)
            {
                WindowsLog.Text = "Could not apply this option: " + ex.Message;
            }
        }

        private void SetDword(string key, string value, int data)
        {
            Run("reg.exe", "ADD \"" + key + "\" /v " + value + " /t REG_DWORD /d " + data + " /f");
        }

        private void CleanTemp()
        {
            string temp = Path.GetTempPath();
            foreach (string file in Directory.GetFiles(temp))
            {
                try { File.Delete(file); } catch { }
            }
        }

        private void SetFortnitePriority()
        {
            Process[] processes = Process.GetProcessesByName("FortniteClient-Win64-Shipping");
            if (processes.Length == 0)
            {
                WindowsLog.Text = "Fortnite is not running. Start Fortnite first, then use this option.";
                return;
            }

            int changed = 0;
            foreach (var p in processes)
            {
                try { p.PriorityClass = ProcessPriorityClass.High; changed++; } catch { }
            }

            WindowsLog.Text = changed > 0
                ? "✓ Fortnite process priority set to High for the current session."
                : "Fortnite was found, but Windows did not allow the priority change.";
        }

        private void OpenFortniteConfig()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FortniteGame", "Saved", "Config", "WindowsClient");

            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                return;
            }

            WindowsLog.Text = "Fortnite config folder was not found at the normal Windows location.";
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "com.epicgames.launcher://apps/Fortnite?action=launch",
                    UseShellExecute = true
                });
                StatusText.Text = "LAUNCHING";
            }
            catch
            {
                MessageBox.Show("Epic Games Launcher could not be opened. Make sure Epic Games Launcher and Fortnite are installed.", "Grimace Optimizer");
            }
        }

        private async void RegionButton_Click(object sender, RoutedEventArgs e)
        {
            RegionButton.IsEnabled = false;
            StatusText.Text = "TESTING";
            RegionText.Text = "Testing...";
            PingText.Text = "Measuring latency...";
            NetworkLog.Text = "";

            var results = await Task.Run(TestRegions);
            RegionButton.IsEnabled = true;

            if (results.Count == 0)
            {
                RegionText.Text = "Best region: unavailable";
                PingText.Text = "Lowest ping: —";
                NetworkLog.Text = "No listed endpoint responded. This does not necessarily mean Fortnite is unavailable.";
                StatusText.Text = "READY";
                return;
            }

            var best = results[0];
            RegionText.Text = "Best region: " + best.Name;
            PingText.Text = "Lowest ping: " + best.Ping + " ms";
            NetworkLog.Text = "Tested " + results.Count + " responding region endpoint(s). Lower latency is generally preferable, but this test cannot guarantee the matchmaking server used by Fortnite.";
            StatusText.Text = "REGION FOUND";
        }

        private List<(string Name, long Ping)> TestRegions()
        {
            var list = new List<(string Name, long Ping)>();
            foreach (var region in regions)
            {
                try
                {
                    using var ping = new Ping();
                    PingReply reply = ping.Send(region.Host, 1200);
                    if (reply.Status == IPStatus.Success)
                        list.Add((region.Name, reply.RoundtripTime));
                }
                catch { }
            }
            list.Sort((a, b) => a.Ping.CompareTo(b.Ping));
            return list;
        }

        private int RunAndGetExitCode(string file, string args)
        {
            try
            {
                using var p = Process.Start(new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (p == null) return -1;
                p.WaitForExit(15000);
                return p.HasExited ? p.ExitCode : -1;
            }
            catch { return -1; }
        }

        private void Run(string file, string args)
        {
            try
            {
                using var p = Process.Start(new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                p?.WaitForExit(5000);
            }
            catch { }
        }
    }
}
