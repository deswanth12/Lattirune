# Lattirune MVP 1.0.0 Release Notes

**Release Date:** August 19, 2026  
**Version:** `1.0.0` (Build `1`)  
**Package Identifier:** `com.developer.lattirune`  
**Target Platform:** Android (Portrait $1080 \times 1920$) / iOS / PC  
**Engine:** Unity 6 LTS (2D URP)  
**Save Version:** `1` (AES-256 Encrypted)

---

## 1. Release Overview

Lattirune MVP 1.0 marks the completion of the core tactical auto-battler roguelite loop, incorporating spatial grid optimization, directional elemental conduits, compounding chain reactions, a 10-floor dungeon, full in-run economy, persistent meta-progression via the Blueprint Forge, and an integrated mobile screen flow.

---

## 2. Major Systems & Features

### Core Spatial Combat & Lattice Grid
- **5×5 Lattice Grid:** 17 active tiles and 8 locked border tiles with strict spatial footprint checking.
- **Directional Energy Conduits:** Directional runes emit continuous energy beams along cardinal axes powering weapons in line.
- **Optical Prism & Crossfire:** Refraction splitters and 4-way omni emitters powering multi-directional grid setups.
- **Master Combinations & Chain Reactions:** 5 named master synergies overriding broad category rules; queue-based cascading trigger resolution with depth limit $N \le 4$.
- **Combat Simulation Agency:** Speed toggles ($1.0\times, 2.0\times, 3.0\times$) and manual emergency potion drinking during battle.

### Dungeon & Progression Architecture
- **10-Floor Cursed Sewers:** Complete biome progression featuring Sewer Skirmishes, Goblin Ambushes, Slime Caverns, Armory Gates, Treasure Vaults, Bone Crypts, Spider Nests, and the Boss Sanctum.
- **6-Enemy Bestiary & 3-Phase Lich Lord:** Unique mechanics including swarm attack speed, gold theft, physical damage reflection, ticking poison, bag slot disabling, minion summoning, and dynamic enrage thresholds.
- **In-Run Economy:** Mob gold drops, Floor 4 & 9 Merchant Stalls, and Floor 8 Campfire Rest Site.
- **Procedural Bag Inventory:** 6 starting cells expanding sequentially to 16 maximum cells.
- **Meta-Progression Campfire Hub & Forge:** Persistent Dungeon Embers, 12 unlockable blueprints, and non-stacking start-of-run bonuses.

### Mobile Navigation & Persistence
- **Mobile Screen Controller:** Robust finite state machine managing Main Menu, Campfire Hub, Blueprint Forge, Grid Build, Combat, Rewards, Inventory, Merchant, Campfire Rest, Boss, Run Complete, and Settings.
- **Android Back Handling:** Context-aware routing with combat safety lock preventing accidental surrender.
- **Encrypted Local Persistence:** AES-256 encryption, SaveVersion = 1, and atomic file replacement with corrupted save backup recovery.

---

## 3. Release Artifacts

* **Release Candidate APK:** `Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk`
* **Production Versioned APK:** `Builds/Android/Lattirune-1.0.0.apk`
* **Package Identity:** `com.developer.lattirune`
* **Version Name:** `1.0.0`
* **Version Code:** `1`

---

## 4. Verification & QA Status

* **Automated EditMode Test Suite:** 425 / 425 tests passing ($100\%$ pass rate).
* **Compilation Errors:** 0
* **Console Errors:** 0
* **Android Build Status:** PASS
* **Security Audit:** PASS (Zero secrets, private keys, or API credentials stored in repository; `.gitignore` properly excludes build outputs and keystores).

---

## 5. Physical Android QA

* **Device:** `NOT AVAILABLE`
* **Installation:** `NOT TESTED`
* **Smoke Flow:** `NOT TESTED`
* **Touch:** `NOT TESTED`
* **Android Back:** `NOT TESTED`
* **Save/Load:** `NOT TESTED`
* **Audio:** `NOT TESTED`
* **Haptics:** `NOT TESTED`
* **Lifecycle:** `NOT TESTED`
* **Offline:** `NOT TESTED`
* **Performance:** `NOT TESTED`
* **Overall Status:** `NOT TESTED`

---

## 6. Manual QA Status

* **Automated Regression:** `425 / 425 PASS`
* **Android Device Testing:** `NOT TESTED`
* **Clean APK Installation:** `NOT TESTED`
* **Cold Launch & Resume:** `NOT TESTED`
* **Physical Touch & Drag Ergonomics:** `NOT TESTED`
* **Device Audio Output:** `NOT TESTED`
* **Device Vibration / Haptics:** `NOT TESTED`
* **Thermal & Battery Profiling:** `NOT TESTED`
* **Manual QA Checklist Reference:** [`Docs/MVP1.0-Manual-QA-Checklist.md`](./MVP1.0-Manual-QA-Checklist.md)

---

## 7. Known Limitations & Future Scope

* **Physical Device Testing:** Physical hardware testing on Android retail devices (Samsung Galaxy, Google Pixel) remains pending manual QA lab verification.
* **Google Play Publishing:** Store listing assets, signed AAB bundles, and Google Play Console submissions are scheduled for post-MVP Phase 4.
