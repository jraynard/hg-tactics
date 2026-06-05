# Plan: Port Heavy Gear to MonoGame

Port the XNA 4.0 game to **MonoGame on .NET 8 (DesktopGL)**, restructure into a clean SDK-style solution, replace the three dead subsystems (`Net`, `GamerServices`, XACT), and **convert the standalone WinForms map editor into an in-game editor screen**. Pure game-logic files carry over with little change; the work concentrates in the XNA boundary layer and the replacements. The existing `PacketType` protocol survives — only the network transport changes.

---

## Locked Decisions

- **Backend:** DesktopGL (cross-platform OpenGL).
- **Map editor:** retire the WinForms project; rebuild as an in-game `EditorScreen`.
- **Multiplayer:** keep LAN, reimplemented on **LiteNetLib**.

---

## Key Facts from Research

- The shared library is **nested inside the map editor** (`Source/HeavyGearMapEditor/HeavyGearLibrary/`) — needs to move out.
- There are **duplicate Windows/Xbox360 csproj pairs**, with a version mismatch (Xbox = XNA 3.0/.NET 3.5, Windows = XNA 4.0/.NET 4.0).
- The map editor **doesn't reference the library** — it has its **own duplicate copies** of `Map.cs` and `HexTile.cs` under `Source/HeavyGearMapEditor/Code/`.
- Net + GamerServices are clustered in just a few files; XACT lives only in `Sounds/Sound.cs`.
- Only 4 `#if !XBOX360` conditionals, all in the game's `Program.cs`.

---

## Target Structure

```
hg-tactics/
  HeavyGear.sln
  Directory.Build.props          # shared net8.0 TFM + version
  src/
    HeavyGear.Core/              # shared library (was HeavyGearLibrary)
      GameLogic/
      GameScreens/               # includes new EditorScreen.cs
      Graphics/
      Helpers/
      Shaders/
      Sounds/
      HeavyGearManager.cs
      GameSettings.cs
      Net/                       # new: ITransport + LiteNetLib implementation
    HeavyGear.Desktop/           # MonoGame DesktopGL executable
      Program.cs
  Content/
    Content.mgcb                 # MonoGame content build
```

The standalone `HeavyGearMapEditor` project is **removed**; its map serialization logic is salvaged into Core.

---

## Phases

### Phase 1 — Solution Skeleton
Create SDK-style projects and `Directory.Build.props` (net8.0). Add `MonoGame.Framework.DesktopGL` and `MonoGame.Content.Builder.Task` NuGet packages to Core and Desktop.

### Phase 2 — Restructure & Flatten *(depends on 1)*
- Move `Source/HeavyGearMapEditor/HeavyGearLibrary/` → `src/HeavyGear.Core/`.
- Delete duplicate Windows/Xbox360 csproj pairs (`HeavyGear.csproj`, `HeavyGearLibrary.csproj`).
- Strip `#if XBOX360`, x86 platform lock, `NoStdLib`, and Client Profile from all files.
- Collapse to a single `net8.0` TFM.

### Phase 3 — Content Pipeline *(parallel with 2)*
- Create `Content/Content.mgcb`.
- Re-add the ~20 textures (mostly drop-in with `TextureImporter`/`TextureProcessor`).
- Recompile `LineRendering.fx` via MGFX (`EffectImporter`/`EffectProcessor`).
- Re-import the 2 map XML files (`XmlImporter`/`PassThroughProcessor`).

### Phase 4 — Audio Rewrite *(depends on 2, 3)*
Rewrite `Sounds/Sound.cs` from XACT (`AudioEngine`/`WaveBank`/`SoundBank`/`Cue`) to `SoundEffect`/`Song`, loading the raw WAV files in `Content/Audio/Waves/`.

### Phase 5 — GamerServices Removal *(depends on 2)*
- Replace the `Guide` storage-selector and sign-in calls in `Helpers/FileHelper.cs` with straightforward local file I/O.
- Drop `GamerServicesComponent` from `HeavyGearManager.cs`.
- Stub out / remove the sign-in UI path in `GameScreens/NetworkScreen.cs`.

### Phase 6 — Networking *(depends on 5)*
- Introduce an `ITransport` interface in `src/HeavyGear.Core/Net/`.
- Reuse the existing `PacketType` enum and packet serialization, swapping `PacketReader`/`PacketWriter` for `BinaryReader`/`BinaryWriter`.
- Implement `ITransport` with **LiteNetLib** (LAN UDP).
- Refactor `NetworkScreen.cs` and `HeavyGearManager.cs` against the interface.

### Phase 7 — In-Game Editor *(depends on 2)*
- Retire the WinForms `HeavyGearMapEditor` project entirely.
- Salvage map (de)serialization from `Source/HeavyGearMapEditor/Code/Map.cs` and `Code/HexTile.cs`; merge into Core, de-duping against `GameLogic/Map.cs` and `GameLogic/HexTile.cs`.
- Add `EditorScreen : GameScreen` reachable from `MainMenu`.
- Reuse `Camera2D`, `UIRenderer`, and hex picking for placing tiles/units.
- Save/load maps through `FileHelper`.

### Phase 8 — Build & Verify *(depends on all)*
- `dotnet build HeavyGear.sln` succeeds on net8.0 with zero XNA references remaining.
- MGCB content builds (textures, `LineRendering.fx`, map XML) with no pipeline errors.
- Game launches; main menu renders; fonts/sprites/hex tiles draw; mouse + keyboard input works.
- Audio plays through the rewritten `Sound` class.
- Local single-player match runs end to end (deploy → combat → win).
- LAN host/join works over LiteNetLib.
- In-game editor: place tiles/units, save a map, reload it, confirm a faithful round-trip.

---

## Key Files

| File | Change |
|---|---|
| `Source/HeavyGear.sln` | Replace with new SDK-style solution |
| `HeavyGearManager.cs` | Drop `GamerServicesComponent`; refactor `NetworkSession` usage |
| `Graphics/BaseGame.cs` | `Microsoft.Xna.Framework.Game` base; mostly drop-in under MonoGame |
| `Sounds/Sound.cs` | Full XACT → `SoundEffect`/`Song` rewrite |
| `Helpers/FileHelper.cs` | Remove `Guide` storage selector; use local file I/O |
| `GameScreens/NetworkScreen.cs` | Heaviest Net/GamerServices refactor |
| `GameScreens/MainMenu.cs` | Add entry point to new `EditorScreen` |
| `Code/Map.cs` + `Code/HexTile.cs` | Salvage serialization logic, then retire files |
| `Content/HeavyGearContent.contentproj` | Replaced by `Content/Content.mgcb` |
| `HeavyGear/Program.cs` | Strip `#if !XBOX360`; simplify entry point |

---

## Scope Boundaries

**Included:** desktop port, project restructure, audio/storage/networking replacements, in-game editor.

**Excluded:** Xbox 360 target, mobile/console backends, gameplay/balance changes, new art, online (non-LAN) matchmaking.
