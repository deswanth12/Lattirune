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

### Architecture Principles:
* **Static Rules vs. Runtime State:** `SynergyDefinitionSO` defines static rule criteria; runtime state resides strictly on `ItemInstance` and `SynergySystem`.
* **Zero ScriptableObject Mutation:** Static data assets remain 100% immutable during gameplay and serialization.
* **Deterministic Recalculation:** Active synergies are dynamically recomputed from placed items and rune rays post-load.

## Documentation

* [`PLAN.md`](./PLAN.md) — Master project planning and technical architecture blueprint (v1.0.1).
