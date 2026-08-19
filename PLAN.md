# 💎 LATTIRUNE: Master Project Plan (PLAN.md)
> *“Align the Lattice. Awaken the Runes.”*

---

## Document Control & Metadata
* **Project Name:** Lattirune
* **Document Version:** 1.0.1 (Master Technical Blueprint)
* **Author:** Lead Game Architect & Senior Systems Designer
* **Target Platforms:** Android (Google Play), iOS (Apple App Store), PC (Steam - Post-Launch)
* **Engine & Toolchain:** Unity 6 LTS (2D URP), C#, Target Android API 34+ (Android 14/15), iOS 17+, macOS / Windows Dev Environment
* **Status:** Approved for Phase 0 / Phase 1 Execution

### Change Notes (Version 1.0.1)
* **Grid & Resolution Notation:** Cleaned all instances of duplicate dimension text to strictly $5 \times 5$ (2D coordinate array $\text{Grid}[x, y]$ with $x \in [0, 4], y \in [0, 4]$) and $1080 \times 1920$ portrait reference resolution.
* **Damage Calculation Pipeline:** Formally defined $\text{FinalDamage} = \max(\text{MinimumDamage}, ((\text{Base} + \text{Rune}) \times \text{Crit} \times \text{Mod}) - \text{Armor})$ with step-by-step evaluation order and explicit minimum damage floor of $1$.
* **Scope Delineation:** Formally separated **Core Prototype** (Phase 1: tiny 5-item / 2-rune mechanic test), **Vertical Slice** (Phase 2: 3 floors + 1 boss polished proof), and **MVP 1.0** (Phase 3+: full 10-floor / 20-item release).
* **WebGL & Mobile Testing:** Clarified WebGL export as an optional rapid playtesting target while making physical Android device profiling mandatory.
* **Performance & Quality Targets:** Converted absolute claims into measurable development target budgets validated through device profiling.
* **Hardware Target Tiers:** Defined Baseline Android (Snapdragon 680 / 4GB RAM class), Low-End Baseline, Mid-Range Dev, and High-End Reference tiers.
* **Formatting & Consistency Audit:** Cleaned item footprint dimensions ($1 \times 1, 1 \times 2, 2 \times 1, 2 \times 2, 1 \times 3, \text{L-Shape}$) and completed cross-section coherence check.

---

# Table of Contents
1. [Project Overview](#1-project-overview)
2. [Core Game Loop](#2-core-game-loop)
3. [Core Gameplay Mechanic: The Lattice Conduit Engine](#3-core-gameplay-mechanic-the-lattice-conduit-engine)
4. [Inventory & Grid Architecture](#4-inventory--grid-architecture)
5. [Rune System & Directional Conduits](#5-rune-system--directional-conduits)
6. [Item Taxonomy & Catalogue](#6-item-taxonomy--catalogue)
7. [Synergy System & Adjacency Matrix](#7-synergy-system--adjacency-matrix)
8. [Chain Reaction Engine & Loop Prevention](#8-chain-reaction-engine--loop-prevention)
9. [Combat System & Simulation Engine](#9-combat-system--simulation-engine)
10. [Enemy & Boss Architecture](#10-enemy--boss-architecture)
11. [Dungeon Progression & Room Topology](#11-dungeon-progression--room-topology)
12. [Player Progression: Run vs. Meta](#12-player-progression-run-vs-meta)
13. [Game Economy & Data-Driven Tuning](#13-game-economy--data-driven-tuning)
14. [Game State Machine](#14-game-state-machine)
15. [UI/UX & Screen Architecture](#15-uiux--screen-architecture)
16. [Mobile Ergonomics & Touch Controls](#16-mobile-ergonomics--touch-controls)
17. [Visual Direction & Art Style Guide](#17-visual-direction--art-style-guide)
18. [Audio Design Matrix](#18-audio-design-matrix)
19. [Unity Software Architecture](#19-unity-software-architecture)
20. [Core C# Systems & Class Responsibilities](#20-core-c-systems--class-responsibilities)
21. [Data Architecture & ScriptableObjects](#21-data-architecture--scriptableobjects)
22. [Save System & State Serialization](#22-save-system--state-serialization)
23. [Performance Budget & Mobile Optimization](#23-performance-budget--mobile-optimization)
24. [Device Compatibility & Hardware Tiers](#24-device-compatibility--hardware-tiers)
25. [Offline-First Architecture](#25-offline-first-architecture)
26. [Analytics & Telemetry Schema](#26-analytics--telemetry-schema)
27. [Monetization Integration Strategy](#27-monetization-integration-strategy)
28. [Platform Compliance & Privacy Requirements](#28-platform-compliance--privacy-requirements)
29. [Testing & Quality Assurance Strategy](#29-testing--quality-assurance-strategy)
30. [Development Phases & Milestones](#30-development-phases--milestones)
31. [First 14 Days Implementation Sprint](#31-first-14-days-implementation-sprint)
32. [Scope Hierarchy Matrix (Prototype vs Vertical Slice vs MVP)](#32-scope-hierarchy-matrix-prototype-vs-vertical-slice-vs-mvp)
33. [Vertical Slice Specification](#33-vertical-slice-specification)
34. [Quality Gates & Go/No-Go Criteria](#34-quality-gates--gono-go-criteria)
35. [Risk Register & Mitigation Matrix](#35-risk-register--mitigation-matrix)
36. [AI-Assisted Development Workflow](#36-ai-assisted-development-workflow)
37. [Git & Version Control Strategy](#37-git--version-control-strategy)
38. [Documentation Suite](#38-documentation-suite)
39. [Portfolio Engineering Strategy](#39-portfolio-engineering-strategy)
40. [Project-Wide Definition of Done](#40-definition-of-done)
41. [Master Development Roadmap](#41-master-development-roadmap)
42. [First Actionable Task](#42-first-actionable-task)

---

# 1. Project Overview

### 1.1 Summary & Core Identity
**Lattirune** is an offline-first, portrait-mode 2D spatial roguelite for mobile and PC. Players enter a procedurally generated dungeon, manage a constrained $5 \times 5$ grid inventory (the *Lattice*), and place directional elemental *Runes* that emit physical energy beams. These conduits power adjacent weapons, armor, and relics, triggering compounding elemental chain reactions during fast, semi-automated battles.

### 1.2 Target Audience & Platform Strategy
* **Primary Demographic:** Mobile roguelite and tactical puzzle enthusiasts (Ages 16–40) who appreciate deep build-crafting (*Slice & Dice*, *Balatro*, *Peglin*, *Backpack Hero*).
* **Platforms:** 
  1. Primary: Android (Google Play) & iOS (Apple App Store).
  2. Secondary: PC (Steam / itch.io) via desktop input mapping.
* **Orientation:** Strict Portrait ($1080 \times 1920$ reference canvas, single-thumb ergonomic reach).

### 1.3 Core & Player Fantasy
* **Core Fantasy:** The *Arcane Circuit Engineer*—you are an alchemist-artificer delving into forgotten dungeon vaults, turning an ancient grid receptacle into a humming, lethal elemental power generator.
* **Player Experience:** The visceral satisfaction of turning chaotic loot drops into an organized, geometrically perfect laser matrix, followed by the dopamine rush of watching a boss melt in seconds from automated synergies.

### 1.4 Development Philosophy & Scope Hierarchy
To ensure successful solo development, Lattirune progresses through three strictly defined development stages:
1. **Core Prototype (Phase 1):** Intentionally tiny (5 items, 2 runes, 1 enemy, 1 encounter). Answers the sole question: *"Is placing directional runes to power weapons on a grid fun?"*
2. **Vertical Slice (Phase 2):** 3 floors, 1 boss, full audio/visual polish. Proves the complete loop and visual juice.
3. **MVP 1.0 (Phase 3+):** 10 floors, 20 items, 10 runes, 6 enemies, 1 boss, save system, full commercial release.

---

# 2. Core Game Loop

```
┌────────────────────────────────────────────────────────────────────────┐
│                        THE LATTIRUNE GAME LOOP                         │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│    1. EXPLORE    │────►│    2. LOOT       │────►│    3. ARRANGE    │
│ • Choose path on │     │ • Open chest /   │     │ • Drag, drop &   │
│   dungeon floor  │     │   merchant draft │     │   rotate items   │
└──────────────────┘     └──────────────────┘     └─────────┬────────┘
                                                            │
                                                            ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   6. REINVEST    │◄────│    5. RESOLVE    │◄────│   4. CONNECT     │
│ • Meta-hub forge │     │ • Auto-combat    │     │ • Runes project  │
│ • Unlock recipes │     │ • Floaty numbers │     │   active beams   │
└─────────┬────────┘     └──────────────────┘     └──────────────────┘
          │
          └─────────────────────────► [ REPEAT WITH SCALING FLOORS ]
```

### Step-by-Step Breakdown:
1. **Explore (Dungeon Path):** Player navigates a 10-floor branch (Normal Battle, Elite Battle, Mystery Shrine, Merchant, Campfire Rest, Boss).
2. **Loot (Acquisition):** Defeating enemies or opening chests presents a 3-item draft (Weapons, Armor, Runes, Consumables, Relics).
3. **Arrange (Spatial Puzzle):** Player drags items into their $5 \times 5$ Lattice, managing spatial footprints ($1 \times 1, 1 \times 2, 2 \times 1, 2 \times 2, \text{L-Shape}$).
4. **Connect (Conduit Activation):** Placing a directional Rune immediately casts a raycast conduit through the grid, visually illuminating powered items with glowing particle lines.
5. **Resolve (Combat Payoff):** Player taps `BATTLE`. Weapons execute automated attacks based on cooldown timers. Elemental reactions (Steam, Plasma, Toxic Flame) detonate.
6. **Reinvest (Meta Progression):** Dying or defeating the Floor 10 Boss converts collected *Dungeon Embers* into permanent Blueprint unlocks at the Campfire Hub.

---

# 3. Core Gameplay Mechanic: The Lattice Conduit Engine

### 3.1 The Lattice Grid Geometry
The inventory is a discrete $5 \times 5$ two-dimensional coordinate array:
$$\text{Grid}[x, y] \quad \text{where } x \in [0, 4], \, y \in [0, 4]$$
* **Initial State:** 17 active tiles unlocked in a diamond-square pattern; 8 perimeter corner tiles locked.
* **Expansion:** Gaining a character level in a run allows the player to pick 1 adjacent locked tile to unlock permanently for that run.

```
       [Col 0]      [Col 1]      [Col 2]      [Col 3]      [Col 4]
[Row 0] [LOCKED]   [ACTIVE]     [ACTIVE]     [ACTIVE]     [LOCKED]
[Row 1] [ACTIVE]   [ACTIVE]     [ACTIVE]     [ACTIVE]     [ACTIVE]
[Row 2] [ACTIVE]   [ACTIVE]     [ACTIVE]     [ACTIVE]     [ACTIVE]
[Row 3] [ACTIVE]   [ACTIVE]     [ACTIVE]     [ACTIVE]     [ACTIVE]
[Row 4] [LOCKED]   [ACTIVE]     [ACTIVE]     [ACTIVE]     [LOCKED]
```

### 3.2 Directional Raycasting Rules
1. **Emitter Runes:** Each Rune has an assigned emission vector $\vec{D} \in \{(0,1), (0,-1), (1,0), (-1,0), \text{Cross}\}$.
2. **Ray Traversal:** Starting at Rune coordinate $(x_0, y_0)$, the ray advances iteratively:
   $$(x_{k+1}, y_{k+1}) = (x_k + D_x, \, y_k + D_y)$$
3. **Pass-Through vs. Blocking:**
   * *Weapons & Relics:* Absorb the elemental power and let the beam pass through to the next cell.
   * *Heavy Armor & Tower Shields:* Absorb the beam and act as an **insulator** (stopping the beam from propagating further unless using a Prism Rune).
   * *Empty Cells:* Beams travel cleanly across empty cells without decay.
4. **Visual Ray Feedback:** Every active connection instantiates a high-efficiency Unity `LineRenderer` or segmented procedural sprite mesh with pulsing animated shader UVs.

---

# 4. Inventory & Grid Architecture

### 4.1 Item Footprint Specifications

```
┌──────────┐ ┌──────────┬──────────┐ ┌──────────┐
│ 1x1 RUNE │ │   1x2 SHORT SWORD    │ │ 2x1 BOW  │
│  [ R ]   │ │  [Blade] │ [Hilt]   │ │ [=][=]   │
└──────────┘ └──────────┴──────────┘ └──────────┘
┌──────────┬──────────┐              ┌──────────┬──────────┐
│   2x2 SHIELD        │              │   L-SHAPED AXE   │
│  [Top-L] │ [Top-R]  │              │  [Axe]   │ [Head]   │
├──────────┼──────────┤              ├──────────┴──────────┤
│  [Bot-L] │ [Bot-R]  │              │  [Shaft] │ (Empty)  │
└──────────┴──────────┘              └─────────────────────┘
```

### 4.2 Placement & Collision Rules
* **Strict Footprint Validation:** An item can only be dropped if all tiles in its footprint $(x + dx, y + dy)$ are **Active** and **Unoccupied**.
* **Invalid Drop Handling:** If dropped over an occupied or locked tile, the item smoothly tweens back to its previous valid grid position or the temporary *Loot Staging Tray* using an elastic easing curve ($0.15\text{s}$).
* **Single-Tap Rotation:** Tapping an item while dragging rotates it 90° clockwise. The footprint matrix updates dynamically:
  $$\text{Rotate}_{90^\circ}\begin{pmatrix} w \\ h \end{pmatrix} = \begin{pmatrix} h \\ w \end{pmatrix}$$
* **Item Swapping:** Dropping a $1 \times 1$ item over another $1 \times 1$ item swaps their positions instantly.

---

# 5. Rune System & Directional Conduits

### 5.1 Rune Catalogue (Prototype vs MVP)
* **Core Prototype (2 Runes):** Ember Rune (Fire $\rightarrow$) and Frost Rune (Ice $\downarrow$).
* **MVP 1.0 (10 Core Runes):**

```
┌────┬─────────────────┬──────────┬───────────┬─────────────────────────────────────────────────┬──────────────────────┬───────────────────────────────┐
│ #  │ Rune Name       │ Element  │ Direction │ Mechanical In-Combat Effect                     │ Compatible Items     │ Visual & Audio Feedback       │
├────┼─────────────────┼──────────┼───────────┼─────────────────────────────────────────────────┼──────────────────────┼───────────────────────────────┤
│ 1  │ **Ember Rune**  │ Fire     │ East (→)  │ Adds +6 Fire Dmg; applies Burn (3 dmg/s for 4s).│ Blades, Bows, Wands  │ Flaming red laser + sizzle SFX│
│ 2  │ **Frost Rune**  │ Ice      │ South (↓) │ Adds +4 Ice Dmg; reduces enemy speed by 15%.    │ Shields, Daggers     │ Crystalline blue frost ray    │
│ 3  │ **Spark Rune**  │ Lightning│ North (↑) │ Adds +8 Shock Dmg; 25% chance to arc to adds.   │ Fast weapons (<1.5s) │ Electric purple bolt + zap SFX│
│ 4  │ **Venom Rune**  │ Poison   │ West (←)  │ Applies 2 Poison stacks/s (ignores shields).    │ Daggers, Bows        │ Toxic green bubbling beam     │
│ 5  │ **Crossfire**   │ Fire     │ Cross (+) │ Adds +3 Fire Dmg in all 4 cardinal directions.  │ Multi-weapon setups  │ 4-way pulsing orange cross    │
│ 6  │ **Prism Rune**  │ Light    │ Split (⤢) │ Splits 1 incoming beam into 2 diagonal rays.    │ Runes only           │ Refracting rainbow beam       │
│ 7  │ **Amplifier**   │ Force    │ Omni (o)  │ Doubles the power of all 4 adjacent runes.      │ Runes only           │ Resonating golden pulse ring  │
│ 8  │ **Iron Rune**   │ Earth    │ South (↓) │ Grants +15 Shield at battle start.              │ Armor, Shields       │ Granite stone beam + thud SFX │
│ 9  │ **Vampire Rune**│ Shadow   │ North (↑) │ Connected weapon heals player for 12% damage.   │ Bladed weapons       │ Dark crimson blood tendril    │
│ 10 │ **Haste Rune**  │ Wind     │ East (→)  │ +25% Attack Speed to weapons in line.           │ Heavy weapons (>2.5s)│ Swirling cyan vortex beam     │
└────┴─────────────────┴──────────┴───────────┴─────────────────────────────────────────────────┴──────────────────────┴───────────────────────────────┘
```

---

# 6. Item Taxonomy & Catalogue

```
┌────────────────────────────────────────────────────────────────────────┐
│                         ITEM TAXONOMY (5 CLASSES)                      │
├──────────────┬──────────────┬──────────────┬────────────┬──────────────┤
│ 1. WEAPONS   │ 2. SHIELDS   │ 3. ARMOR     │ 4. RELICS  │ 5. CONSUMABLES│
│ • Deal dmg   │ • Block hits │ • Boost HP   │ • Passives │ • Single-use │
│ • Have speed │ • Add Thorns │ • Reduce dmg │ • Modifiers│ • Auto-drink │
└──────────────┴──────────────┴──────────────┴────────────┴──────────────┘
```

### 6.1 Item Catalogue Breakdown
* **Core Prototype Items (5 Items):** Rusty Dagger ($1 \times 1$), Iron Broadsword ($1 \times 2$), Wooden Buckler ($1 \times 1$), Whetstone ($1 \times 1$), Health Potion ($1 \times 1$).
* **MVP 1.0 Items (20 Items):**
  1. **Rusty Dagger ($1 \times 1$):** 4 Dmg | 0.8s cooldown. *Synergy:* +2 Dmg if placed in a corner tile.
  2. **Iron Broadsword ($1 \times 2$):** 10 Dmg | 2.0s cooldown. *Synergy:* +3 Dmg for each adjacent weapon.
  3. **Shortbow ($2 \times 1$):** 6 Dmg | 1.4s cooldown. *Synergy:* Attacks pierce 5 armor.
  4. **Apprentice Wand ($1 \times 2$):** 7 Dmg | 1.8s cooldown. *Synergy:* +50% elemental rune damage.
  5. **Battleaxe ($\text{L-Shape}$):** 18 Dmg | 3.0s cooldown. *Synergy:* Deals $1.5\times$ damage if shield is 0.
  6. **Phalanx Spear ($1 \times 3$):** 12 Dmg | 1.8s cooldown. *Synergy:* +4 Dmg for empty tiles behind shaft.
  7. **Wooden Buckler ($1 \times 1$):** 8 Shield at start of battle.
  8. **Iron Tower Shield ($2 \times 2$):** 25 Shield at start; +5 Shield every 3 seconds.
  9. **Spiked Buckler ($1 \times 2$):** 12 Shield; reflects 4 Thorns damage when struck.
  10. **Leather Tunic ($2 \times 2$):** +25 Max HP; +10 HP per adjacent potion.
  11. **Chainmail Coat ($2 \times 2$):** +15 Max HP; reduces all incoming damage by 2 flat.
  12. **Whetstone ($1 \times 1$ Relic):** All adjacent bladed weapons gain +3 flat Base Damage.
  13. **Ruby Ring ($1 \times 1$ Relic):** Adjacent Fire Runes gain +25% burn duration.
  14. **Sapphire Ring ($1 \times 1$ Relic):** Adjacent Ice Runes gain +25% slow potency.
  15. **Lucky Clover ($1 \times 1$ Relic):** +10% Critical Strike Chance.
  16. **Health Potion ($1 \times 1$ Consumable):** Auto-drinks at $< 30\%$ HP; restores 35 HP.
  17. **Stamina Flask ($1 \times 1$ Consumable):** Auto-drinks at battle start; +40% speed for 4 seconds.
  18. **Poison Vial ($1 \times 1$ Consumable):** Breaks on first hit taken; inflicts 15 Poison on attacker.
  19. **Decaying Blade ($1 \times 2$ Cursed Weapon):** 22 Dmg | 1.2s cooldown. *Drawback:* Deals 2 damage to adjacent items every 3s.
  20. **Blood Shield ($2 \times 2$ Cursed Shield):** 45 Shield. *Drawback:* Reduces all healing by 50%.

---

# 7. Synergy System & Adjacency Matrix

```
┌────────────────────────────────────────────────────────────────────────┐
│                        ELEMENTAL REACTION MATRIX                       │
├───────────────┬───────────────────────────────┬────────────────────────┤
│ Reaction Name │ Formula                       │ Combat Effect          │
├───────────────┼───────────────────────────────┼────────────────────────┤
│ **Steam**     │ Fire Beam + Ice Beam          │ 25% Enemy Blind/Miss   │
│ **Plasma**    │ Fire Beam + Lightning Beam    │ 18 Dmg/s Continuous Ray│
│ **Toxic Flame│ Fire Beam + Poison Beam       │ Detonates Poison (2x)  │
│ **Supercond.**│ Lightning Beam + Ice Beam     │ -40% Enemy Resistance  │
│ **Frostbite** │ Ice Beam + Poison Beam        │ +50% Poison Tick Dmg   │
└───────────────┴───────────────────────────────┴────────────────────────┘
```

### 7.1 Master Item Combinations (MVP)
* **Flaming Blade:** Ember Rune $(\rightarrow) + \text{Broadsword} \implies \text{Sword deals +6 Fire Dmg and applies Burn.}$
* **Venom Shiv:** Venom Rune $(\rightarrow) + \text{Rusty Dagger} \implies \text{Applies 2 Poison stacks every 0.8s.}$
* **Thunder Bow:** Spark Rune $(\uparrow) + \text{Shortbow} \implies \text{Arrows chain 8 Lightning Dmg to backline adds.}$
* **Molten Wall:** Ember Rune $(\downarrow) + \text{Tower Shield} \implies \text{Attackers take 8 Burn Dmg upon striking shield.}$
* **Shatterstrike:** Frost Rune $(\downarrow) + \text{Battleaxe} \implies \text{Axe deals 2x damage against chilled/frozen targets.}$

---

# 8. Chain Reaction Engine & Loop Prevention

```
[ Dagger Critical Strike ] ───► [ Nova Rune Detonation ]
                                         │
                                         ▼
[ Screen-Clearing Pulse ] ◄─── [ Spark Rune Arc ]
```

### 8.1 Execution & Loop Guard Algorithm
To eliminate infinite execution loops in circular beam configurations (e.g., Rune A $\rightarrow$ Rune B $\rightarrow$ Rune A):
1. **Frame-Tick Propagation Cap:** An item can trigger a downstream event at most **once per physics tick ($0.02\text{s}$)**.
2. **Recursion Depth Limit:** Hard-coded maximum call depth of $N = 4$ chain links per root trigger:
   $$\text{Depth} \le 4$$
3. **Execution Queue:** Chain reactions are enqueued in a `Queue<ChainEvent>` and processed sequentially at frame boundaries, preventing stack overflows.

---

# 9. Combat System & Simulation Engine

### 9.1 Simulation Architecture
* **Mode:** Semi-Automated Execution. Once the player taps `BATTLE`, items execute on independent timers:
  $$\text{Cooldown Remaining} = \text{Cooldown Remaining} - \Delta t \times \text{SpeedMultiplier}$$
* **Player Agency During Combat:** 
  * Speed Toggle: $1\times$ (Standard) $\rightarrow$ $2\times$ (Fast) $\rightarrow$ $3\times$ (Instant).
  * Manual Emergency Potion Tap: Player can tap a consumable to force an early drink.

### 9.2 Damage Calculation Pipeline

$$\text{FinalDamage} = \max\Big(\text{MinimumDamage}, \, ((\text{BaseDamage} + \text{RuneBonus}) \times \text{CritMultiplier} \times \text{DamageModifiers}) - \text{EnemyArmor}\Big)$$

* **Definitions & Multipliers:**
  * $\text{BaseDamage}$: Weapon's inherent attack value.
  * $\text{RuneBonus}$: Flat damage added by connected directional runes.
  * $\text{CritMultiplier}$: $1.0$ on normal hits; $1.5$ on critical hits (buffable via relics).
  * $\text{DamageModifiers}$: $1.0$ default multiplier (accounting for external status buffs/debuffs).
  * $\text{EnemyArmor}$: Flat defense value subtracted after multipliers.
  * $\text{MinimumDamage}$: Fixed floor of $1$ (an attack that lands will always deal at least 1 damage; damage never falls to 0 or negative).

* **Calculation Order Example:**
  $$\text{Base} = 10, \, \text{Rune} = 6, \, \text{Crit} = 1.5, \, \text{Mod} = 1.0, \, \text{Armor} = 4$$
  1. Raw Base + Rune: $10 + 6 = 16$
  2. Apply Multipliers: $16 \times 1.5 \times 1.0 = 24$
  3. Subtract Armor: $24 - 4 = 20$
  4. Apply Minimum Floor: $\max(1, 20) = 20 \implies \mathbf{\text{FinalDamage} = 20}$

---

# 10. Enemy & Boss Architecture

```
┌────┬───────────────────────┬──────────┬────────┬──────────────────────────────────────┬───────────────────────────────────────────┐
│ #  │ Enemy Name            │ Tier     │ HP/Spd │ Unique Grid-Disrupting Mechanic      │ Counter Strategy                          │
├────┼───────────────────────┼──────────┼────────┼──────────────────────────────────────┼───────────────────────────────────────────┤
│ 1  │ **Sewer Rat**         │ Normal   │ 35/1.2s│ Fast melee bites; tests opening burst│ High shield or fast daggers.              │
│ 2  │ **Goblin Thief**      │ Normal   │ 45/1.0s│ Steals 3 Gold on every hit!          │ Burst down before 5 seconds.              │
│ 3  │ **Armored Skeleton**  │ Normal   │ 75/2.0s│ 15 Armor; reflects 20% physical dmg  │ Elemental Wands & Poison Runes.           │
│ 4  │ **Venomous Spider**   │ Normal   │ 50/1.4s│ Inflicts 2 Poison stacks (bypasses HP│ Sun Runes & Healing Potions.              │
│ 5  │ **Acid Slime**        │ Elite    │ 160/2s │ Acid spit: Disables 1 random bag slot│ Redundant weapon arrays.                  │
│ 6  │ **Necromancer**       │ Elite    │ 140/3s │ Summons 2 Skeletons every 4 seconds  │ Lightning arc & piercing bows.            │
│ 7  │ **The Lich Lord**     │ **BOSS** │ 750/2.5│ Freezes top row; inverts rune beams! │ Horizontal cross-runes & Sun Rune cleanse.│
└────┴───────────────────────┴──────────┴────────┴──────────────────────────────────────┴───────────────────────────────────────────┘
```

---

# 11. Dungeon Progression & Room Topology

```
[ Floor 1: Normal Fight ] ───► [ Floor 2: Loot Cache ]
                                      │
         ┌────────────────────────────┴────────────────────────────┐
         ▼                                                         ▼
[ Floor 3: Elite Fight ]                                  [ Floor 3: Mystery Shrine ]
 (High Risk / Guaranteed Rune)                             (Safe / Gamble Event)
         │                                                         │
         └────────────────────────────┬────────────────────────────┘
                                      ▼
                           [ Floor 4: Merchant Stall ]
```

### Biome 1: The Cursed Sewers (10 Floors Total)
* **Floors 1–2:** Introduction encounters (Sewer Rats, Goblin Scouts).
* **Floor 3:** Elite Encounter (Acid Slime) or Mystery Shrine.
* **Floor 4:** Merchant Stall (Buy items/runes, expand bag slot for 40 Gold).
* **Floor 5:** Mid-Boss Challenge (Armored Skeleton Horde).
* **Floor 6:** Treasure Vault (Guaranteed Rare Item).
* **Floor 7:** Elite Encounter (Necromancer).
* **Floor 8:** Campfire Rest Site (Heal 40% HP OR Upgrade 1 Rune).
* **Floor 9:** Pre-Boss Merchant & Armory.
* **Floor 10:** Boss Chamber (**The Lich Lord**).

---

# 12. Player Progression: Run vs. Meta

```
┌────────────────────────────────────────────────────────────────────────┐
│                         PROGRESSION SEPARATION                         │
├───────────────────────────────────┬────────────────────────────────────┤
│ IN-RUN TEMPORARY (Resets on Death)│ PERSISTENT META-HUB (Permanent)    │
├───────────────────────────────────┼────────────────────────────────────┤
│ • Lattice Grid Expansions (+1 tile)│ • Blueprint Forge Unlocks (Embers) │
│ • In-run Gold & Consumables       │ • Hero Class Unlocks (4 Classes)   │
│ • Equipped Weapons & Runes        │ • Bestiary & Synergy Codex Entries │
│ • Character Level & XP            │ • Lifetime Run Statistics          │
└───────────────────────────────────┴────────────────────────────────────┘
```

---

# 13. Game Economy & Data-Driven Tuning

### 13.1 Economy Balance Sheet (Tunable via ScriptableObjects)

```
┌─────────────────────────┬──────────────┬───────────────────────────────┐
│ Resource / Item         │ Base Value   │ Tuning ScriptableObject Path  │
├─────────────────────────┼──────────────┼───────────────────────────────┤
│ Common Item Buy Cost    │ 15–25 Gold   │ `Data/Economy/ShopPrices.asset`│
│ Rare Item Buy Cost      │ 35–50 Gold   │ `Data/Economy/ShopPrices.asset`│
│ Rune Buy Cost           │ 30–45 Gold   │ `Data/Economy/ShopPrices.asset`│
│ Lattice Slot Expansion  │ 40 Gold      │ `Data/Economy/ShopPrices.asset`│
│ Gold Drop (Normal Mob)  │ 6–12 Gold    │ `Data/Enemies/LootTables.asset`│
│ Gold Drop (Elite Mob)   │ 20–35 Gold   │ `Data/Enemies/LootTables.asset`│
│ Embers Drop (Boss Clear)│ 80–120 Embers│ `Data/Progression/Embers.asset`│
└─────────────────────────┴──────────────┴───────────────────────────────┘
```

---

# 14. Game State Machine

```
┌────────────────────────────────────────────────────────────────────────┐
│                       FINITE STATE MACHINE (FSM)                       │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  BOOT STATE  │────►│  MAIN MENU   │────►│  RUN SETUP   │────►│  INVENTORY   │
│ • Load Save  │     │ • Play / HUB │     │ • Init Grid  │     │ • Drag/Drop  │
└──────────────┘     └──────────────┘     └──────────────┘     └──────┬───────┘
                                                                      │
                                                                      ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ RESULTS STATE│◄────│ VICTORY / DIE│◄────│ REWARD STATE │◄────│ COMBAT STATE │
│ • Award Ember│     │ • Win / Loss │     │ • Draft 1of3 │     │ • Auto-tick  │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

---

# 15. UI/UX & Screen Architecture

```
┌────────────────────────────────────────────────────────┐
│             PORTRAIT MOBILE UI HUD LAYOUT              │
│                                                        │
│ [TOP 30% - COMBAT & ENEMY DISPLAY ZONE]                │
│ • Floor: 07/10    [Gold: 85]    [Embers: 120]   [⚙]    │
│ • Lich Lord [HP: 450/750]  [Shield: 50]  [Status: 🔥2] │
│ • Animated 2D Pixel Boss Sprite & Combat Floaty Text   │
│                                                        │
│ ────────────────────────────────────────────────────── │
│ [MIDDLE 25% - PLAYER STATUS & COMBAT CONTROLS]         │
│ • Hero HP: [120/150]    Shield: [35]    Mana: [20/20]  │
│ • [ ▶ BATTLE ]    [ 1x / 2x Speed ]    [ 🧪 Potion ]   │
│                                                        │
│ ────────────────────────────────────────────────────── │
│ [BOTTOM 45% - ACTIVE THUMB LATTICE GRID ZONE]          │
│ • 5x5 Interactive Touch Grid (≥ 52 dp targets)         │
│ • Active LineRenderer Laser Conduits Glow Live         │
│ • [Staging Tray: 3 Unassigned Items]   [Sell Trash]    │
└────────────────────────────────────────────────────────┘
```

---

# 16. Mobile Ergonomics & Touch Controls

* **Touch Targets:** Minimum $52 \times 52\text{ dp}$ per grid cell ($160 \times 160$ physical pixels on modern high-DPI screens).
* **Touch Event Matrix:**
  * *Drag:* Pick up item with $1.1\times$ scale-up and dynamic drop shadow.
  * *Tap While Dragging:* Rotate item 90° clockwise.
  * *Tap on Grid Item:* Inspect full item stats and synergy links in a lightweight non-blocking tooltip.
  * *Haptics:* Trigger Unity `Handheld.Vibrate()` (light haptic click) on valid snap and rune connection.

---

# 17. Visual Direction & Art Style Guide

```
┌────────────────────────────────────────────────────────────────────────┐
│                        VISUAL PALETTE & STYLE                          │
├───────────────────────┬────────────────────────────────────────────────┤
│ **Aesthetic Theme**   │ Dark Neo-Arcane Fantasy (16-bit Pixel + Neon)  │
│ **Background Base**   │ Deep Slate Obsidian (`#0F111A`)                │
│ **Lattice Grid Frame**│ Burnished Ancient Brass (`#C59B27`)            │
│ **Fire Element**      │ Magma Crimson / Solar Orange (`#FF4500`)       │
│ **Ice Element**       │ Glacial Cyan / Frost Blue (`#00E5FF`)          │
│ **Lightning Element** │ Electric Violet / Arc Purple (`#B026FF`)       │
│ **Poison Element**    │ Toxic Emerald / Acid Green (`#39FF14`)         │
│ **Typography**        │ High-contrast clean pixel typography with      │
│                       │ drop-shadow (e.g. *Kenney Pixel* / *Monocraft*)│
└───────────────────────┴────────────────────────────────────────────────┘
```

---

# 18. Audio Design Matrix

```
┌────┬───────────────────────┬────────────┬────────────────────────────────────────────────────────┐
│ #  │ Audio Cue Name        │ Type       │ Sonic Description / Acoustic Profile                   │
├────┼───────────────────────┼────────────┼────────────────────────────────────────────────────────┤
│ 1  │ `ui_grid_pickup`      │ UI / SFX   │ Snappy wooden click with soft leather rustle.          │
│ 2  │ `ui_grid_snap`        │ UI / SFX   │ Solid metallic thud with high haptic resonance.        │
│ 3  │ `rune_conduit_ignite` │ Magic SFX  │ Crackling laser hum building to an electric pitch.     │
│ 4  │ `combat_blade_slash`  │ Combat SFX │ Heavy steel-on-steel slicing swoosh.                   │
│ 5  │ `combat_burn_tick`    │ Status SFX │ Sizzling ember crackle.                                │
│ 6  │ `combat_freeze_shatter│ Status SFX │ High-frequency crystalline glass shatter.              │
│ 7  │ `combat_boss_roar`    │ Voice SFX  │ Deep resonant low-frequency monster growl.             │
│ 8  │ `bgm_dungeon_loop`    │ Music      │ Atmospheric 16-bit dungeon synth with driving bassline.│
└────┴───────────────────────┴────────────┴────────────────────────────────────────────────────────┘
```

---

# 19. Unity Software Architecture

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           // GameManagers, FSM, Global Enums, EventBus
│   │   ├── Grid/           // LatticeGrid, GridCell, TouchController
│   │   ├── Items/          // ItemDataSO, InventoryItem, ItemDatabase
│   │   ├── Runes/          // RuneConduitEngine, BeamRenderer, PrismRay
│   │   ├── Synergy/        // SynergyCalculator, ElementalReactions
│   │   ├── Combat/         // CombatSimulation, DamagePipeline, StatusEffects
│   │   ├── Enemies/        // EnemyController, BossAI, LootDropper
│   │   ├── Dungeon/        // FloorManager, RoomNode, BiomeDatabase
│   │   ├── Progression/    // MetaForge, BlueprintManager, EmberWallet
│   │   ├── UI/             // UIManager, ScreenControllers, Tooltips
│   │   ├── Audio/          // SoundManager, HapticFeedbackController
│   │   └── Save/           // SaveSystem, EncryptionUtility, SaveDataModel
│   ├── ScriptableObjects/  // Items, Runes, Enemies, Biomes, Economy
│   ├── Prefabs/            // GridCells, BeamRenderers, UI Screens, FloatyText
│   ├── Art/                // Sprites, TextureAtlases, UI Slices, Shaders
│   └── Audio/              // SFX, Ambient Loops, Music Tracks
```

---

# 20. Core C# Systems & Class Responsibilities

```
┌─────────────────────────┬────────────────────────────────────────────────────────────────────────┐
│ C# Class / Component    │ Primary Responsibility & System Boundary                               │
├─────────────────────────┼────────────────────────────────────────────────────────────────────────┤
│ `GameManager`           │ Root persistent singleton orchestrating high-level state & scenes.     │
│ `LatticeGrid`           │ Manages the $5 \times 5$ array, slot queries, coordinate bounds, state.│
│ `TouchController`       │ Translates mobile touch inputs into drag, drop, rotate & inspect.      │
│ `RuneConduitEngine`     │ Raycasts directional vectors across the lattice; updates LineRenderers.│
│ `SynergyCalculator`     │ Calculates adjacency buffs and elemental reaction triggers.           │
│ `CombatSimulation`      │ Ticks weapon cooldowns, manages health bars, triggers status ticks.    │
│ `SaveManager`           │ Serializes run state & persistent meta-currency to local JSON storage. │
│ `UIManager`             │ Controls screen transitions, HUD binding, and floaty damage text pools.│
└─────────────────────────┴────────────────────────────────────────────────────────────────────────┘
```

---

# 21. Data Architecture & ScriptableObjects

```csharp
// Core ScriptableObject defining item data
[CreateAssetMenu(fileName = "Item_", menuName = "Lattirune/Data/Item")]
public class ItemDataSO : ScriptableObject
{
    public string itemID;
    public string displayName;
    public ItemCategory category;
    public ElementType element;
    public Vector2Int dimensions = Vector2Int.one;
    public Sprite icon;
    
    [Header("Combat Stats")]
    public float baseDamage;
    public float baseShield;
    public float cooldownSeconds = 1.5f;
    
    [Header("Conduit Properties")]
    public ConduitDirection emissionDirection = ConduitDirection.None;
    public float elementalPower;
}
```

---

# 22. Save System & State Serialization

* **Storage Target:** Encrypted local JSON file saved to `Application.persistentDataPath + "/lattirune_save.dat"`.
* **Atomic Save Pattern:** Writes to a temporary `.tmp` file and replaces the target file atomically to prevent save corruption during unexpected app termination.
* **Save Data Schema:**
  * `MetaCurrency` (Embers count)
  * `UnlockedBlueprints` (List of item IDs)
  * `ActiveRunState` (Current floor, grid layout array, player current HP, current gold).

---

# 23. Performance Budget & Mobile Optimization

```
┌───────────────────────────┬────────────────────────────────────────────────────────┐
│ Performance Metric        │ Target Development Budget (Validated via Profiling)    │
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ **Target Framerate**      │ Target 60 FPS (Stable during conduit cascades)         │
│ **Total Memory (RAM)**    │ Target < 180 MB Total Resident Memory                  │
│ **Draw Calls (Batches)**  │ Target < 25 Draw Calls per frame (Sprite Atlasing)     │
│ **Cold Startup Time**     │ Target < 2.0 Seconds to interactive Main Menu          │
│ **Texture Compression**   │ ASTC 6x6 on Mobile / Textures strictly ≤ 2048x2048     │
│ **Object Pooling**        │ Floaty damage text & laser beam segments 100% pooled   │
└───────────────────────────┴────────────────────────────────────────────────────────┘
```

---

# 24. Device Compatibility & Hardware Tiers

```
┌───────────────────────────┬───────────────────────────────┬──────────────────────────────┐
│ Hardware Tier             │ Representative Class          │ Performance Expectation      │
├───────────────────────────┼───────────────────────────────┼──────────────────────────────┤
│ **Low-End Baseline**      │ Helio G85 / 3-4GB RAM         │ Target 45–60 FPS             │
│ **Baseline Android Test** │ Snapdragon 680 / 4GB RAM class│ Target stable 60 FPS         │
│ **Mid-Range Development** │ Snapdragon 778G / 6-8GB RAM   │ Target rock-solid 60 FPS     │
│ **High-End Reference**    │ Snapdragon 8 Gen 2 / A15+     │ Target 60–120 FPS            │
└───────────────────────────┴───────────────────────────────┴──────────────────────────────┘
```

* **Aspect Ratio Range:** Responsive across $16:9$ (legacy tablets) to $21.5:9$ (tall smartphones).
* **Safe Area Handling:** Dedicated `SafeAreaFitter.cs` component offsets top HUD icons below hardware camera notches and dynamic islands.

---

# 25. Offline-First Architecture

* **Zero Mandatory Internet:** 100% of gameplay, combat simulation, RNG generation, and local save progression executes locally with zero network calls.
* **Graceful Degradation:** When offline, analytics and cloud backups silently queue locally and sync only when network connectivity is re-established.

---

# 26. Analytics & Telemetry Schema

* **Platform:** Privacy-compliant Google Analytics for Firebase (or Unity Analytics).
* **Key Event Pipeline:**
  1. `run_start` (Class selected)
  2. `floor_cleared` (Floor number, time taken, current HP)
  3. `synergy_discovered` (Rune ID + Weapon ID)
  4. `run_ended` (Floor reached, cause of death, total gold earned)
  5. `blueprint_unlocked` (Item ID, embers spent)

---

# 27. Monetization Integration Strategy

> **Design Principle:** Postponed strictly to Phase 7. The core game must be exceptionally fun as a standalone experience before monetization is enabled.

* **Rewarded Ads (100% Opt-in):** 1 Merchant Shop Reroll per floor; 1 Dungeon Revive per run at 50% HP.
* **Non-Consumable IAP:** ₹199 ($2.99) *"Remove Death Ads & Permanent +10% Embers"*.
* **Zero Pay-to-Win:** Meta-upgrades are strictly capped; skill, grid geometry, and tactical choices determine victory.

---

# 28. Platform Compliance & Privacy Requirements

* **Google Play Store:** Target Android 14/15 (API level 34+), complete Data Safety form declaring zero user tracking, complete 20-tester closed beta requirement.
* **Apple App Store:** StoreKit 2 integration, prominent "Restore Purchases" button in Settings, Privacy Nutrition Label declaration.

---

# 29. Testing & Quality Assurance Strategy

* **Unity Test Runner (EditMode):**
  * `LatticeGridTests.cs`: Verifies $5 \times 5$ boundary checks, rotation matrices, and collision overlaps.
  * `ConduitRaycastTests.cs`: Asserts that an East-facing Fire Rune correctly detects and buffs an adjacent sword.
* **Playmode Simulation Tests:** Automated 1,000-run Monte Carlo simulation script balancing enemy HP vs player DPS curves.

---

# 30. Development Phases & Milestones

```
┌────────────────────────────────────────────────────────────────────────┐
│                        DEVELOPMENT ROADMAP PHASES                      │
├─────────┬───────────────────┬──────────────────────────────────────────┤
│ Phase 0 │ Planning & Spec   │ PLAN.md approved; repo initialized.      │
│ Phase 1 │ Core Prototype    │ 5x5 Grid + 5 Items + 2 Runes + 1 Enemy.  │
│ Phase 2 │ Vertical Slice    │ 3 Floors + 1 Boss + Audio + Final UI.    │
│ Phase 3 │ Content Build     │ 10 Floors + 20 Items + 10 Runes.         │
│ Phase 4 │ Meta Progression  │ Campfire Hub + Blueprints + Save System. │
│ Phase 5 │ Polish & Feel     │ Screen shake, particle juice, haptics.   │
│ Phase 6 │ QA & Testing      │ 20-Tester closed beta on Google Play.    │
│ Phase 7 │ Monetization      │ AdMob Rewarded SDK + $2.99 IAP unlock.   │
│ Phase 8 │ Store Launch      │ Production release on iOS & Android.     │
└─────────┴───────────────────┴──────────────────────────────────────────┘
```

---

# 31. First 14 Days Implementation Sprint

```
┌────────────────────────────────────────────────────────────────────────┐
│                       DAY-BY-DAY SPRINT SCHEDULE                       │
├────────┬───────────────────────────────────────────────────────────────┤
│ Day 1  │ Initialize Git repo `lattirune-game`. Setup Unity 6 2D URP.   │
│ Day 2  │ Code `LatticeGrid.cs` (5x5 2D array data structure & bounds). │
│ Day 3  │ Build `TouchController.cs` (Drag-and-drop & 90° rotation).    │
│ Day 4  │ Implement grid tile snapping and green/red valid drop visual. │
│ Day 5  │ Code `RuneConduitEngine.cs` (Cardinal raycasting logic).      │
│ Day 6  │ Implement `LineRenderer` glowing laser conduit visualizer.    │
│ Day 7  │ Code `ItemDataSO.cs` and create first 5 items + 2 runes.      │
│ Day 8  │ Build `CombatSimulation.cs` (Weapon cooldown attack ticks).   │
│ Day 9  │ Implement health bars, floating combat text, and damage math. │
│ Day 10 │ Code first test enemy (`SewerRat`) and auto-combat resolution.│
│ Day 11 │ Build basic loot draft screen (Choose 1 of 2 items on win).   │
│ Day 12 │ Assemble 1-encounter test loop (Fight -> Loot -> Next Fight). │
│ Day 13 │ Add sound effects (snap, click, laser hum, slash) and shake.  │
│ Day 14 │ Internal test build on Android. (Optional: WebGL for desktop).│
└────────┴───────────────────────────────────────────────────────────────┘
```
> **Note on Day 14 Testing:** Exporting a WebGL build is an **optional** target for rapid browser playtesting. Android physical device profiling remains **mandatory** for mobile performance validation.

---

# 32. Scope Hierarchy Matrix (Prototype vs Vertical Slice vs MVP)

```
┌──────────────────────────────────────┬──────────────────────────────────────┬──────────────────────────────────────┐
│ CORE PROTOTYPE (Phase 1: Days 1–14)  │ VERTICAL SLICE (Phase 2: Month 1)    │ MVP 1.0 (Phase 3+: Release)          │
├──────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────┤
│ • 5x5 Grid (Drag, drop, rotate)      │ • 3 Floors + 1 Boss Encounter        │ • Full 10 Floors (Cursed Sewers)     │
│ • Max 5 Items + 2 Runes (Fire, Ice)  │ • 10 Items + 4 Runes                 │ • 20 Items + 10 Runes                │
│ • 1 Enemy (Sewer Rat) + 1 Combat Enc.│ • 2 Elemental Reactions              │ • 5 Master Elemental Reactions       │
│ • 1 Synergy (Ember Rune + Sword)     │ • Full Audio / SFX / Screen Shake    │ • 6 Enemies + 1 Boss (Lich Lord)     │
│ • Basic Raycasting & Damage Formula  │ • Polished 16-Bit UI Layout          │ • Encrypted Local JSON Save System   │
│ • Zero Meta / Zero Ads / Zero Stores │ • Basic Run Progression Loop         │ • Campfire Meta-Hub & Blueprints     │
└──────────────────────────────────────┴──────────────────────────────────────┴──────────────────────────────────────┘
```

---

# 33. Vertical Slice Specification

The **Lattirune Vertical Slice** (Milestone Phase 2) must prove the fun factor with production-quality assets before content scaling:
1. **Playable Loop:** Floor 1 $\rightarrow$ Loot Draft $\rightarrow$ Floor 2 (Elite) $\rightarrow$ Campfire Upgrade $\rightarrow$ Floor 3 (**Boss Battle**).
2. **Audio/Visual Polish:** High-contrast 16-bit sprites, animated glowing laser conduits, responsive screen shake on critical hits, punchy audio feedback on every drag-and-drop.
3. **Target Device Performance:** Stable 60 FPS on baseline Android test device (Snapdragon 680 / 4GB RAM class).

---

# 34. Quality Gates & Go/No-Go Criteria

```
┌───────────────────────────┬────────────────────────────────────────────────────────┐
│ Quality Gate Dimension    │ Practical Measurable Metric                            │
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ **Comprehension Speed**   │ > 80% of playtesters understand rune laser beams in    │
│                           │ under 45 seconds without reading a tutorial.           │
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ **Voluntary Replay Rate** │ > 60% of playtesters immediately tap "Play Again"     │
│                           │ upon dying on Floor 3.                                 │
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ **Touch Ergonomics**      │ Critical drag/drop interactions have near-zero         │
│                           │ accidental placements during structured playtests.     │
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ **Framerate Stability**   │ Maintain approximately 60 FPS on target test device    │
│                           │ with profiling used to resolve frame-time spikes.      │
└───────────────────────────┴────────────────────────────────────────────────────────┘
```

---

# 35. Risk Register & Mitigation Matrix

```
┌────┬───────────────────────┬──────┬──────┬──────────────────────────────────┬────────────────────────────────────────┐
│ #  │ Identified Risk       │ Prob │ Imp  │ Proactive Mitigation Strategy    │ Early Warning Trigger                  │
├────┼───────────────────────┼──────┼──────┼──────────────────────────────────┼────────────────────────────────────────┤
│ 1  │ **Scope Creep**       │ High │ High │ Strictly enforce the 20-item MVP;│ Sprints taking > 14 days               │
│    │                       │      │      │ cut all nice-to-have systems.    │ without a playable build.              │
├────┼───────────────────────┼──────┼──────┼──────────────────────────────────┼────────────────────────────────────────┤
│ 2  │ **Boring Combat**     │ Med  │ High │ Add screen shake, floating crits,│ Playtesters describe combat as         │
│    │                       │      │      │ and elemental reaction cascades. │ "waiting around for bars to empty".    │
├────┼───────────────────────┼──────┼──────┼──────────────────────────────────┼────────────────────────────────────────┤
│ 3  │ **Touch Misclicks**   │ Med  │ High │ Enforce ≥ 52 dp touch targets    │ Playtesters accidentally drop items    │
│    │                       │      │      │ and elastic snapping.            │ in wrong slots.                        │
├────┼───────────────────────┼──────┼──────┼──────────────────────────────────┼────────────────────────────────────────┤
│ 4  │ **Asset Inconsistency│ Med  │ Med  │ Clean all AI pixel art in        │ Visual mismatch between items and grid.│
│    │                       │      │      │ Aseprite with unified 16-color.  │                                        │
└────┴───────────────────────┴──────┴──────┴──────────────────────────────────┴────────────────────────────────────────┘
```

---

# 36. AI-Assisted Development Workflow

```
[ 1. PLAN SPEC ] ───► [ 2. PROMPT AI ] ───► [ 3. CODE REVIEW ] ───► [ 4. INTEGRATE & TEST ]
  (Define class API)   (Cursor / Claude)    (Verify SOLID rules)   (Run Unit Tests in Unity)
```

### Protocol Rules:
* AI is an **implementation accelerator**, not an autonomous designer.
* Every AI-generated script must undergo human code review for memory allocations (avoiding `new` inside `Update()`), proper object pooling, and adherence to Unity best practices.

---

# 37. Git & Version Control Strategy

* **Branching Model:** `main` (Production/Release) $\leftarrow$ `develop` (Integration) $\leftarrow$ `feature/grid-raycasting`.
* **Commit Conventions:** Semantic Conventional Commits (`feat:`, `fix:`, `refactor:`, `perf:`, `docs:`).

---

# 38. Documentation Suite

* `PLAN.md`: Master architectural blueprint and systems plan (This document).
* `ARCHITECTURE.md`: Technical class diagrams, data models, and API interfaces.
* `GAME_DESIGN.md`: Detailed item stats, damage numbers, and balance sheets.
* `CHANGELOG.md`: Chronological log of versions, fixes, and milestones.

---

# 39. Portfolio Engineering Strategy

When published on your developer portfolio and GitHub, **Lattirune** will highlight:
1. **Custom 2D Computational Raycasting Engine:** Highlighting discrete array traversal algorithms.
2. **Data-Driven Architecture:** Complete separation of game balance via ScriptableObjects.
3. **Mobile-First Optimization:** 60 FPS profiling data, draw-call batching, and memory footprint management.
4. **Production Discipline:** Clean Git commit history, automated unit tests, and comprehensive technical documentation.

---

# 40. Project-Wide Definition of Done

A feature in **Lattirune** is considered **DONE** only when:
1. It is fully implemented in clean, decoupled C# adhering to SOLID principles.
2. It has zero compiler warnings and passes all EditMode/PlayMode unit tests.
3. It has been playtested on a physical Android device at locked 60 FPS.
4. It is integrated with the audio, haptic, and save systems where required.
5. It is documented in the codebase with clean XML summaries.

---

# 41. Master Development Roadmap

```
┌────────────────────────────────────────────────────────────────────────┐
│                        MASTER MILESTONE FLOW                           │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ 1. PLANNING      │────►│ 2. PROTOTYPE     │────►│ 3. VERTICAL SLICE│
│ • PLAN.md Locked │     │ • 5x5 Grid Core  │     │ • 3 Floors + Boss│
│ • Repo Initialized     │ • Raycast Conduits     │ • Polished Audio │
└──────────────────┘     └──────────────────┘     └─────────┬────────┘
                                                            │
                                                            ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ 6. STORE LAUNCH  │◄────│ 5. TEST & POLISH │◄────│ 4. MVP PRODUCTION│
│ • Production APK │     │ • 20-Tester Beta │     │ • 10 Floors      │
│ • iOS App Store  │     │ • AdMob / IAP    │     │ • 20 Items+Runes │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

---

# 42. First Actionable Task

```
┌────────────────────────────────────────────────────────────────────────┐
│                             NEXT ACTION                                │
├────────────────────────────────────────────────────────────────────────┤
│ TASK ID: TASK-001                                                      │
│ TITLE: Initialize Unity 6 LTS 2D Project & Git Repository              │
│                                                                        │
│ EXECUTION STEPS:                                                       │
│ 1. Initialize Git repository in workspace: `git init`                  │
│ 2. Add standard Unity `.gitignore` and `.gitattributes`                │
│ 3. Setup folder directory structure under `Assets/_Project/`           │
│ 4. Configure Unity project settings:                                   │
│    • Color Space: Linear                                               │
│    • Target Resolution: 1080x1920 (Portrait)                          │
│    • Target Framerate: 60 FPS                                          │
│ 5. Commit: `chore: initial project structure and baseline configuration`│
└────────────────────────────────────────────────────────────────────────┘
```
