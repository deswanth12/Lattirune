# Lattirune 1.1: Replayability & Procedural Event Architecture Specification

**Document Version:** 1.1.0  
**Status:** Implemented & Verified  
**Target Platform:** Android (Google Play) & iOS  
**Save Compatibility:** SaveVersion = 1 (100% Backward-Compatible)

---

## 1. System Overview

Lattirune 1.1 builds upon the MVP 1.0 baseline to deliver deep procedural replayability and high-dopamine buildcrafting through four decoupled, event-driven modules:

1. **Run Modifiers (Lattirune.Modifiers)**: Data-driven boons, curses, and hybrid modifiers affecting damage, elemental scaling, greed, and defense.
2. **Combo Engine (Lattirune.Combo)**: Real-time tracking of attack streaks and elemental reaction cascades, granting dynamic battle damage multipliers and end-of-encounter reward surges.
3. **Risk / Reward Choices (Lattirune.Choices)**: Double-edged offerings balancing immediate power against permanent run curses or life tithes.
4. **Procedural Run Events (Lattirune.Events)**: Six canonical procedural encounters appearing between dungeon floors with deterministic weighted RNG and pure transactional resolution.

---

## 2. Architecture & Data Flow

`
                                  ┌──────────────────────┐
                                  │   IRandomSource      │ (Deterministic RNG Seed)
                                  └──────────┬───────────┘
                                             │
┌──────────────────────┐          ┌──────────▼───────────┐
│     RunManager       ├─────────►│   RunEventTrigger    │ (Cadence & Encounter Checks)
└──────────┬───────────┘          └──────────┬───────────┘
           │                                 │
           │ Floor/Encounter Transition       │
           ▼                                 ▼
┌──────────────────────┐          ┌──────────────────────┐
│  CombatSystem        │          │   RunEventPresenter  │ (Pauses RunState -> EventActive)
│  (1v1 Battle Engine) │          └──────────┬───────────┘
└──────────┬───────────┘                     │
           │                                 ▼
           ├────────────► ⚡ ComboTracker ──► [Live Dynamic Multiplier]
           │
           ├────────────► ✨ RunModifierManager ──► [Aggregate Multipliers: DMG / ELEM / GOLD / DEF]
           │
           ▼
┌──────────────────────┐
│ CombatEncounterUI    │ (Live HUD Indicators: Active Modifiers & Combo Multiplier)
└──────────────────────┘
`

---

## 3. Canonical Data Repositories

### 3.1 Run Modifiers
* **mod_sharpened_runes**: Common (+15% Physical & Rune Damage)
* **mod_elemental_surge**: Uncommon (+25% Elemental Reaction & Conduit Damage)
* **mod_midas_touch**: Rare (+50% Gold Multiplier)
* **mod_glass_cannon**: Epic (+50% Damage, +30% Enemy HP)
* **mod_curse_vulnerability**: Curse (-20% Defense)

### 3.2 Procedural Run Events
* **event_ancient_shrine**: Gain mod_sharpened_runes or depart safely.
* **event_blood_altar**: Suffer 20% Max HP sacrifice to forge mod_sharpened_runes.
* **event_cursed_treasury**: Claim 75 Gold, but contract mod_curse_vulnerability.
* **event_elemental_forge**: Pay 30 Gold to ignite mod_elemental_surge.
* **event_ember_well**: Drink to restore 35% Max HP.
* **event_mysterious_chest**: Discover 40 Gold and a rejuvenating tonic (+15% HP).

---

## 4. Verification & Quality Gates

* **Regression Protection**: Zero modifications to existing MVP 1.0 test contracts.
* **Pure Evaluation**: All resolution calculations performed in RunEventResolver with zero side-effects.
* **Combat Isolation**: Events strictly prohibited from firing while CombatState == Fighting.
* **Save Compatibility**: SaveVersion = 1 preserved across all JSON serialization roundtrips.
