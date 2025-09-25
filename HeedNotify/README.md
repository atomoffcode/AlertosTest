## HeedNotify (WPF) — Portable Desktop Notification App

HeedNotify is a Windows tray application that shows toast-style notifications based on schedules. This package contains source code ready to open in Visual Studio or build with .NET 8 on Windows.

### Build (Windows)
1. Install .NET 8 SDK and Visual Studio (with .NET Desktop Development) or use `dotnet` CLI.
2. Open `HeedNotify/HeedNotify.csproj` in Visual Studio and Build, or run:
   - `dotnet build HeedNotify/HeedNotify.csproj -c Release`
   - Publish (framework-dependent):
     `dotnet publish HeedNotify/HeedNotify.csproj -c Release -r win-x64 --self-contained false -o publish`

### Run
- After build/publish, run `HeedNotify.exe`. It appears in the system tray.
- Tray menu: Enabled, Test Notification, Reload Config, Open Config/Logs, Start with Windows, Exit.

### Config
- Global: `C:\ProgramData\HeedNotify\config.json`
- Per-user: `%AppData%\HeedNotify\config.json`
- Default file provided: `HeedNotify/appsettings.json` (copy into desired location).

### CLI
- One-off: `HeedNotify.exe --notify "Title|Message|5" --exit-after`
- Enable/Disable: `--enable` or `--disable`
- Startup: `--install-startup` / `--uninstall-startup`
- Reload: `--reload`

### Notes
- Build must be done on Windows (WPF is Windows-only).
- No outbound network; logs at `%AppData%\HeedNotify\logs`.
