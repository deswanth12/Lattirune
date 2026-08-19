# Lattirune
> *Align the Lattice. Awaken the Runes.*

A portrait-mode 2D spatial inventory auto-battler roguelite where directional elemental runes emit energy conduits across a $5 \times 5$ lattice grid, powering weapons and triggering compounding elemental chain reactions.

## Project Specifications

* **Genre:** 2D spatial roguelite / inventory auto-battler
* **Platforms:** Android (Primary) / iOS (Primary) / PC (Steam post-launch)
* **Engine:** Unity 6 LTS (2D URP)
* **Language:** C#
* **Orientation:** Portrait ($1080 \times 1920$ reference canvas)
* **Development Status:** Pre-production / Prototype

## Phase 2: 5-Element Synergy Architecture

The elemental synergy matrix maps directional energy conduits to compatible item classes through data-driven ScriptableObjects:

| Element | Prototype Synergy | Target Category | Mechanical Effect | Visual Aura |
| :--- | :--- | :--- | :--- | :--- |
| 🔥 **Fire** | `fire_sword` (Flamebound Edge) | Weapon | +5 Flat Rune Damage Bonus | Flame Orange `#FFA61A` |
| ❄️ **Ice** | `ice_shield` (Glacial Bastion) | Shield | +4 Defense Armor Bonus | Ice Cyan `#33BFFF` |
| ⚡ **Lightning** | `lightning_weapon` (Storm Surge) | Weapon | +8 Shock Damage Bonus | Electric Yellow `#F2D926` |
| ☠️ **Poison** | `poison_blade` (Venomous Strike) | Weapon | +3 Ticking Poison Bonus | Toxic Green `#26D940` |
| ✨ **Light** | `light_relic` (Radiant Dawn) | Relic | +4 Radiant Resonator Power | Radiant Gold `#FFEB73` |

## Elemental Reaction Matrix (2-Beam Cross-Intersections)

When two distinct directional rune beams cross at the same discrete grid cell, `ElementalIntersectionEngine` evaluates the pair symmetrically and triggers an Elemental Reaction:

| Reaction Name | Reaction ID | Elemental Pair | In-Combat Mechanical Effect |
| :--- | :--- | :--- | :--- |
| **Steam** | `reaction_steam` | Fire Beam + Ice Beam | 25% Enemy Blind / Miss |
| **Plasma** | `reaction_plasma` | Fire Beam + Lightning Beam | 18 Dmg/s Continuous Ray |
| **Toxic Flame** | `reaction_toxic_flame` | Fire Beam + Poison Beam | Detonates Poison Stacks (2×) |
| **Superconductor** | `reaction_superconductor` | Lightning Beam + Ice Beam | -40% Enemy Resistance |
| **Frostbite** | `reaction_frostbite` | Ice Beam + Poison Beam | +50% Poison Tick Damage |

## Prism Rune & Directional Refraction

The Prism Rune (`prism_rune`, Light affinity) acts as an optical beam splitter. When an incoming cardinal conduit strikes a Prism cell:
* **Horizontal Inflow (East/West):** Splits into two perpendicular branches going **North** and **South**.
* **Vertical Inflow (North/South):** Splits into two perpendicular branches going **East** and **West**.
* **Beam Abstraction & Parentage:** `ConduitBeamPath` maintains recursive hierarchy (`ParentBeamId`, `Depth`), preserving source elemental affinity.
* **Full Interoperability:** Refracted split branches participate seamlessly in both Item Synergies and Elemental 2-Beam Cross-Intersections.
* **Cycle Protection:** Recursive traversal tracks visited prism nodes to eliminate infinite loop configurations.

## Crossfire Rune & Multi-Directional Emitters

The Crossfire Rune (`rune_crossfire`, Fire affinity) and Amplifier Node (`rune_omni`) emit energy simultaneously along multiple cardinal vectors from a single origin tile:
* **4-Way Cardinal Emission:** Emits four independent root `ConduitBeamPath` instances (North, South, East, West) across the 5×5 lattice grid.
* **MultiDirectionalEmitter:** Unified emitter abstraction managing directional resolution without mutating static `RuneData` assets.
* **Chained Prism Compatibility:** Each emitted Crossfire beam can independently intersect with Prisms, generating secondary refractions.
* **Zero Self-Intersection:** Beams originating from the same root emitter ID are filtered out from triggering self-reactions.

## Spatial Bag Inventory & Procedural Expansion

The player's backpack storage operates as a discrete 2D spatial grid completely decoupled from the $5 \times 5$ combat lattice:
* **Starting Capacity:** Initial $2 \times 3$ (6 cells) active inventory area inside a $4 \times 4$ frame.
* **Deterministic Expansion Order:** Unlocks adjacent locked cells sequentially (up to 16 maximum cells).
* **Multi-Tile Footprint Support:** Validates multi-tile items ($1 \times 1$, $1 \times 2$, $2 \times 1$, $2 \times 2$) and dynamic $90^\circ$ rotation states.
* **Persistence:** Serializes unlocked cell coordinates and expansion stage to encrypted JSON save data.

## Dungeon Run Progression & Master State Machine

The multi-floor roguelite dungeon progression coordinates encounters, reward drafts, and floor transitions:

```
[RUN START]
    ↓
[Floor 1: Sewer Entry (Rat)] → [Combat] → [Victory] → [Reward Draft] → [Continue]
    ↓
[Floor 2: Armory Cellar (Skeleton)] → [Combat] → [Victory] → [Reward Draft] → [Continue]
    ↓
[Floor 3: Boss Sanctum (Lich Lord)] → [Combat] → [Victory] → [Reward Draft] → [RUN COMPLETE]
```

## Boss Encounter Mechanics & The Lich Lord

The Lich Lord (`boss_lich_lord`) is a 3-phase boss encounter with deterministic dynamic enrage thresholds:

| Phase # | Phase Name | HP% Threshold | Effective Armor | Effective Attack | Attack Interval |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Phase 1** | Frost Warden | $100\% \rightarrow 66\%$ | 10 | 8 | 2.5s |
| **Phase 2** | Soul Harvest | $66\% \rightarrow 33\%$ | 15 (+5) | 12 (+4) | 2.0s (0.8×) |
| **Phase 3** | Necrotic Inversion | $33\% \rightarrow 0\%$ | 20 (+10) | 16 (+8) | 1.6s (0.64×) |

## Combat Status & Effect Framework

Elemental reactions resolve dynamically into runtime combat effects:

```
Rune Beam A x Rune Beam B (or Prism/Crossfire Branches)
        ↓
ElementalIntersectionEngine
        ↓
ElementalReactionSystem (ReactionResult)
        ↓
ReactionEffectResolver
        ↓
CombatEffectSystem (CombatEffectInstance)
        ↓
Combatant (Damage Pipeline & Runtime Modifiers)
```

## Documentation

* [`PLAN.md`](./PLAN.md) — Master project planning and technical architecture blueprint (v1.0.1).
