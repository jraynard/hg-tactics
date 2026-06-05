# Heavy Gear Tactics

A turn-based hex-grid tactics game set in the Heavy Gear universe, ported from XNA 4.0 to **MonoGame 3.8 (DesktopGL)** targeting **.NET 10**.

## Status

The `port` branch contains a complete MonoGame port of the original XNA 4.0 codebase. The game builds and runs on Linux (tested on CachyOS). Core gameplay, menus, local multiplayer lobby, and the in-game map editor are functional.

## Building

**Prerequisites**

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SDL2 runtime libraries (Linux: `sudo apt install libsdl2-2.0-0 libopenal1` or equivalent for your distro)

**Restore tools and build**

```bash
dotnet tool restore   # installs the MonoGame Content Builder (mgcb) locally
dotnet build HeavyGear.slnx
```

**Run**

```bash
dotnet run --project src/HeavyGear.Desktop/HeavyGear.Desktop.csproj
```

## Project Structure

```
HeavyGear.slnx                  # SDK-style solution (requires .NET 9+ SDK)
Directory.Build.props           # Shared: net10.0, Nullable=disable
.config/dotnet-tools.json       # Local tool: dotnet-mgcb 3.8.4.1

src/
  HeavyGear.Core/               # Class library — all game logic, graphics, audio, networking
  HeavyGear.Desktop/            # DesktopGL executable entry point

Content/                        # Source assets (processed by MGCB at build time)
  Textures/                     # PNG spritesheets
  Audio/                        # WAV sound effects
  Maps/                         # Hex map XML files + preview PNGs
  Shaders/                      # HLSL fx files (LineRendering.fx — requires Wine on Linux to compile)

Source/                         # Original XNA 4.0 source (preserved for reference, not built)
```

## Key Architecture

| Area | Implementation |
|---|---|
| Framework | MonoGame 3.8.4.1 DesktopGL (OpenGL / SDL2) |
| Rendering | SpriteBatch (2D), custom vertex buffers for line rendering |
| Map format | Custom XML hex grid, loaded directly at runtime (not via content pipeline) |
| Audio | `SoundEffect` via `ContentManager` (4 menu WAVs) |
| Networking | `ITransport` abstraction — `LiteNetLibTransport` (LAN UDP, port 7777) or `NullTransport` stub |
| Settings | `GameSettings` (XML serialized, stored in `~/.config/HeavyGear/`) |
| Map editor | In-game editor screen — Tab cycles tools, left-click applies, Ctrl+S saves to AppData |

## Networking

LAN multiplayer uses [LiteNetLib](https://github.com/RevenantX/LiteNetLib) UDP (port 7777). One player hosts, others discover via LAN broadcast and join. From the main menu select **Network** (debug builds only) to access the lobby.

## Known Limitations

- **Shader compilation** (`LineRendering.fx`) requires Wine + `d3dcompiler_47` on Linux. The shader loads gracefully on failure — debug line rendering is disabled but the game runs normally.
- **Online / internet play** is not implemented. LAN only.
- **Audio** — only the 4 menu sound effects are ported. In-game sounds from the original XACT project are removed.

## Original Project

The original game was built with Microsoft XNA Game Studio 4.0 targeting .NET 4.0 / Xbox 360. The `historical` branch preserves that original source.
