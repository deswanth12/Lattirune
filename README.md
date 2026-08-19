# Lattirune
> *Align the Lattice. Awaken the Runes.*

A portrait-mode 2D spatial inventory auto-battler roguelite where directional elemental runes emit energy conduits across a $5 \times 5$ lattice grid, powering weapons and triggering compounding elemental chain reactions.

## Project Specifications

* **Genre:** 2D spatial roguelite / inventory auto-battler
* **Platforms:** Android (Primary) / iOS (Primary) / PC (Steam post-launch)
* **Engine:** Unity 6 LTS (2D URP)
* **Language:** C#
* **Orientation:** Portrait ($1080 \times 1920$ reference canvas)
* **Development Status:** Pre-production / Phase 3 Content Build

## MVP 1.0 10-Rune Catalogue

The complete rune catalogue defined in PLAN.md Section 5.1:

| # | Rune Name | ID | Element | Direction | Mechanical In-Combat Effect | Compatible Setup |
| :- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | **Ember Rune** | `rune_ember` | Fire | East ($\rightarrow$) | +6 Fire Dmg; Burn (3 dmg/s for 4s) | Blades, Bows, Wands |
| 2 | **Frost Rune** | `rune_frost` | Ice | South ($\downarrow$) | +4 Ice Dmg; Enemy speed -15% | Shields, Daggers |
| 3 | **Spark Rune** | `rune_spark` | Lightning | North ($\uparrow$) | +8 Shock Dmg; 25% chain arc chance | Fast weapons (<1.5s) |
| 4 | **Venom Rune** | `rune_venom` | Poison | West ($\leftarrow$) | 2 Poison stacks/sec (ignores shields) | Daggers, Bows |
| 5 | **Crossfire Rune** | `rune_crossfire` | Fire | Cross ($+$) | +3 Fire Dmg in all 4 cardinal vectors | Multi-weapon setups |
| 6 | **Prism Rune** | `rune_prism` | Light | Split ($\backslash$) | Splits incoming beam into 2 branches | Runes & optical setups |
| 7 | **Amplifier Node** | `rune_amplifier` | Force | Omni ($\circ$) | Doubles power of adjacent runes | Concentrated rune clusters |
| 8 | **Iron Rune** | `rune_iron` | Earth | South ($\downarrow$) | +15 Shield at battle start | Armor, Shields |
| 9 | **Vampire Rune** | `rune_vampire` | Shadow | North ($\uparrow$) | Heals player for 12% of damage dealt | Bladed weapons |
| 10 | **Haste Rune** | `rune_haste` | Wind | East ($\rightarrow$) | +25% Attack Speed to weapons in line | Heavy weapons (>2.5s) |

## MVP 1.0 Item Catalogue (20 Items)

The complete item catalogue defined in PLAN.md Section 6.1:

| # | Item Name | ID | Category | Footprint | Base Stats | Special Traits / Synergies |
| :- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | **Rusty Dagger** | `item_rusty_dagger` | Weapon | $1 \times 1$ | 4 Dmg, 0.8s cd | +2 Dmg if placed in corner tile |
| 2 | **Iron Broadsword** | `item_iron_broadsword` | Weapon | $1 \times 2$ | 10 Dmg, 2.0s cd | +3 Dmg for each adjacent weapon |
| 3 | **Shortbow** | `item_shortbow` | Weapon | $2 \times 1$ | 6 Dmg, 1.4s cd | 5 Armor Pierce |
| 4 | **Apprentice Wand** | `item_apprentice_wand` | Weapon | $1 \times 2$ | 7 Dmg, 1.8s cd | +50% Elemental Rune Damage |
| 5 | **Battleaxe** | `item_battleaxe` | Weapon | L-Shape ($2 \times 2$) | 18 Dmg, 3.0s cd | $1.5\times$ Dmg if shield is 0 |
| 6 | **Phalanx Spear** | `item_phalanx_spear` | Weapon | $1 \times 3$ | 12 Dmg, 1.8s cd | +4 Dmg for empty tiles behind shaft |
| 7 | **Wooden Buckler** | `item_wooden_buckler` | Shield | $1 \times 1$ | 8 Shield | Starting shield at battle start |
| 8 | **Iron Tower Shield** | `item_iron_tower_shield` | Shield | $2 \times 2$ | 25 Shield | Heavy insulator shield |
| 9 | **Spiked Buckler** | `item_spiked_buckler` | Shield | $1 \times 2$ | 12 Shield | Reflects 4 Thorns damage when struck |
| 10 | **Leather Tunic** | `item_leather_tunic` | Armor | $2 \times 2$ | +25 Max HP | +10 HP per adjacent potion |
| 11 | **Chainmail Coat** | `item_chainmail_coat` | Armor | $2 \times 2$ | +15 Max HP | Reduces incoming damage by 2 flat |
| 12 | **Whetstone** | `item_whetstone` | Relic | $1 \times 1$ | +3 Flat Dmg | All adjacent blades gain +3 Base Dmg |
| 13 | **Ruby Ring** | `item_ruby_ring` | Relic | $1 \times 1$ | Passive | Adjacent Fire Runes gain +25% burn |
| 14 | **Sapphire Ring** | `item_sapphire_ring` | Relic | $1 \times 1$ | Passive | Adjacent Ice Runes gain +25% slow |
| 15 | **Lucky Clover** | `item_lucky_clover` | Relic | $1 \times 1$ | +10% Crit | Increases Critical Strike Chance |
| 16 | **Health Potion** | `item_health_potion` | Consumable | $1 \times 1$ | +35 HP | Auto-drinks below 30% HP |
| 17 | **Stamina Flask** | `item_stamina_flask` | Consumable | $1 \times 1$ | +40% Speed | Auto-drinks at battle start for 4s |
| 18 | **Poison Vial** | `item_poison_vial` | Consumable | $1 \times 1$ | 15 Poison | Inflicts 15 Poison on attacker on hit |
| 19 | **Decaying Blade** | `item_decaying_blade` | Cursed Weapon | $1 \times 2$ | 22 Dmg, 1.2s cd | Cursed: Deals 2 dmg to adjacent items |
| 20 | **Blood Shield** | `item_blood_shield` | Cursed Shield | $2 \times 2$ | 45 Shield | Cursed: Reduces healing by 50% |

## Phase 3: Biome 1 ("The Cursed Sewers") & 6-Enemy Bestiary

The 10-floor dungeon expands across Biome 1 with data-driven enemy archetypes and tactical grid-disrupting traits:

| Enemy Name | Tier | HP | Armor | Interval | Unique Trait / Mechanic | Tactical Counterplay |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Sewer Rat** | Normal | 35 | 0 | 1.2s | High attack rate swarm | High shield or fast daggers |
| **Goblin Thief** | Normal | 45 | 0 | 1.0s | Steals 3 Gold on every hit | Burst down before 5 seconds |
| **Armored Skeleton** | Normal | 75 | 15 | 2.0s | Reflects 20% physical damage | Elemental wands & poison runes |
| **Venomous Spider** | Normal | 50 | 0 | 1.4s | Inflicts 2 ticking Poison stacks | Sun runes & healing potions |
| **Acid Slime** | Elite | 160 | 2 | 2.0s | Acid spit: disables 1 bag slot | Redundant weapon arrays |
| **Necromancer** | Elite | 140 | 0 | 3.0s | Summons 2 Skeletons every 4.0s | Lightning arc & piercing bows |
| **The Lich Lord** | BOSS | 750 | 10 | 2.5s | 3-Phase dynamic enrage | Multi-beam reaction synergies |

### 10-Floor Topology (Biome 1)

1. **Floor 1:** Sewer Entry (Sewer Rat Skirmish)
2. **Floor 2:** Drain Basin (Goblin Thief Ambush)
3. **Floor 3:** Slime Cavern (Elite: Acid Slime)
4. **Floor 4:** Merchant Stall
5. **Floor 5:** Armory Gate (Mid-Boss: Armored Skeleton)
6. **Floor 6:** Treasure Vault
7. **Floor 7:** Bone Crypt (Elite: Necromancer)
8. **Floor 8:** Campfire Rest Site
9. **Floor 9:** Spider Nest (Venomous Spider)
10. **Floor 10:** Boss Sanctum (The Lich Lord - 3 Phases)

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

## Boss Encounter Mechanics & The Lich Lord

The Lich Lord (`boss_lich_lord`) is a 3-phase boss encounter with deterministic dynamic enrage thresholds:

| Phase # | Phase Name | HP% Threshold | Effective Armor | Effective Attack | Attack Interval |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Phase 1** | Frost Warden | $100\% \rightarrow 66\%$ | 10 | 8 | 2.5s |
| **Phase 2** | Soul Harvest | $66\% \rightarrow 33\%$ | 15 (+5) | 12 (+4) | 2.0s (0.8×) |
| **Phase 3** | Necrotic Inversion | $33\% \rightarrow 0\%$ | 20 (+10) | 16 (+8) | 1.6s (0.64×) |

## Documentation

* [`PLAN.md`](./PLAN.md) — Master project planning and technical architecture blueprint (v1.0.1).
