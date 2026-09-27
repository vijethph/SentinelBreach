# SentinelBreach

A cyberpunk corridor runner where you infiltrate a corporate AI facility, hack terminals on the run, and outpace an escalating autonomous security system.

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#features">Features</a></li>
    <li><a href="#getting-started">Getting Started</a></li>
    <li><a href="#gameplay-systems">Gameplay Systems</a></li>
    <li><a href="#controls">Controls</a></li>
    <li><a href="#building-and-deployment">Building and Deployment</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

---

## About The Project

**SentinelBreach** is a 3D endless runner set in a futuristic corporate facility. You play as CIPHER, a data courier with neural-hacking implants, infiltrating AXIOM Corp's server vaults beneath the city. SENTINEL, an autonomous security AI, escalates its defenses as you descend deeper.

The game combines fast-paced corridor navigation with strategic gadget usage, persistent progression, and dynamic difficulty scaling. Built with Unity 2022.3, it features custom-written physics, procedural level generation, infinite streaming, event-driven architecture, and support for both PC and Android.

### Key Features

- Endless procedurally-generated corridor with 30-unit segments
- Three-lane navigation with smooth lane-switching
- Custom gravity and jump physics
- Five obstacle types: laser grids, turrets, drones, terminals, and exfiltration checkpoints
- Four collectibles: Data Shards, Shield Cells, Surge Tokens, Ghost Chips
- Three gadgets with cooldown management: Dash, EMP, Time-Slow
- Quest pool of 8 with 3 random quests per run
- Persistent progression through a skill tree (5 nodes × 3 levels), character levels 1–10, and best-distance tracking
- Android support with accelerometer and touch controls alongside PC keyboard
- Event-driven UI with HUD, pause menu, and game-over stats

## Features

### Gameplay

- **Procedural Generation** - Infinite corridor built from reusable 30-unit segments with weighted spawn pools
- **Multi-Lane Movement** - Three lanes with smooth horizontal transitions; jump and slide for obstacle avoidance
- **Hazard Variety** - Laser grids (timed barriers), turrets (tracking fire), drones (patrol collision), terminals (quest triggers), exfil checkpoints
- **Collectibles System** - Data Shards (currency), Shield Cells (invulnerability), Surge Tokens (cooldown refill), Ghost Chips (obstacle disabler)
- **Gadget Mechanics** - Dash (forward boost), EMP (stun enemies), Time-Slow (slowdown effect) with individual cooldowns
- **Quest System** - Pool of 8 quests, 3 random per run; event-driven completion tracking
- **Progressive Difficulty** - SENTINEL tier increases every checkpoint; obstacle spawn rates and complexity scale with distance
- **Dynamic Camera** - Cinemachine-driven follow camera with shake feedback for hits
- **Audio System** - Background music with dynamic pitch ramping, SFX for all actions (jump, hit, shard, gadget)

### Technical Highlights

- **Custom Physics** - Gravity, jump, and knockback implemented with manual velocity math
- **Event-Driven Architecture** - Decoupled systems via Unity events
- **Object Pooling** - Efficient segment spawning/despawning for seamless infinite levels
- **JSON-Driven Configuration** - Segment weights and spawn overrides via StreamingAssets (moddable without recompiling)
- **Persistent Progression** - PlayerPrefs-backed skill tree and character level system
- **Multi-Platform Input** - Unified input handling for PC keyboard and Android accelerometer/touch
- **Collision Management** - CharacterController for movement, raycasts for gadget fire, triggers for collectibles

## Getting Started

### Prerequisites

- **Unity Editor 2022.3.62f3** (LTS) with:
  - Core modules (UI, Physics)
  - Android Build Support (if targeting Android)
  - Visual Studio or Rider (C# IDE)
- **Git** for version control
- **Android SDK 26+**, Android NDK, and JDK (for Android builds)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/vijethph/SentinelBreach.git
   cd SentinelBreach
   ```

2. **Open in Unity Hub**
   - Launch Unity Hub and select **Open**
   - Choose the SentinelBreach folder
   - Unity loads with version 2022.3.62f3

3. **Wait for import**
   - Assets, scripts, and dependencies import automatically
   - Check Console (Window → General → Console) for errors
   - Initial load may take a few minutes

4. **Open the main scene**
   - In Project window, navigate to `Assets/Scenes/Game.unity`
   - Double-click to load

5. **Play**
   - Press Play or Ctrl+P (Win) / Cmd+P (Mac)
   - Use arrow keys or WASD to move through the corridor

### Building for Android

1. **File → Build Settings**
2. Switch platform to **Android**
3. Add open scene to build list
4. Click **Player Settings** and configure:
   - Package Name: `com.yourname.sentinelbreach`
   - Minimum API: Android 8.0 (API 26)
   - Target API: Automatic (Highest Installed)
5. **Build and Run** or create an APK

## Gameplay Systems

### Movement & Navigation

**CharacterController-based runner** with auto-forward velocity (~8 m/s). Three lanes positioned at X = -2.5, 0, +2.5 with smooth 0.1-second transitions. Jump applies upward velocity (~12 m/s) with custom gravity accumulation (-25 m/s²). Slide reduces capsule height, applies forward boost, and locks lane changes for ~0.8 seconds.

**Knockback Physics** - On damage, enemies apply a directional impulse to knockbackVelocity. Decay follows exponential lerp at 5 m/s decay rate, creating natural momentum loss over time.

### Infinite Level Streaming

SegmentSpawner maintains a queue of active segments. It spawns new ones when the player is about 150 units ahead and destroys ones 60+ units behind. Each segment is 30 units long. Spawn weighting is configurable through `Assets/StreamingAssets/segment_config.json` for runtime tweaks or ScriptableObject assets as a fallback. Six segment types exist: Open (empty), Laser, Turret, Drone, Terminal, and Exfil.

### Obstacles

| Type             | Behavior                                                     | Interaction                        |
| ---------------- | ------------------------------------------------------------ | ---------------------------------- |
| **Laser Grid**   | Toggles on/off with timed windows; instant damage on contact | TriggerStay collision              |
| Turret           | Tracks player with raycast, fires projectiles                | Raycast hit + projectile collision |
| Drone            | Patrols segment with sine-wave flight                        | Overlap damage                     |
| Terminal         | Trigger zone for quests and progression                      | TriggerEnter                       |
| Exfil Checkpoint | 500m milestone, escalates difficulty                         | TriggerEnter                       |

Obstacles support disable/pause commands through LaserToggle.ForceOff, TurretController.Disable, and DronePatrol.Disable for gadget effects.

### Collectibles

Data Shard adds to your run total and persistent shard pool with UI feedback. Shield Cell activates temporary invulnerability and shows a visual overlay. Surge Token refills all gadget cooldowns over about 3 seconds. Ghost Chip disables obstacles for 5 seconds with a full-screen tint overlay. All pickups trigger quest updates.

### Gadgets

| Gadget        | Action            | Cooldown | Effect                             |
| ------------- | ----------------- | -------- | ---------------------------------- |
| Dash (Q)      | Directional boost | 5s       | Forward impulse, knockback += 18   |
| EMP (E)       | Stun all enemies  | 8s       | Turrets and drones disabled for 4s |
| Time-Slow (R) | Global slowdown   | 12s      | Time.timeScale = 0.3 for 2s        |

GadgetData ScriptableObjects set cooldown, duration, and force. GadgetManager handles activation, cooldown tracking, and UI lockouts.

### Progression Systems

The Skill Tree has 5 nodes (Neural Speed, Nano Shield, Hack Range, Gadget Efficiency, Knockback Resistance), each with 3 levels. Level cost scales with tier. Perks apply permanent multipliers: Neural Speed grants +5% run speed per level, Nano Shield adds +25% max health, Hack Range reduces gadget cooldowns, Gadget Efficiency lowers time-slow cooldown, and Knockback Resistance cuts knockback force.

CIPHER levels from 1–10 through accumulated XP. Each run earns XP based on distance (0.1 per meter) and quest completion (+50 XP per quest). Level ups grant permanent stat boosts. Best distance is tracked via PlayerPrefs key `BestDistance`.

Quests draw from a pool of 8 assets (CollectShards, DodgeHits, UseGadgets, and more), with 3 randomly selected per run. Event callbacks update quest progress.

## Controls

### PC (Keyboard + Input System Action Map)

| Action               | Key          |
| -------------------- | ------------ |
| Move Left            | A or ← Arrow |
| Move Right           | D or → Arrow |
| Jump                 | Space        |
| Slide                | Left Ctrl    |
| Gadget 1 (Dash)      | Q            |
| Gadget 2 (EMP)       | E            |
| Gadget 3 (Time-Slow) | R            |
| Pause                | ESC or P     |

### Android (Accelerometer + Touch Fallback)

| Action     | Input             |
| ---------- | ----------------- |
| Lane Left  | Tilt device left  |
| Lane Right | Tilt device right |
| Jump       | Swipe up          |
| Slide      | Swipe down        |
| Gadgets    | On-screen buttons |
| Pause      | Pause button      |

## Building and Deployment

### PC Standalone

1. **File → Build Settings**
2. Switch to **PC, Mac & Linux Standalone**
3. Configure **Player Settings** as needed
4. **Build** to generate executable

### Android APK/AAB

1. **File → Build Settings → Android**
2. Add `Assets/Scenes/Game.unity` to build list
3. **Player Settings:**
   - Package Name: `io.github.vijethph.sentinelbreach`
   - Minimum API: Android 8.0 (API 26)
   - Graphics: OpenGL ES 3.0
4. **Build and Run** to device or create APK

### Development Tips

- Use `Assets/StreamingAssets/segment_config.json` to tweak spawn weights without recompiling
- Gadget cooldowns and forces are configurable via ScriptableObject assets
- Add new obstacle types by creating SegmentConfig + prefab, then assigning to SegmentSpawner
- Quests are data-driven; create new QuestData assets to expand quest pool
- Skill Tree values are tuned via SkillData assets

## Contributing

Contributions welcome! To contribute:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/YourFeatureName`
3. Make changes
4. Commit: `git commit -m 'Add: Brief description'`
5. Push: `git push origin feature/YourFeatureName`
6. Open a pull request

**Code Guidelines:**
Follow C# naming conventions: PascalCase for classes and public methods, camelCase for private. Use meaningful variable names and keep methods focused on a single responsibility. Comment only when logic is complex or non-obvious. Don't break existing scene wiring or manager dependencies. Test on both PC and Android if your changes touch input or platform-specific code.

## License

Licensed under Apache License 2.0. See LICENSE file for details.

## Contact

- Game built by [Vijeth](https://github.com/vijethph)
- Repository: <https://github.com/vijethph/SentinelBreach>
