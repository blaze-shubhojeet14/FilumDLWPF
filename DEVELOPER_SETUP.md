# Developer Setup Guide

This guide will help you set up the FilumDLWPF project for development.

## Prerequisites

- .NET 8.0 SDK or later
- Windows 7 or newer (64-bit) OS
- Visual Studio 2022 or JetBrains Rider (recommended for development)
- Spotify Developer Account (for Spotify downloader feature)

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/blaze-shubhojeet14/FilumDLWPF.git
cd FilumDLWPF
```

### 2. Set Up Spotify API Credentials

The application requires Spotify API credentials to download music from Spotify.

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications)
2. Log in with your Spotify account
3. Click "Create an App"
4. Give it a name and description
5. Copy the **Client ID** and **Client Secret**

### 3. Create Secrets File

1. Copy the example secrets file:
   ```bash
   cp Secrets.cs.example Secrets.cs
   ```

2. Open `Secrets.cs` and replace the placeholder values with your actual credentials:
   ```csharp
   public string SetSpotifyClientID()
   {
       return "your_actual_client_id_here";
   }

   public string SetSpotifyClientSecret()
   {
       return "your_actual_client_secret_here";
   }
   ```

3. **Important**: Never commit `Secrets.cs` to git. It's already in `.gitignore` to prevent accidental commits.

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Build the Project

```bash
dotnet build
```

## Running the Application

### From Command Line

```bash
dotnet run
```

### From Visual Studio

1. Open `FilumDLWPF.sln` in Visual Studio
2. Press F5 or click the "Start" button

## Project Structure

- `MainWindow.xaml.cs` - Main application window
- `YTWindow.xaml.cs` - YouTube downloader interface
- `SpotWindow.xaml.cs` - Spotify downloader interface
- `VideoWindow.xaml.cs` - Video preview player
- `Secrets.cs` - API credentials (not in git, must be created)

## Troubleshooting

### Build Errors

If you encounter build errors related to `Secrets` class:
- Make sure you've created `Secrets.cs` from the example file
- Verify the namespace is `FilumDLWPF`

### Missing Dependencies

Run:
```bash
dotnet restore
```

### Windows-Specific Issues

This is a WPF application and requires Windows to build and run. On non-Windows systems, you can:
- Use a Windows VM
- Use GitHub Actions for CI/CD (as the build system)

## Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit a pull request

## License

MIT License - See LICENSE file for details
