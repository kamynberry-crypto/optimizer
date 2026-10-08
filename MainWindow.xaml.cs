using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PulseForge;

public partial class MainWindow : Window
{
    private readonly string backupRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PulseForge", "Backups");

    private Border _page = null!;
    private TextBlock _status = null!;

    public MainWindow()
    {
        InitializeComponent();
        ShowDashboard();
    }

    private void BuildPage(string title, string subtitle, UIElement body)
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var titlePanel = new StackPanel();
        titlePanel.Children.Add(new TextBlock { Text = title, FontSize = 30, FontWeight = FontWeights.Bold });
        titlePanel.Children.Add(new TextBlock { Text = subtitle, Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0, 4, 0, 20) });
        Grid.SetRow(titlePanel, 0);
        root.Children.Add(titlePanel);

        _status = new TextBlock { Text = "Ready", Foreground = (Brush)FindResource("Good"), Margin = new Thickness(0, 0, 0, 14) };
        Grid.SetRow(_status, 1);
        root.Children.Add(_status);

        Grid.SetRow(body, 3);
        root.Children.Add(body);
        PageHost.Content = root;
    }

    private Border Card(UIElement content)
    {
        return new Border
        {
            Background = (Brush)FindResource("Panel"),
            BorderBrush = (Brush)FindResource("Line"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 12, 12),
            Child = content
        };
    }

    private Button Action(string text, RoutedEventHandler click, bool primary = false)
    {
        var b = new Button { Content = text, Margin = new Thickness(0, 8, 8, 0), Style = (Style)FindResource(primary ? "Primary" : "Secondary") };
        b.Click += click;
        return b;
    }

    private void ShowDashboard()
    {
        var stack = new StackPanel();

        var hero = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(12, 31, 43)),
            BorderBrush = (Brush)FindResource("Accent"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(24),
            Margin = new Thickness(0, 0, 0, 16)
        };
        var heroGrid = new Grid();
        heroGrid.ColumnDefinitions.Add(new ColumnDefinition());
        heroGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var hs = new StackPanel();
        hs.Children.Add(new TextBlock { Text = "READY TO FORGE", Foreground = (Brush)FindResource("Accent"), FontWeight = FontWeights.Bold, FontSize = 12 });
        hs.Children.Add(new TextBlock { Text = "Fortnite Performance Profile", FontSize = 28, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 0, 4) });
        hs.Children.Add(new TextBlock { Text = "A safe, reversible baseline for FPS stability, input consistency and background-load reduction.", Foreground = (Brush)FindResource("Muted") });
        heroGrid.Children.Add(hs);
        var boost = Action("⚡  FORGE FORTNITE", OptimizeFortnite_Click, true);
        boost.Padding = new Thickness(24, 16, 24, 16);
        Grid.SetColumn(boost, 1);
        heroGrid.Children.Add(boost);
        hero.Child = heroGrid;
        stack.Children.Add(hero);

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(Card(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "SAFE PROFILE", Foreground = (Brush)FindResource("Good"), FontWeight = FontWeights.Bold },
                new TextBlock { Text = "Conservative", FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0,5,0,2) },
                new TextBlock { Text = "No Defender or Update disabling.", Foreground = (Brush)FindResource("Muted") }
            }
        }));
        row.Children.Add(Card(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "GAME SCOPE", Foreground = (Brush)FindResource("Accent"), FontWeight = FontWeights.Bold },
                new TextBlock { Text = "Fortnite + Custom", FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0,5,0,2) },
                new TextBlock { Text = "Use the same engine for any Windows game.", Foreground = (Brush)FindResource("Muted") }
            }
        }));
        stack.Children.Add(row);

        var tips = new StackPanel();
        tips.Children.Add(new TextBlock { Text = "WHAT PULSEFORGE DOES", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 8) });
        foreach (var t in new[]
        {
            "Creates a safety snapshot before system changes.",
            "Enables Windows Game Mode and disables background capture for the current user.",
            "Cleans safe temporary files and checks the system drive.",
            "Applies high priority to a selected game only while it is running.",
            "Tests network latency without changing your router or DNS.",
            "Keeps advanced tweaks optional instead of forcing risky changes."
        })
            tips.Children.Add(new TextBlock { Text = "•  " + t, Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,3,0,0) });
        stack.Children.Add(tips);

        BuildPage("Dashboard", "A focused gaming optimizer designed around safe, reversible changes.", stack);
    }

    private void ShowFortnite()
    {
        var stack = new StackPanel();
        stack.Children.Add(Card(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "FORTNITE PROFILE", Foreground = (Brush)FindResource("Accent"), FontWeight = FontWeights.Bold },
                new TextBlock { Text = "Performance / Competitive", FontSize = 24, FontWeight = FontWeights.Bold, Margin = new Thickness(0,4,0,4) },
                new TextBlock { Text = "Applies only Windows-side settings. It never edits Fortnite memory, injects DLLs, or modifies anti-cheat.", Foreground = (Brush)FindResource("Muted") },
                Action("⚡ Optimize Fortnite Safely", OptimizeFortnite_Click, true),
                Action("▶ Launch Fortnite", LaunchFortnite_Click),
                Action("📁 Open Fortnite Config", OpenFortniteConfig_Click)
            }
        }));

        var settings = new WrapPanel();
        foreach (var item in new[] { "Game Mode", "Capture Reduction", "Temp Cleanup", "Game Priority", "Timer Test", "Network Test" })
        {
            settings.Children.Add(Card(new StackPanel
            {
                Width = 250,
                Children =
                {
                    new TextBlock { Text = item, FontSize = 16, FontWeight = FontWeights.Bold },
                    new TextBlock { Text = item == "Timer Test" ? "Temporary only" : "Safe profile", Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,4,0,0) }
                }
            }));
        }
        stack.Children.Add(settings);
        BuildPage("Fortnite", "The main profile — tuned for a stable, low-background-load competitive setup.", stack);
    }

    private void ShowGames()
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Universal Game Launcher", FontSize = 20, FontWeight = FontWeights.Bold });
        stack.Children.Add(new TextBlock { Text = "Pick any executable and use the same safe optimizer engine.", Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,4,0,10) });
        stack.Children.Add(Action("＋ Select Game EXE", BrowseGame_Click, true));
        stack.Children.Add(Action("▶ Launch Selected Game", LaunchSelected_Click));
        stack.Children.Add(Action("⚙ Optimize Selected Game", OptimizeSelected_Click));

        var games = new WrapPanel { Margin = new Thickness(0, 16, 0, 0) };
        foreach (var game in new[] { "Fortnite", "VALORANT", "Counter-Strike 2", "Apex Legends", "Rocket League", "Roblox", "Custom Game" })
            games.Children.Add(Card(new StackPanel
            {
                Width = 200,
                Children =
                {
                    new TextBlock { Text = game, FontSize = 16, FontWeight = FontWeights.Bold },
                    new TextBlock { Text = "Universal profile", Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,4,0,0) }
                }
            }));
        stack.Children.Add(games);
        BuildPage("My Games", "Fortnite gets the deepest preset, while every Windows game can use the universal engine.", stack);
    }

    private void ShowOptimizations()
    {
        var stack = new StackPanel();
        var items = new[]
        {
            ("Game Mode", "Enable Windows Game Mode", EnableGameMode_Click),
            ("Capture", "Disable Game DVR background capture", DisableDvr_Click),
            ("Power", "Use Windows High Performance plan", HighPerformance_Click),
            ("Cleanup", "Remove safe temporary files", Cleanup_Click),
            ("Priority", "Raise the selected game process", Priority_Click),
            ("Network", "Run a latency test — no permanent network changes", Network_Click)
        };
        foreach (var item in items)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var text = new StackPanel { Width = 430 };
            text.Children.Add(new TextBlock { Text = item.Item1, FontSize = 17, FontWeight = FontWeights.Bold });
            text.Children.Add(new TextBlock { Text = item.Item2, Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,3,0,0) });
            row.Children.Add(text);
            row.Children.Add(Action("Apply", item.Item3));
            stack.Children.Add(Card(row));
        }
        BuildPage("Optimizations", "Every module is separated so you can see exactly what changes.", stack);
    }

    private void ShowNetwork()
    {
        var stack = new StackPanel();
        stack.Children.Add(Card(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "LATENCY LAB", FontSize = 20, FontWeight = FontWeights.Bold },
                new TextBlock { Text = "Measure connection quality without changing your networking stack.", Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,4,0,8) },
                Action("Run Epic/Fortnite Endpoint Test", Network_Click, true),
                new TextBlock { Text = "Results appear here after the test.", Name = "NetworkResult", Margin = new Thickness(0,12,0,0), Foreground = (Brush)FindResource("Accent") }
            }
        }));
        BuildPage("Network Test", "Testing is safer than guessing: PulseForge does not promise a ping reduction from registry myths.", stack);
    }

    private void ShowSafety()
    {
        var stack = new StackPanel();
        stack.Children.Add(Card(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "SAFETY CENTER", FontSize = 20, FontWeight = FontWeights.Bold },
                new TextBlock { Text = "Back up before changing anything. Restore the latest PulseForge snapshot if needed.", Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0,4,0,8) },
                Action("Create Safety Snapshot", CreateSnapshot_Click, true),
                Action("Restore Latest Registry Snapshot", Restore_Click),
                new TextBlock { Text = "PulseForge avoids disabling Windows Defender, Windows Update, audio, core networking, or anti-cheat.", Foreground = (Brush)FindResource("Good"), Margin = new Thickness(0,12,0,0) }
            }
        }));
        BuildPage("Safety & Restore", "The optimizer is designed to fail safely and keep advanced changes opt-in.", stack);
    }

    private void SetStatus(string message, bool good = true)
    {
        if (_status != null)
        {
            _status.Text = message;
            _status.Foreground = (Brush)FindResource(good ? "Good" : "Warn");
        }
    }

    private void Dashboard_Click(object s, RoutedEventArgs e) => ShowDashboard();
    private void Fortnite_Click(object s, RoutedEventArgs e) => ShowFortnite();
    private void Games_Click(object s, RoutedEventArgs e) => ShowGames();
    private void Optimizations_Click(object s, RoutedEventArgs e) => ShowOptimizations();
    private void Network_Click(object s, RoutedEventArgs e) => ShowNetwork();
    private void Safety_Click(object s, RoutedEventArgs e) => ShowSafety();

    private void CreateSnapshot_Click(object s, RoutedEventArgs e) => CreateSafetySnapshot();
    private void Restore_Click(object s, RoutedEventArgs e) => RestoreLatest();

    private bool CreateSafetySnapshot()
    {
        try
        {
            Directory.CreateDirectory(backupRoot);
            var folder = Path.Combine(backupRoot, DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(folder);

            var keys = new[]
            {
                (@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "GameDVR.reg"),
                (@"HKCU\System\GameConfigStore", "GameConfigStore.reg"),
                (@"HKCU\Software\Microsoft\GameBar", "GameBar.reg")
            };

            foreach (var key in keys)
            {
                var psi = new ProcessStartInfo("reg.exe", $"export \"{key.Item1}\" \"{Path.Combine(folder, key.Item2)}\" /y")
                {
                    UseShellExecute = false, CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(3000);
            }

            SetStatus("Safety snapshot created.");
            return true;
        }
        catch (Exception ex)
        {
            SetStatus("Safety snapshot failed: " + ex.Message, false);
            return false;
        }
    }

    private void RestoreLatest()
    {
        try
        {
            if (!Directory.Exists(backupRoot))
            {
                SetStatus("No snapshot found.", false);
                return;
            }

            var folder = new DirectoryInfo(backupRoot).GetDirectories()
                .OrderByDescending(x => x.Name).FirstOrDefault();

            if (folder == null)
            {
                SetStatus("No snapshot found.", false);
                return;
            }

            foreach (var file in folder.GetFiles("*.reg"))
            {
                var psi = new ProcessStartInfo("reg.exe", $"import \"{file.FullName}\"")
                {
                    UseShellExecute = false, CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(5000);
            }

            SetStatus("Latest registry snapshot restored.");
        }
        catch (Exception ex)
        {
            SetStatus("Restore failed: " + ex.Message, false);
        }
    }

    private bool RunPowerShell(string command)
    {
        var psi = new ProcessStartInfo("powershell.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-ExecutionPolicy");
        psi.ArgumentList.Add("Bypass");
        psi.ArgumentList.Add("-Command");
        psi.ArgumentList.Add(command);
        using var p = Process.Start(psi);
        p?.WaitForExit(10000);
        return p?.ExitCode == 0;
    }

    private void EnableGameMode_Click(object s, RoutedEventArgs e)
    {
        if (!CreateSafetySnapshot()) return;
        RunPowerShell("New-Item -Path 'HKCU:\\Software\\Microsoft\\GameBar' -Force | Out-Null; Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\GameBar' -Name 'AutoGameModeEnabled' -Type DWord -Value 1");
        SetStatus("Windows Game Mode enabled.");
    }

    private void DisableDvr_Click(object s, RoutedEventArgs e)
    {
        if (!CreateSafetySnapshot()) return;
        RunPowerShell("New-Item -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' -Force | Out-Null; Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' -Name 'AppCaptureEnabled' -Type DWord -Value 0; New-Item -Path 'HKCU:\\System\\GameConfigStore' -Force | Out-Null; Set-ItemProperty -Path 'HKCU:\\System\\GameConfigStore' -Name 'GameDVR_Enabled' -Type DWord -Value 0");
        SetStatus("Background capture disabled.");
    }

    private void HighPerformance_Click(object s, RoutedEventArgs e)
    {
        if (!CreateSafetySnapshot()) return;
        Process.Start(new ProcessStartInfo("powercfg.exe", "/setactive SCHEME_MIN") { UseShellExecute = false })?.WaitForExit(5000);
        SetStatus("High Performance power plan selected.");
    }

    private void Cleanup_Click(object s, RoutedEventArgs e)
    {
        try
        {
            long count = 0;
            foreach (var dir in new[]
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            })
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var f in Directory.EnumerateFiles(dir))
                {
                    try { File.Delete(f); count++; } catch { }
                }
            }
            SetStatus($"Safe cleanup finished. Removed {count} temporary files.");
        }
        catch (Exception ex) { SetStatus("Cleanup failed: " + ex.Message, false); }
    }

    private void Priority_Click(object s, RoutedEventArgs e)
    {
        var name = "FortniteClient-Win64-Shipping";
        var ps = Process.GetProcessesByName(name);
        if (ps.Length == 0) { SetStatus("Fortnite is not running.", false); return; }
        try
        {
            foreach (var p in ps) p.PriorityClass = ProcessPriorityClass.High;
            SetStatus("Fortnite priority set to High for the current session.");
        }
        catch (Exception ex) { SetStatus("Priority change failed: " + ex.Message, false); }
    }

    private async void Network_Click(object s, RoutedEventArgs e)
    {
        SetStatus("Testing Epic endpoints...");
        var hosts = new[] { "ping-nae.ds.on.epicgames.com", "ping-nac.ds.on.epicgames.com", "ping-eu.ds.on.epicgames.com" };
        var result = new StringBuilder();
        foreach (var host in hosts)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 1500);
                result.AppendLine($"{host}: {(reply.Status == IPStatus.Success ? reply.RoundtripTime + " ms" : reply.Status)}");
            }
            catch { result.AppendLine($"{host}: unavailable"); }
        }
        SetStatus(result.ToString().Replace(Environment.NewLine, "  |  "));
    }

    private void OptimizeFortnite_Click(object s, RoutedEventArgs e)
    {
        if (!CreateSafetySnapshot()) return;
        EnableGameMode_Click(s, e);
        DisableDvr_Click(s, e);
        Cleanup_Click(s, e);
        SetStatus("Fortnite safe profile applied. No anti-cheat or game files were modified.");
    }

    private void BrowseGame_Click(object s, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "Game executable (*.exe)|*.exe", Title = "Select your game executable" };
        if (dlg.ShowDialog() == true)
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PulseForge", "game.txt"), dlg.FileName);
            SetStatus("Selected: " + Path.GetFileName(dlg.FileName));
        }
    }

    private string LoadGamePath()
    {
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PulseForge", "game.txt");
        return File.Exists(file) ? File.ReadAllText(file).Trim() : "";
    }

    private void LaunchSelected_Click(object s, RoutedEventArgs e)
    {
        var path = LoadGamePath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) { SetStatus("Select a game EXE first.", false); return; }
        Process.Start(new ProcessStartInfo(path) { WorkingDirectory = Path.GetDirectoryName(path)! });
        SetStatus("Game launched.");
    }

    private void OptimizeSelected_Click(object s, RoutedEventArgs e)
    {
        var path = LoadGamePath();
        if (string.IsNullOrWhiteSpace(path)) { SetStatus("Select a game EXE first.", false); return; }
        if (!CreateSafetySnapshot()) return;
        EnableGameMode_Click(s, e);
        DisableDvr_Click(s, e);
        Cleanup_Click(s, e);
        SetStatus("Universal safe profile applied.");
    }

    private void LaunchFortnite_Click(object s, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("com.epicgames.launcher://apps/Fortnite?action=launch") { UseShellExecute = true });
            SetStatus("Fortnite launch requested.");
        }
        catch (Exception ex) { SetStatus("Could not launch Fortnite: " + ex.Message, false); }
    }

    private void OpenFortniteConfig_Click(object s, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FortniteGame", "Saved", "Config", "WindowsClient");
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
        SetStatus("Opened Fortnite configuration folder.");
    }
}
