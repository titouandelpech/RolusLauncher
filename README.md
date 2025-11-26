# Rolus Game Launcher

A modern, clean launcher application for the Rolus Unity game that automatically checks for updates and manages game installation.

## Features

- **Automatic Version Checking**: Checks for new versions from a remote server
- **No Launcher Updates Needed**: The launcher fetches version information from the internet, so you only need to update the `version.json` file on your server - users never need to download a new launcher
- **Download & Install**: Automatically downloads and installs game updates
- **Modern UI**: Clean, dark-themed WPF interface
- **Progress Tracking**: Real-time download and installation progress
- **Game Launching**: One-click game launch after installation

## Setup

### 1. Configure Version Endpoint

Edit `LauncherService.cs` and update the `VERSION_URL` constant with your server URL:

```csharp
private const string VERSION_URL = "https://your-server.com/rolus/version.json";
```

### 2. Create Version JSON File on Your Server

**Important**: The launcher fetches version information from this file on every launch. You only need to update this file on your server when releasing new game versions - the launcher itself never needs to be recompiled or redistributed.

Create a `version.json` file on your server with the following structure:

```json
{
  "version": "1.0.0",
  "downloadUrl": "https://your-server.com/rolus/builds/Rolus_v1.0.0.zip",
  "size": 104857600,
  "executable": "Rolus.exe"
}
```

- `version`: Version string (e.g., "1.0.0")
- `downloadUrl`: Direct download URL to the compressed build (ZIP file)
- `size`: Size of the download in bytes
- `executable`: Name of the game executable file

### 3. Prepare Game Build

1. Build your Unity game as a Windows standalone build
2. Compress the entire build folder into a ZIP file
3. Upload the ZIP file to your server
4. **Update the `version.json` file on your server** with the new version information and download URL

**That's it!** Users with the launcher will automatically see the new version on their next launch. No need to redistribute the launcher.

### 4. Build the Launcher

```bash
cd Launcher
dotnet publish -c Release -r win-x64 --self-contained true
```

The executable will be in `bin/Release/net8.0-windows/win-x64/publish/RolusLauncher.exe`

### 5. Code Signing (Optional but Recommended)

To sign the executable for security:

1. Obtain a code signing certificate
2. Use `signtool.exe` (from Windows SDK):

```bash
signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com RolusLauncher.exe
```

Or use a tool like [SignTool](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool) or [OSS Code Signing](https://www.ssl.com/code-signing-certificate/) services.

## Usage

### For Users

1. Download and run `RolusLauncher.exe` (one-time setup)
2. The launcher automatically checks for updates on every launch
3. If an update is available, click "Update" to download and install
4. Click "Play" to launch the game

### For Developers (Releasing New Versions)

1. Build your Unity game and compress it to a ZIP file
2. Upload the ZIP to your server
3. Update the `version.json` file on your server with:
   - New version number
   - New download URL
   - File size
4. Done! Users will see the update automatically on their next launcher launch

**You never need to rebuild or redistribute the launcher** - it automatically fetches version information from your server.

## Installation Location

The game is installed to: `%LocalAppData%\RolusGame\`

## Customization

### Change Colors

Edit `Styles.xaml` to customize the color scheme:

- `PrimaryColor`: Main accent color
- `BackgroundColor`: Window background
- `SurfaceColor`: Card/panel backgrounds
- `TextColor`: Primary text color

### Change Window Size

Edit `MainWindow.xaml`:

```xml
Height="600" Width="900"
```

## Requirements

- .NET 8.0 Runtime (included in self-contained build)
- Windows 10/11
- Internet connection for version checking and downloads

