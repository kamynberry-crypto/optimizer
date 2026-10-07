using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            OptimizeEverythingButton.IsEnabled = false;
            StatusText.Text = "OPTIMIZING";
            DashboardProfile.Text = "Applying gaming optimizations...";
            SmartLog.Text = "Running the full recommended optimization set...";

            try
            {
                // Full one-click action uses the existing Ultimate profile, preserving
                // all existing optimizer functionality in a single button.
                ApplyCoreGamingSettings();
                ApplyHags();
                ApplyVisuals();
                CleanTemp();
                SetFortnitePriority();

                ProfileText.Text = "Ultimate Performance";
                DashboardProfile.Text = "Optimization complete";
                SmartLog.Text = "Everything available in the full profile was applied.\n\n" +
                    "✓ High-performance power plan\n" +
                    "✓ Windows Game Mode\n" +
                    "✓ Background Game DVR capture disabled\n" +
                    "✓ HAGS requested\n" +
                    "✓ Windows visual effects reduced\n" +
                    "✓ User temp files cleaned\n" +
                    "✓ Fortnite priority raised when running\n\n" +
                    "Testing network regions now...";

                StatusText.Text = "TESTING NETWORK";
                var results = await Task.Run(TestRegions);
                if (results.Count > 0)
                {
                    var best = results[0];
                    RegionText.Text = "Best region: " + best.Name;
                    PingText.Text = "Lowest ping: " + best.Ping + " ms";
                    NetworkLog.Text = "Tested " + results.Count + " responding region endpoint(s).";
                }
                else
                {
                    RegionText.Text = "Best region: unavailable";
                    PingText.Text = "Lowest ping: —";
                    NetworkLog.Text = "No listed endpoint responded.";
                }

                StatusText.Text = "OPTIMIZED";
                DashboardProfile.Text = "PC optimized";
            }
            catch (Exception ex)
            {
                StatusText.Text = "READY";
                DashboardProfile.Text = "Optimization finished with an issue";
                SmartLog.Text = "Optimization completed with an issue: " + ex.Message;
            }
            finally
            {
                OptimizeEverythingButton.IsEnabled = true;
            }
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
                        SetFortnitePriority();
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
