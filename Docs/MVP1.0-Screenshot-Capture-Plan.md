# Lattirune MVP 1.0 Google Play Screenshot Capture Plan

**Date:** August 19, 2026  
**Application:** Lattirune  
**Target Package:** `com.developer.lattirune`  
**Reference Resolution:** $1080 \times 1920$ (Portrait 9:16)  
**Status:** `PENDING PHYSICAL/EDITOR CAPTURE`

---

## 1. Capture Guidelines & Quality Standards

* **Orientation:** Strict Portrait mode ($1080 \times 1920$).
* **Safe Area:** Zero critical UI elements cut off by rounded device corners or camera notches.
* **Aspect Ratio:** $16:9$ portrait or modern tall aspect ratio ($19.5:9, 20:9$).
* **Exclusions (Must NOT Appear):**
  * No debug logging text or development console windows.
  * No desktop mouse cursors or simulated touch pointer circles.
  * No development FPS counter overlays or memory profiling charts.
  * No low-resolution placeholder sprites or missing texture magenta squares.

---

## 2. 12-Screen Capture Specification

### Screen 01: Main Menu
* **Screen State:** `MAIN_MENU`
* **Purpose:** First impression showcasing official title typography and primary mobile navigation.
* **Visible UI:** *Lattirune* logo, "Start New Run", "Campfire Hub", "Settings", Version `1.0.0` footer.
* **Suggested Caption:** *Enter the Cursed Sewers in a tactical spatial roguelite!*
* **QA Status:** `PENDING CAPTURE`

### Screen 02: Campfire Meta-Hub
* **Screen State:** `CAMPFIRE_HUB`
* **Purpose:** Showcase persistent meta-progression currencies, lifetime stats, and hub atmosphere.
* **Visible UI:** Persistent Dungeon Embers balance, Boss Clears tally, "Enter Forge", "Start Run".
* **Suggested Caption:** *Manage your persistent Dungeon Embers at the Campfire Hub!*
* **QA Status:** `PENDING CAPTURE`

### Screen 03: Grid Build & Spatial Placement
* **Screen State:** `GRID_BUILD`
* **Purpose:** Demonstrate the core $5 \times 5$ spatial lattice grid and backpack equipment management.
* **Visible UI:** $5 \times 5$ lattice grid, equipped weapons (Iron Broadsword, Rusty Dagger), active backpack slots ($2 \times 3$).
* **Suggested Caption:** *Arrange weapons and relics on the 5x5 lattice grid!*
* **QA Status:** `PENDING CAPTURE`

### Screen 04: Directional Rune Conduits
* **Screen State:** `GRID_BUILD` (Conduit Highlight Active)
* **Purpose:** Showcase glowing cardinal energy beams emitted from directional runes powering weapons in line.
* **Visible UI:** Fire/Lightning runes projecting continuous luminous energy vectors across weapons.
* **Suggested Caption:** *Power weapons with directional elemental energy conduits!*
* **QA Status:** `PENDING CAPTURE`

### Screen 05: Active Auto-Battle Combat
* **Screen State:** `COMBAT`
* **Purpose:** Showcase combat speed control agency, health bars, and active battle dynamics.
* **Visible UI:** Hero & Enemy HP bars, attack cooldown tickers, speed toggle ($1\times, 2\times, 3\times$), emergency potion button.
* **Suggested Caption:** *Unleash tactical auto-battles with 1x, 2x, and 3x speed controls!*
* **QA Status:** `PENDING CAPTURE`

### Screen 06: 3-Card Reward Draft
* **Screen State:** `REWARD_SELECTION`
* **Purpose:** Demonstrate roguelite draft agency after defeating dungeon encounters.
* **Visible UI:** Three distinct reward cards (e.g. Apprentice Wand, Frost Rune, Ruby Ring), "Select" and "Skip" buttons.
* **Suggested Caption:** *Draft powerful items, runes, and relics after each victory!*
* **QA Status:** `PENDING CAPTURE`

### Screen 07: Dungeon Merchant Stall
* **Screen State:** `MERCHANT` (Floors 4 & 9)
* **Purpose:** Showcase in-run Gold economy, consumable purchases, and backpack expansion.
* **Visible UI:** In-run Gold counter, 4 purchasable shop wares with Gold price tags, "Expand Bag Slot" button.
* **Suggested Caption:** *Spend hard-earned dungeon gold at floor merchant stalls!*
* **QA Status:** `PENDING CAPTURE`

### Screen 08: Campfire Rest Site
* **Screen State:** `CAMPFIRE_REST` (Floor 8)
* **Purpose:** Illustrate high-stakes tactical branching decisions before boss encounters.
* **Visible UI:** Option A (Rest & Heal 40% HP) vs Option B (Rune Reforge +2 Power) cards.
* **Suggested Caption:** *Make critical tactical choices at mid-run campfire rest sites!*
* **QA Status:** `PENDING CAPTURE`

### Screen 09: The Lich Lord (Boss Encounter)
* **Screen State:** `BOSS` / `COMBAT` (Floor 10)
* **Purpose:** Showcase the climax of Biome 1 against the 3-phase Lich Lord.
* **Visible UI:** Lich Lord boss model (750 HP bar), phase banner (*Phase 2: Soul Harvest*), particle explosion effects.
* **Suggested Caption:** *Conquer the 3-phase Lich Lord in an epic boss showdown!*
* **QA Status:** `PENDING CAPTURE`

### Screen 10: Run Complete (Victory Screen)
* **Screen State:** `RUN_COMPLETE`
* **Purpose:** Display victory rewards, total floor clears, and awarded persistent Dungeon Embers.
* **Visible UI:** "Victory Cleared" banner, Embers Awarded ($+100$), Run Summary stats, "Return to Hub" button.
* **Suggested Caption:** *Claim persistent Dungeon Embers upon dungeon triumph!*
* **QA Status:** `PENDING CAPTURE`

### Screen 11: Blueprint Forge (Meta-Progression)
* **Screen State:** `BLUEPRINT_FORGE`
* **Purpose:** Showcase permanent hero talent upgrades and persistent blueprint unlocking.
* **Visible UI:** Blueprint grid (12 blueprints), Embers cost badges, "Unlock" button, stat modifier descriptions.
* **Suggested Caption:** *Unlock 12 permanent game-changing hero blueprints!*
* **QA Status:** `PENDING CAPTURE`

### Screen 12: Audio & Haptics Settings
* **Screen State:** `SETTINGS`
* **Purpose:** Demonstrate accessibility, volume control, and tactile haptic feedback toggles.
* **Visible UI:** Master Volume slider, SFX Volume slider, Haptic Vibration toggle, "Back" navigation.
* **Suggested Caption:** *Customize sound, audio volume, and tactile haptic feedback!*
* **QA Status:** `PENDING CAPTURE`
