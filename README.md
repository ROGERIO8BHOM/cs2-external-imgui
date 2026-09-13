# CS2 External ImGui

![Language](https://img.shields.io/badge/language-C%23-512BD4?style=flat-square)
![Framework](https://img.shields.io/badge/framework-.NET%208-512BD4?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D6?style=flat-square)
![Status](https://img.shields.io/badge/status-learning%20project-orange?style=flat-square)

My first external Counter-Strike 2 project, developed as an educational experiment to better understand reverse engineering, process memory, data structures, real-time rendering, and how computers read and process data.

The application runs as a separate process, reads selected game state from `client.dll`, maintains a local entity cache, and displays information through a transparent ImGui overlay. The codebase is intentionally organized into small modules so each concept can be studied independently.

> [!WARNING]
> This repository is intended strictly for education and research in controlled environments. Using game modifications in public or competitive matches may violate Valve's terms of service and can result in account restrictions. Do not use this project to disrupt other players' experience. This project does not include or claim any anti-cheat bypass.

## Features

### Combat

- **Aimbot** — selects a target by FOV or distance, supports configurable target bones and smoothing, and can optionally filter teammates.
- **Triggerbot** — fires when a valid entity is under the crosshair, with an optional teammate setting.
- **No Recoil** — compensates the current aim punch using the previous recoil state and configurable compensation strength.
- **Aimbot/No Recoil integration** — both modules share the same recoil model so they do not fight over view-angle updates.

### Visuals

- **ESP boxes**
- **Player names**
- **Health bars**
- **Skeleton/bone rendering**
- **Tracer lines**
- **View-direction lines**
- **Enemy and teammate color customization**
- **Aimbot FOV circle**
- **Active-feature list**
- **FOV changer module**
- **Anti-flash module**

### Interface

- Transparent, always-on-top ImGui overlay
- Modular pages, tabs, groups, toggles, sliders, combo boxes, color editors, and keybind controls
- Draggable active-feature panel
- Runtime settings stored in a central configuration model

## Default controls

| Action | Default key |
| --- | --- |
| Open/close menu | `Insert` |
| Panic/exit | `End` |
| Hold Aimbot | `F` |
| Triggerbot key setting | `Mouse 4` (`XBUTTON1`) |

Most options can be changed through the overlay. All modules start disabled except ESP, which is enabled by default in the current configuration.

## How it works

```text
Counter-Strike 2
      │
      │ process memory
      ▼
  GameContext ──► EntityCache ──► Entity / Skeleton data
      │
      ├──► Combat modules ──► view angles and input state
      │
      └──► Visual modules ──► transparent ImGui overlay
```

The main loop refreshes the view matrix and entity cache, then updates the enabled combat and visual modules. `GameContext` owns the shared memory interface, module base address, settings, screen size, and cached entities. This avoids every feature independently scanning the same data.

The overlay runs on a separate rendering thread. It becomes interactive while the menu is open and click-through when the menu is hidden.

## Project structure

```text
CS2/
├── Config/                 Runtime feature settings
├── Core/                   Game context, entity cache, matrix and skeleton data
├── Enums/                  Aim bones and UI-related enums
├── Helpers/                Entity and bone helpers
├── Modules/
│   ├── Combat/             Aimbot, Triggerbot, No Recoil and target selection
│   └── Visual/             ESP, Anti-Flash and FOV changer
├── Offsets/                Generated client schemas, inputs and offsets
├── Settings/               Reusable setting types
├── UI/                     ImGui menu, theme, animations and widgets
├── Calculations.cs         Angle, smoothing and world-to-screen calculations
├── Input.cs                Keyboard and mouse state handling
├── Renderer.cs             Overlay composition and module controls
└── Program.cs              Application entry point and update loop
```

## Requirements

- Windows 10 or Windows 11 (64-bit)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Counter-Strike 2
- Visual Studio 2022 or another C#/.NET-compatible IDE (optional)

NuGet dependencies are restored automatically:

- `ClickableTransparentOverlay`
- `Microsoft.Web.WebView2`
- `swed64`

## Building

Clone the repository and enter its directory:

```powershell
git clone https://github.com/ROGERIO8BHOM/cs2-external-imgui.git
cd cs2-external-imgui
```

Restore and build the project:

```powershell
dotnet restore
dotnet build -c Release
```

The compiled output will be placed under:

```text
bin/Release/net8.0/
```

You can also open `CS2.slnx` in Visual Studio, select the `x64` configuration, and build the solution.

## Running locally

1. Start Counter-Strike 2.
2. Build the project for `x64`.
3. Run the compiled application in your own controlled testing environment.
4. Press `Insert` to show or hide the menu.
5. Enable and configure the modules you want to study.
6. Press `End` to immediately close the application.

The application expects a running process named `cs2` and resolves the base address of `client.dll` during startup.

## Offsets and game updates

Counter-Strike 2 updates can change class layouts and memory offsets. When that happens, features may stop working even though the project still compiles. The generated files inside `Offsets/` must match the currently installed game version.

Never assume that an old offset is still valid. Invalid addresses can produce incorrect data or cause the application to fail. Keep schema and offset changes isolated from feature logic and validate values before using them.

## Current limitations

- Windows-only and x64-only.
- Settings currently live in memory; the menu's config action is not yet a complete persistence system.
- Offset files require manual regeneration or replacement after relevant game updates.
- The project has no automated test suite yet.
- Some menu sections are placeholders for future experiments.
- This is an early learning project and should not be considered production-ready software.

## Learning goals

This project is being used to study:

- Reading and writing another process's memory
- Pointer chains, module bases, schemas, and offsets
- Entity-list traversal and cached state
- 3D vectors, view matrices, and world-to-screen projection
- Angle calculation, normalization, recoil compensation, and smoothing
- Real-time overlays and immediate-mode interfaces with ImGui
- Modular C# architecture for continuously updated features
- Defensive validation of pointers and externally sourced values

## Contributing

This is primarily a personal learning repository, but constructive suggestions and educational improvements are welcome. Please keep contributions focused on code quality, architecture, documentation, stability, and safe research practices.

## Disclaimer

This project is not affiliated with, endorsed by, or sponsored by Valve Corporation. Counter-Strike, Counter-Strike 2, Steam, and their respective logos are trademarks or registered trademarks of Valve Corporation.

The author is not responsible for account penalties, data loss, misuse, or any other consequences resulting from the use of this source code. You are responsible for understanding and following the rules that apply to any software or service you interact with.
