# Pomodoro

A minimalist, always-on-top Pomodoro widget for Windows. Built with WPF on .NET 10, with no NuGet dependencies in the app project. Includes optional Todoist integration.

## Features

- Borderless, always-on-top window. No Windows chrome (minimize/maximize/close); drag the window by clicking its background.
- Launch at login via an `HKCU\...\Run` registry entry, toggled in settings.
- Three modes: Pomodoro, short break, long break. A long break replaces the short one every N completed pomodoros.
- Optional auto-start of breaks and pomodoros, plus a sound when a mode ends.
- Todoist task list (unified API v1). Click a task to complete it. Optional filter (for example `today` or `#Work`).
- Configuration and window position are stored in `%APPDATA%\Pomodoro\settings.json`.

## Requirements

- Windows 10 or 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build

## Build and run

```powershell
git clone <repo-url>
cd Pomodoro

# Run directly
dotnet run -c Release

# Or build the executable
dotnet build -c Release
# -> bin\Release\net10.0-windows\Pomodoro.exe
```

For autostart, a single self-contained file is recommended:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false `
  -p:PublishSingleFile=true -o publish
# -> publish\Pomodoro.exe
```

## Usage

### Timer

- START / PAUSE starts and stops the countdown.
- Reset restores the full duration of the current mode.
- The mode tabs (Pomodoro, short break, long break) switch modes manually. The active mode is highlighted and the window background changes color.
- When the timer reaches zero, a sound plays (if enabled) and the timer advances to the next mode. Every `LongBreakInterval` pomodoros, a long break runs instead of a short one.
- Drag the window by clicking and holding its background. The position is saved on close.

### Settings

The settings window configures:

- Durations for Pomodoro, short break, and long break.
- `LongBreakInterval`: how many pomodoros precede a long break.
- Auto-start of breaks and auto-start of pomodoros.
- End-of-mode sound on or off.
- Launch at login.
- Todoist token and filter.

Changes are saved on confirmation and take effect immediately.

### Todoist (optional)

1. Get an API token from Todoist: Settings > Integrations > Developer > API token.
2. Paste the token into the widget settings.
3. The task list appears in the window. Pick a project from the dropdown or leave it set to all projects.
4. An optional filter narrows tasks using Todoist filter syntax, for example `today`, `overdue`, or `#Work`. Project selection takes precedence over the filter.
5. Click a task to complete it in Todoist.
6. The sync button refreshes the list.

The token is stored locally only, in `%APPDATA%\Pomodoro\settings.json`. It is never committed.

## Tests

```powershell
dotnet test Tests\Pomodoro.Tests.csproj
```

## Architecture

The design separates a pure core from platform adapters behind interfaces, so the timer and task-list logic can be unit-tested without UI, threads, real time, network, or disk.

| File or directory | Role |
|-------------------|------|
| `Models/` | `AppSettings`, `TimerMode`, `TodoistTask`, `TodoistProject`, and pagination pages |
| `Services/PomodoroEngine` | Pure timer state machine (no UI, no threads) |
| `Services/PomodoroSession` | Session module: tick to finish to auto-start, exposed via `Changed` and `Finished` events |
| `Services/IClock`, `DispatcherClock` | One-second clock abstraction (test clock advances ticks synchronously) |
| `Services/HttpTodoistGateway` | Todoist API v1 (HttpClient, cursor pagination) |
| `Services/TaskListModel` | Task-list logic (projects, filter, selection, task completion) |
| `Services/SettingsService`, `SettingsStore` | Single owner of live settings; the only place persistence happens (JSON in `%APPDATA%`) |
| `Services/AutoStartManager` | Registry `Run` entry for launch at login |
| `Presentation/ModeTheme` | Per-mode background colors |
| `MainWindow`, `SettingsWindow` | The widget and the settings dialog |

## Notes

- The Todoist token is stored locally only. Do not commit it.
- Todoist REST API v2 returns `410 Gone`; this app uses the unified API v1 (`api.todoist.com/api/v1`).
