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

### Synergy vs. Reaction Distinction:
* **Synergy (Rune + Item):** Direct interaction where a directional conduit illuminates and powers an item in the lattice grid.
* **Elemental Reaction (Rune + Rune):** Crossing interaction where two orthogonal beams intersect at a shared integer coordinate without needing an item placed at the junction.

## Documentation

* [`PLAN.md`](./PLAN.md) — Master project planning and technical architecture blueprint (v1.0.1).
