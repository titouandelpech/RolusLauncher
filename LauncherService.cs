#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RolusLauncher
{
    public class VersionInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; } = "";

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("executable")]
        public string Executable { get; set; } = "";
    }

    public class VersionManifest
    {
        [JsonProperty("latest")]
        public string Latest { get; set; } = "";

        [JsonProperty("versions")]
        public List<VersionInfo> Versions { get; set; } = new();
    }

    public class LauncherService
    {
        // This URL points to a version.json file on your server
        // Update this file on your server when releasing new game versions
        // The launcher will automatically detect new versions without needing to be recompiled
        // Optional: Add ?token=YOUR_TOKEN at the end if you set VERSION_TOKEN on Railway
        private const string VERSION_URL = "https://rolus-production.up.railway.app/version.json";
        private const string INSTALL_DIR = "RolusGame";
        private readonly string installPath;
        private readonly HttpClient httpClient;
        private VersionInfo? currentVersionInfo;
        private string? currentVersionFile;
        private VersionManifest? versionManifest;
        private List<VersionInfo>? availableVersions;

        public event Action<string, string>? OnStatusChanged;
        public event Action<int, string>? OnProgressChanged;
        public event Action<VersionInfo, bool>? OnVersionChecked;
        public event Action? OnDownloadComplete;
        public event Action<string>? OnError;
        public event Action<List<VersionInfo>, VersionInfo>? OnVersionsLoaded;

        public LauncherService()
        {
            installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), INSTALL_DIR);
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            currentVersionFile = Path.Combine(installPath, "version.txt");
        }

        public async Task CheckForUpdates()
        {
            try
            {
                OnStatusChanged?.Invoke("Checking for updates...", "Connecting to server...");

                var response = await httpClient.GetStringAsync(VERSION_URL);
                versionManifest = JsonConvert.DeserializeObject<VersionManifest>(response);

                if (versionManifest == null || versionManifest.Versions == null || versionManifest.Versions.Count == 0)
                {
                    // Fallback to old format for backwards compatibility
                    currentVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(response);
                    if (currentVersionInfo == null || string.IsNullOrEmpty(currentVersionInfo.Version))
                    {
                        OnError?.Invoke("Invalid version information received from server.");
                        return;
                    }
                    availableVersions = new List<VersionInfo> { currentVersionInfo };
                    OnVersionsLoaded?.Invoke(availableVersions, currentVersionInfo);
                }
                else
                {
                    availableVersions = versionManifest.Versions;
                    
                    // Find latest version
                    VersionInfo? latest = availableVersions.FirstOrDefault(v => v.Version == versionManifest.Latest);
                    if (latest == null)
                    {
                        latest = availableVersions.First();
                    }
                    currentVersionInfo = latest;
                    OnVersionsLoaded?.Invoke(availableVersions, latest);
                }

                string? installedVersion = GetInstalledVersion();
                bool isUpdateAvailable = installedVersion != currentVersionInfo.Version;

                OnVersionChecked?.Invoke(currentVersionInfo, isUpdateAvailable);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to check for updates: {ex.Message}");
            }
        }

        public async Task DownloadAndInstall()
        {
            if (currentVersionInfo == null)
            {
                OnError?.Invoke("Version information not available. Please check for updates first.");
                return;
            }

            try
            {
                OnStatusChanged?.Invoke("Downloading update...", $"Downloading version {currentVersionInfo.Version}...");

                string tempZipPath = Path.Combine(Path.GetTempPath(), $"Rolus_{currentVersionInfo.Version}.zip");

                using (var response = await httpClient.GetAsync(currentVersionInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    long downloadedBytes = 0;

                    using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes.HasValue)
                            {
                                int progress = (int)((downloadedBytes * 100) / totalBytes.Value);
                                string progressText = $"{FormatBytes(downloadedBytes)} / {FormatBytes(totalBytes.Value)}";
                                OnProgressChanged?.Invoke(progress, progressText);
                            }
                        }
                    }
                }

                OnStatusChanged?.Invoke("Installing update...", "Extracting files...");
                OnProgressChanged?.Invoke(0, "Extracting...");

                if (Directory.Exists(installPath))
                {
                    Directory.Delete(installPath, true);
                }
                Directory.CreateDirectory(installPath);

                ZipFile.ExtractToDirectory(tempZipPath, installPath);

                if (currentVersionFile != null)
                {
                    await File.WriteAllTextAsync(currentVersionFile, currentVersionInfo.Version);
                }

                File.Delete(tempZipPath);

                OnProgressChanged?.Invoke(100, "Complete");
                OnDownloadComplete?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to download or install update: {ex.Message}");
                throw;
            }
        }

        public string GetGameExecutablePath()
        {
            string? executableName = currentVersionInfo?.Executable;
            if (string.IsNullOrEmpty(executableName))
            {
                executableName = "Rolus.exe";
            }

            string expectedPath = Path.Combine(installPath, executableName);
            
            if (File.Exists(expectedPath))
            {
                return expectedPath;
            }

            if (!Directory.Exists(installPath))
            {
                return expectedPath;
            }

            string[] exeFiles = Directory.GetFiles(installPath, "*.exe", SearchOption.AllDirectories);
            if (exeFiles.Length > 0)
            {
                return exeFiles[0];
            }

            return expectedPath;
        }

        private string? GetInstalledVersion()
        {
            if (currentVersionFile != null && File.Exists(currentVersionFile))
            {
                return File.ReadAllText(currentVersionFile).Trim();
            }
            return null;
        }

        public void SelectVersion(VersionInfo version)
        {
            currentVersionInfo = version;
            string? installedVersion = GetInstalledVersion();
            bool isUpdateAvailable = installedVersion != currentVersionInfo.Version;
            OnVersionChecked?.Invoke(currentVersionInfo, isUpdateAvailable);
        }

        public List<VersionInfo>? GetAvailableVersions()
        {
            return availableVersions;
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

