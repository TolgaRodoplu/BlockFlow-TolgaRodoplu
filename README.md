# BlockFlow

A Color Block Jam-style puzzle game built in Unity. Players drag and drop colored blocks onto matching grinders to clear each level before the timer runs out.

![Platform](https://img.shields.io/badge/Platform-PC-blue) ![Engine](https://img.shields.io/badge/Engine-Unity-black) ![Language](https://img.shields.io/badge/Language-C%23-green)

---

## Gameplay Overview

- The game is viewed from a **top-down orthographic** perspective.
- **Drag and drop** colored blocks around the grid.
- **Feed blocks into grinders** that match their color to consume them.
- A block is consumed only when its shape fits the grinder's entry cells and the path is unobstructed.
- Clear all blocks before the **countdown timer** reaches zero to pass the level.

---

## Features

- **Color Matching** — 10 distinct color slots; grinders only accept blocks of the matching color.
- **Multi-Cell Block Shapes** — 1x1, 2x1, 3x1, 2x2, L, Reverse-L, S, Z, T, Plus, and SmallL shapes.
- **Ice Mechanic** — Blocks can be frozen with an ice counter. Iced blocks cannot be moved until the counter decrements to zero (decrements on each successful grinder consume event).
- **Movement Constraints** — Blocks can be restricted to horizontal or vertical movement only, visualized with axis arrows.
- **5 Hand-Crafted Levels** — Progressively larger grids (7x8 up to 9x10) with increasing complexity.
- **Data-Driven Levels** — All levels are defined in JSON files, making them easy to author and extend.
- **Audio System** — Separate SFX and music mixer channels with toggle buttons in the UI.
- **Physics-Based Drag** — Velocity-driven smooth drag with snap-to-grid on release.

---

## How to Play

1. Open the project in Unity and hit **Play**.
2. **Click and hold** a block to pick it up.
3. **Drag** it toward a grinder of the same color.
4. **Release** to drop it — the block snaps to the nearest valid grid cell.
5. When a block is correctly aligned with a matching grinder, it is automatically consumed.
6. Consume all blocks before time runs out to advance to the next level.

> Blocks with a **snowflake/ice texture** and a number overlay cannot be picked up until they thaw. Each time any block is consumed by a grinder, all ice counters decrement by one.

---

## Project Structure

```
BlockFlow-TolgaRodoplu/
├── Assets/
│   ├── Scripts/               # All C# game logic (18 scripts)
│   ├── Scenes/
│   │   └── SampleScene.unity  # Single main scene
│   ├── Prefabs/               # Block shapes, grinders, walls, axis arrows
│   ├── ScriptableObject/      # Block shape definitions, color palette
│   ├── Resources/
│   │   └── Levels/            # Level_01.json – Level_05.json
│   ├── Materials/             # Shaders and materials
│   ├── SFX/                   # Audio clips
│   ├── Anim/                  # Animation controllers (grinder)
│   └── ExternalAssets/        # Epic Toon FX particle effects
```

---

## Architecture

### Scripts

| Script | Responsibility |
|---|---|
| `GameManager.cs` | Singleton entry point, initializes systems |
| `LevelManager.cs` | Loads levels from JSON, runs countdown timer, broadcasts game events |
| `GridController.cs` | Owns the 2D grid, spawns all objects, handles placement validation and snapping |
| `Grid2D.cs` | Generic `T[,]` grid data structure with world↔grid coordinate conversion |
| `PlacedObject.cs` | Runtime wrapper for any object on the grid (block, wall, grinder) |
| `PlacedObjectTypeSO.cs` | ScriptableObject defining a shape's cells and rotation math for all 4 directions |
| `Block.cs` | Block entity: color, ice state, movement constraints, visual feedback |
| `Grinder.cs` | Trigger-based consumer: validates color, shape, alignment, then destroys block |
| `DragDropController.cs` | Mouse input, raycast picking, physics-based drag, grid snapping on release |
| `UIManager.cs` | Timer display, stage number, SFX/music toggle buttons |
| `AudioManager.cs` | Singleton audio system with AudioMixer integration |
| `StagePassedUI.cs` | Panel shown on level clear |
| `FailedUI.cs` | Panel shown on timeout |
| `GameComplatedUI.cs` | Panel shown when all levels are finished |
| `LevelData.cs` | Serializable data classes for JSON deserialization |
| `ColorPalette.cs` | ScriptableObject mapping `PaletteColor` enum values to `Color` and ice texture |
| `Sound.cs` | Data container (clip, volume, loop, mixer group) |

### Key Design Patterns

- **Singleton** — `GameManager`, `LevelManager`, `GridController`, `DragDropController`, `AudioManager`
- **Event System** — `LevelManager` exposes static C# events (`OnLevelComplete`, `OnLevelFailed`, `OnSecondsUpdated`, etc.) that UI and gameplay systems subscribe to; no tight coupling between systems
- **ScriptableObjects** — Block shapes (`PlacedObjectTypeSO`) and the color palette (`ColorPalette`) are data assets, editable without code changes
- **Factory Method** — `PlacedObject.Create()` instantiates and configures objects from type data
- **Generic Data Structure** — `Grid2D<T>` is a reusable spatial grid that is not tied to any specific game object type

---

## Level System

Levels are stored as JSON files under `Assets/Resources/Levels/` and loaded at runtime via `Resources.Load`.

**JSON structure:**
```json
{
  "width": 7,
  "height": 8,
  "timeLimit": 60,
  "floors": [ { "x": 1, "y": 1 }, ... ],
  "walls": [ { "typeName": "Wall", "x": 0, "y": 0, "direction": "Down" }, ... ],
  "grinders": [ { "typeName": "Grinder2X1", "color": "Color1", ... }, ... ],
  "blocks": [ { "typeName": "Block2X1", "color": "Color2", "iceCounter": 0, "movementConstraint": "None", ... }, ... ]
}
```

| Level | Grid | Time | Notes |
|---|---|---|---|
| Level 01 | 7×8 | 60s | Tutorial — basic shapes, no ice or constraints |
| Level 02 | 7×8 | 60s | More grinders and larger block shapes (2×2, 3×1) |
| Level 03 | 7×9 | 60s | Introduces iced blocks (counter=4) and axis-constrained blocks |
| Level 04 | 5×9 | 60s | Narrower grid, ice only (counter=5), no axis constraints |
| Level 05 | 10×14 | 60s | Largest grid, all 10 colors, heavy ice (up to counter=10), in-grid obstacle blocks |

---

## Development Notes

- **Physics drag** — A `Rigidbody` is added at pickup and removed on drop. Velocity is set each `FixedUpdate` to smoothly follow the mouse, with continuous collision detection to avoid tunneling.
- **Rotation math** — `PlacedObjectTypeSO` pre-computes cell offsets for all four rotation states (Down/Left/Up/Right) and provides origin offset corrections so shapes pivot visually correctly on the grid.
- **Ice decrement** — Each iced `Block` subscribes to `GridController.OnBlockExit` directly in its own `SetMat()` method. When any block is consumed, every subscribed iced block calls `UpdateIceCounter()` on itself: the counter decrements, the ice sound plays, and the material swaps back to the block's color when the counter reaches zero. `LevelManager` uses the same event independently to track only the remaining block count.
- **Grinder validation** — Before accepting a block, `Grinder.cs` checks: (1) color match, (2) block size fits entry cell count, (3) block origin aligns with grinder opening, (4) no wall or obstacle occupies the path between block and grinder.

---

## Author

**Tolga Rodoplu** — [rodoplutolga@gmail.com](mailto:rodoplutolga@gmail.com)
