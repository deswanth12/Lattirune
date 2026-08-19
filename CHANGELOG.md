# Changelog

All notable changes to the **Lattirune** project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-08-19 (MVP 1.0 Release Candidate)

### Core Gameplay
- **5×5 Spatial Lattice Grid:** 17 active cells, 8 locked perimeter cells, discrete spatial indexing, and multi-tile placement validation.
- **Directional Rune Conduit Engine:** 10 canonical runes casting cardinal energy conduits across the grid.
- **Optical Prism Refraction:** Dynamic beam splitting for horizontal and vertical energy conduits with recursive loop protection.
- **Crossfire Multi-Directional Emitters:** 4-way cardinal emission and omni-directional amplification nodes.
- **5-Element Synergy Architecture:** Flamebound Edge, Glacial Bastion, Storm Surge, Venomous Strike, and Radiant Dawn.
- **5 Master Item Combinations:** Flaming Blade, Venom Shiv, Thunder Bow, Molten Wall, and Shatterstrike overriding generic category rules.
- **Elemental Reaction Matrix:** Steam (Blind/Miss), Plasma (Continuous Ray), Toxic Flame (Detonation), Superconductor (Armor Shred), and Frostbite (Poison Amp) with symmetric pair resolution ($A + B == B + A$).
- **Chain Reaction Engine:** Queue-based cascading trigger resolution with $0.02\text{s}$ propagation interval and recursion depth cap $N \le 4$.

### Content
- **10-Floor Cursed Sewers Topology:** 10 deterministic procedural floors including Sewer Entry, Drain Basin, Slime Cavern, Armory Gate, Treasure Vault, Bone Crypt, Spider Nest, and Boss Sanctum.
- **20 Canonical Items:** Complete data-driven catalogue across Weapons, Shields, Armor, Relics, Consumables, and Cursed items.
- **10 Canonical Runes:** Ember, Frost, Spark, Venom, Crossfire, Prism, Amplifier Node, Iron, Vampire, and Haste.
- **6-Enemy Bestiary:** Sewer Rat (Swarm), Goblin Thief (Gold Steal), Armored Skeleton (Reflect), Venomous Spider (Poison), Acid Slime (Slot Disable), and Necromancer (Summons).
- **The Lich Lord 3-Phase Boss:** 750 HP, 10 Armor, 8 Attack, 2.5s base interval with Soul Harvest ($66\%$) and Necrotic Inversion ($33\%$) dynamic enrage states.

### Progression
- **3-Card Reward Draft:** Deterministic 3-choice draft respecting unlocked blueprint pools with duplicate protection and selection locking.
- **Procedural Spatial Bag Inventory:** $4 \times 4$ frame, initial 6 cells ($2 \times 3$), expanding sequentially to 16 maximum cells.
- **In-Run Economy:** Normal mob gold drops ($6-12$), elite drops ($20-35$), and boss embers ($80-120$).
- **Merchant Stalls (Floor 4 & Floor 9):** Item, Rune, and Bag Expansion purchasing with atomic balance validation.
- **Floor 8 Campfire Rest Site:** Mutually exclusive choice between restoring $40\%$ Max HP or reforging 1 Rune (+2 runtime power).
- **Persistent Meta-Progression:** Campfire Meta-Hub tracking lifetime runs, boss clears, and persistent Dungeon Embers.
- **Blueprint Forge:** 12 canonical blueprints with prerequisite unlock requirements and starting bonus integrations.

### Mobile
- **Responsive Portrait Orientation:** $1080 \times 1920$ reference canvas with notch safe-area handling.
- **Touch Targets:** All interactive buttons and cards conform to $\ge 52\text{ dp}$ minimum touch targets.
- **Safe Android Back Button Routing:** Context-aware back navigation with combat lock preventing accidental battle abandonment.
- **Simulation Speed Multipliers:** Toggleable $1.0\times, 2.0\times, 3.0\times$ battle speeds.
- **Manual Emergency Potion:** Single-tap emergency consumable drink during active combat.
- **Audio & Haptic Feedback:** Complete SFX and tactile vibration support across UI clicks, placements, combat, and rewards.
- **Settings Screen:** Master volume, SFX volume, Mute toggle, and Haptics toggles persisting to storage.

### Persistence
- **Encrypted Local JSON Storage:** AES-256 encrypted payload serialization.
- **SaveVersion = 1:** Backward-compatible schema preserving run and meta-progression data.
- **Atomic Disk Writes & Backup Recovery:** Crash-safe file swapping using temporary and backup save files.

### Verification
- **Full EditMode Regression:** 389+ / 389+ tests passing with 100% success rate.
- **Compilation & Console Errors:** 0 compilation errors, 0 runtime console errors.
- **Release Candidate Artifact:** `Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk`.
- **Production Versioned Artifact:** `Builds/Android/Lattirune-1.0.0.apk`.
- **Physical Android Device Testing:** `NOT TESTED` (Hardware lab test pending).
