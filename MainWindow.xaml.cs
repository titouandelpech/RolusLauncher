using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RolusLauncher
{
    public partial class MainWindow : Window
    {
        private readonly LauncherService launcherService;
        private readonly DispatcherTimer statusTimer;
        private string gameExecutablePath = "";

        public MainWindow()
        {
            InitializeComponent();
            launcherService = new LauncherService();
            launcherService.OnStatusChanged += UpdateStatus;
            launcherService.OnProgressChanged += UpdateProgress;
            launcherService.OnVersionChecked += OnVersionChecked;
            launcherService.OnDownloadComplete += OnDownloadComplete;
            launcherService.OnError += OnError;
            launcherService.OnVersionsLoaded += OnVersionsLoaded;

            statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            statusTimer.Tick += (s, e) => UpdateUI();
            statusTimer.Start();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentVersionText.Text = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            await CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            try
            {
                await launcherService.CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatus(string status, string subStatus = "")
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = status;
                StatusSubText.Text = subStatus;
            });
        }

        private void UpdateProgress(int percentage, string message = "")
        {
            Dispatcher.Invoke(() =>
            {
                DownloadProgress.Value = percentage;
                ProgressText.Text = message;
                DownloadProgress.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Visible;
            });
        }

        private void OnVersionChecked(VersionInfo versionInfo, bool isUpdateAvailable)
        {
            Dispatcher.Invoke(() =>
            {
                GameVersionText.Text = $"Latest Version: {versionInfo.Version}";
                GameSizeText.Text = $"Size: {FormatBytes(versionInfo.Size)}";

                if (isUpdateAvailable)
                {
                    StatusText.Text = "Update Available";
                    StatusSubText.Text = $"Version {versionInfo.Version} is available for download.";
                    UpdateButton.IsEnabled = true;
                    PlayButton.IsEnabled = false;
                }
                else
                {
                    StatusText.Text = "Game is up to date";
                    StatusSubText.Text = "You have the latest version installed.";
                    UpdateButton.IsEnabled = false;
                    PlayButton.IsEnabled = true;
                    gameExecutablePath = launcherService.GetGameExecutablePath();
                }
            });
        }

        private void OnDownloadComplete()
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Download Complete";
                StatusSubText.Text = "The game has been successfully updated.";
                DownloadProgress.Visibility = Visibility.Collapsed;
                ProgressText.Visibility = Visibility.Collapsed;
                UpdateButton.IsEnabled = false;
                PlayButton.IsEnabled = true;
                gameExecutablePath = launcherService.GetGameExecutablePath();
            });
        }

        private void OnError(string error)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error";
                StatusSubText.Text = error;
            });
        }

        private void UpdateUI()
        {
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            try
            {
                await launcherService.DownloadAndInstall();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateButton.IsEnabled = true;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gameExecutablePath) || !File.Exists(gameExecutablePath))
            {
                MessageBox.Show("Game executable not found. Please update the game first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = gameExecutablePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnVersionsLoaded(List<VersionInfo> versions, VersionInfo latest)
        {
            Dispatcher.Invoke(() =>
            {
                VersionComboBox.ItemsSource = versions;
                VersionComboBox.DisplayMemberPath = "Version";
                VersionComboBox.SelectedItem = latest;
            });
        }

        private void VersionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is VersionInfo selectedVersion)
            {
                launcherService.SelectVersion(selectedVersion);
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

