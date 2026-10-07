using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GrimaceOptimizer
{
    public partial class MainWindow : Window
    {
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
                ApplyCoreGamingSettings();
                ApplyHags();
                ApplyVisuals();
                CleanTemp();
                SetSelectedGamePriority();

                ProfileText.Text = "Universal Gaming";
                SmartLog.Text = "Universal gaming profile applied to " + SelectedGame.Name + ".\n\n" +
                    "✓ High-performance power plan\n" +
                    "✓ Windows Game Mode\n" +
                    "✓ Background Game DVR capture disabled\n" +
                    "✓ HAGS requested\n" +
                    "✓ Windows visual effects reduced\n" +
                    "✓ User temp files cleaned\n" +
                    "✓ " + SelectedGame.Name + " process priority raised when running\n\n" +
                    "This profile is game-agnostic and does not change game configuration files.";
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
            StatusText.Text = "OPTIMIZING";

            try
            {
                switch (profile)
                {
                    case "ultimate":
                        ProfileText.Text = "Ultimate Performance";
                        ApplyCoreGamingSettings();
                        ApplyHags();
                        ApplyVisuals();
                        CleanTemp();
                        SetFortnitePriority();
                        SmartLog.Text = "Ultimate profile applied.\n\n✓ High-performance power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n✓ HAGS requested\n✓ Windows visual effects reduced\n✓ User temp files cleaned\n✓ Fortnite priority raised when running\n\nSome settings may require administrator rights, Fortnite running, or a Windows restart.";
                        break;

                    case "competitive":
                        ProfileText.Text = "Competitive FPS";
                        ApplyCoreGamingSettings();
                        SetFortnitePriority();
                        SmartLog.Text = "Competitive FPS profile applied.\n\n✓ High-performance power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n✓ Fortnite priority raised when running\n\nThis is the recommended everyday gaming profile.";
                        break;

                    default:
                        ProfileText.Text = "Balanced & Safe";
                        ApplyCoreGamingSettings();
                        SmartLog.Text = "Balanced & Safe profile applied.\n\n✓ High-performance power plan\n✓ Windows Game Mode\n✓ Background Game DVR capture disabled\n\nNo HAGS, visual-effect, or cleanup changes were applied.";
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

        private void Tweak_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            string tag = button.Tag?.ToString() ?? "";

            try
            {
                switch (tag)
                {
                    case "power":
                        Run("powercfg.exe", "/setactive SCHEME_MIN");
                        WindowsLog.Text = "✓ High-performance power plan requested.";
                        break;
                    case "gamemode":
                        SetDword(@"HKCU\Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                        SetDword(@"HKCU\Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
                        WindowsLog.Text = "✓ Windows Game Mode enabled.";
                        break;
                    case "dvr":
                        SetDword(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
                        SetDword(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0);
                        WindowsLog.Text = "✓ Background Game DVR capture disabled.";
                        break;
                    case "hags":
                        ApplyHags();
                        WindowsLog.Text = "✓ HAGS requested. Windows/GPU/driver support is required and a restart may be needed.";
                        break;
                    case "visuals":
                        ApplyVisuals();
                        WindowsLog.Text = "✓ Windows visual-effects setting adjusted toward performance.";
                        break;
                    case "temp":
                        CleanTemp();
                        WindowsLog.Text = "✓ User temp files cleaned where Windows allowed.";
                        break;
                    case "priority":
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
