# Lattirune MVP 1.0 Manual QA Readiness Checklist

**Date:** August 19, 2026  
**Application:** Lattirune  
**Package Identifier:** `com.developer.lattirune`  
**Version:** `1.0.0` (Build `1`)  
**Save Version:** `1` (AES-256 Encrypted)  
**Target Hardware:** Android 10+ (Portrait $1080 \times 1920$, $\ge 52\text{ dp}$ Touch Targets)  
**Status Key:** `PASS` | `FAIL` | `BLOCKED` | `NOT TESTED`

---

## 1. Automated vs Physical Device Verification Matrix

| Verification Area | Automated Coverage | Physical Device Required | Current QA Status |
| :--- | :--- | :--- | :--- |
| **Lattice Grid ($5 \times 5$)** | 10 Unit Tests (`LatticeGridTests.cs`) | Touch drag feel & tile snap response | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Directional Rune Conduits** | 15 Engine Tests (`RuneConduitEngineTests.cs`) | Visual beam aura rendering | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Prism & Crossfire Runes** | 18 Tests (`PrismConduitTests.cs`, `CrossfireConduitTests.cs`) | GPU multi-beam rendering | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **20 Canonical Items** | 8 Tests (`ItemCatalogue20Tests.cs`) | UI card layout and tooltip inspection | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **10 Canonical Runes** | 11 Tests (`RuneCatalogue10Tests.cs`) | Icon readability at $1080 \times 1920$ | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **5 Master Synergies** | 14 Tests (`MasterSynergyAndChainReactionTests.cs`) | Audio & visual particle explosion feedback | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **5 Elemental Reactions** | 15 Tests (`ElementalReactionTests.cs`) | Frame rate stability during reaction cascades | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Damage Formula & Speeds** | 14 Tests (`DamageCalculatorTests.cs`, `CombatSystemTests.cs`) | $1\times, 2\times, 3\times$ animation pacing feel | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **6 Enemies & Bestiary** | 7 Tests (`EnemyBestiaryTests.cs`) | Enemy attack timing and animation loops | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **3-Phase Lich Lord Boss** | 14 Tests (`BossSystemTests.cs`, `BossPhaseTests.cs`) | Phase transition cinematic pacing | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **10-Floor Dungeon Loop** | 12 Tests (`TenFloorDungeonProgressionTests.cs`) | Floor transition responsiveness | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **In-Run Economy & Stalls** | 20 Tests (`CombatAgencyAndEconomyTests.cs`) | Purchase button touch hitboxes | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Procedural Bag ($4 \times 4$)** | 16 Tests (`InventoryGridTests.cs`, `InventoryExpansionTests.cs`) | Multi-finger drag & inventory scrolling | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **3-Card Reward Draft** | 9 Tests (`RewardGeneratorTests.cs`, `RewardServiceTests.cs`) | Touch responsiveness on card selection | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Campfire Meta-Hub & Forge** | 36 Tests (`MetaProgressionTests.cs`, `MetaProgressionUITests.cs`) | Forge button tap accuracy | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Mobile Screen Navigation** | 14 Tests (`MobileUIFlowTests.cs`) | Android Hardware Back button transitions | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Encrypted Persistence** | 19 Tests (`SaveSystemTests.cs`, `SaveValidatorTests.cs`) | Storage write permissions & OS sandbox | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **Audio & Haptic Feedback** | 10 Tests (`AudioControllerTests.cs`, `HapticFeedbackTests.cs`) | Real device speaker output & motor rumble | `AUTOMATED VERIFIED` / `DEVICE PENDING` |
| **APK Installation & Launch** | Build Pipeline (`AndroidBuildScript.cs`) | Package manager install & cold launch time | `NOT TESTED` |
| **Thermal & Memory Stress** | Target Metrics ($<180\text{ MB}, 60\text{ FPS}$) | Real device 30-minute play session profiling | `NOT TESTED` |

---

## 2. Target Device Test Matrix

| Device Model | Android Version | RAM | Screen Resolution | APK Install | Cold Launch | Touch / Drag | HW Back | Audio | Haptics | Save/Load | 60 FPS Thermal | Overall Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Google Pixel 7** | Android 14 | 8 GB | $1080 \times 2400$ | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` |
| **Samsung Galaxy S22** | Android 13 | 8 GB | $1080 \times 2340$ | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` |
| **Xiaomi Redmi Note 11** | Android 11 | 4 GB | $1080 \times 2400$ | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` |
| **Samsung Galaxy A14** | Android 13 | 4 GB | $1080 \times 2408$ | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` | `NOT TESTED` |

---

## 3. Manual QA Test Cases

### Area 1: Installation & Launch
* **`INSTALL-001` (Clean APK Installation)**
  * **Preconditions:** Android device in Developer Mode with Unknown Sources enabled.
  * **Steps:** Run `adb install Builds/Android/Lattirune-1.0.0.apk`.
  * **Expected Result:** Package `com.developer.lattirune` installs cleanly with zero signature or permission errors.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`LAUNCH-001` (Cold Launch from Launcher)**
  * **Preconditions:** APK installed cleanly.
  * **Steps:** Tap Lattirune app icon on phone home screen.
  * **Expected Result:** App boots to Main Menu within $<2.0\text{s}$ in portrait mode ($1080 \times 1920$).
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`LAUNCH-002` (Force-Stop and Relaunch)**
  * **Preconditions:** App running on device.
  * **Steps:** Swipe away app in Android task manager and relaunch.
  * **Expected Result:** App cold launches cleanly without stale cache corruption.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 2: Persistence & Progression
* **`SAVE-001` (Encrypted Mid-Run Restoration)**
  * **Preconditions:** Player has started a run and reached Floor 3.
  * **Steps:** Quit app to home screen, kill app process, relaunch app, tap "Continue Run".
  * **Expected Result:** Floor, Gold, Grid layout, and Bag items restore identically from AES-256 save file.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`SAVE-002` (Run Defeat / Reset Meta Persistence)**
  * **Preconditions:** Player has accumulated 50 persistent Embers and unlocked 1 Blueprint.
  * **Steps:** Enter dungeon, let hero perish in combat, return to Campfire Hub.
  * **Expected Result:** In-run Gold resets to 0; Dungeon Embers and unlocked Blueprints remain intact.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 3: Navigation & Hardware Back
* **`BACK-001` (Safe Screen Back Navigation)**
  * **Preconditions:** Player on Settings, Blueprint Forge, or Campfire Hub screen.
  * **Steps:** Press Android hardware/gesture back button.
  * **Expected Result:** Settings returns to caller; Forge returns to Hub; Hub returns to Main Menu.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`BACK-002` (Combat Back Safety Lock)**
  * **Preconditions:** Active battle in progress on Combat screen.
  * **Steps:** Press Android hardware back button repeatedly.
  * **Expected Result:** Back navigation is strictly blocked during combat; battle continues uninterrupted without state corruption.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 4: Touch & Ergonomics
* **`TOUCH-001` (Grid Item Drag & Drop)**
  * **Preconditions:** Grid Build screen open with items in backpack.
  * **Steps:** Touch and drag Rusty Dagger ($1 \times 1$) and Iron Broadsword ($1 \times 2$) onto $5 \times 5$ lattice.
  * **Expected Result:** Drag follow is smooth and responsive; snap to discrete coordinates is exact; invalid placements bounce back.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`TOUCH-002` (Minimum $\ge 52\text{ dp}$ Hitbox Validation)**
  * **Preconditions:** Any active screen with buttons or reward cards.
  * **Steps:** Tap buttons with thumbs at normal one-handed and two-handed grips.
  * **Expected Result:** Zero missed taps or misclicks across all screen sizes.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 5: Audio & Haptics
* **`AUDIO-001` (Sound Effects & Volume Sliders)**
  * **Preconditions:** Device media volume at 50%.
  * **Steps:** Tap buttons, place items, trigger combat hits, and adjust volume sliders in Settings.
  * **Expected Result:** SFX triggers without latency; mute toggle silences audio; settings persist across reboot.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`HAPTIC-001` (Tactile Vibration Feedback)**
  * **Preconditions:** Haptics enabled in Settings.
  * **Steps:** Place item on grid, make merchant purchase, trigger elemental reaction.
  * **Expected Result:** Device vibration motor produces crisp tactile impulse.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 6: App Lifecycle & Offline Mode
* **`LIFECYCLE-001` (App Backgrounding & Resumption)**
  * **Preconditions:** Player is in Grid Build screen.
  * **Steps:** Press Home button, open other apps for 2 minutes, return to Lattirune.
  * **Expected Result:** App resumes instantly without memory leaks, visual artifacts, or crash.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`LIFECYCLE-002` (Combat Interruption Handling)**
  * **Preconditions:** Active battle in progress.
  * **Steps:** Receive incoming phone call or trigger notification pull-down.
  * **Expected Result:** Battle state pauses or resumes cleanly without state loss.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`
* **`OFFLINE-001` (Airplane Mode Operation)**
  * **Preconditions:** Enable Airplane Mode (Wi-Fi and Cellular off).
  * **Steps:** Cold launch app, start run, defeat boss, re-enter Campfire Hub.
  * **Expected Result:** Complete 10-floor game functions 100% offline with zero network errors.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

### Area 7: Performance & Thermals
* **`PERFORMANCE-001` (30-Minute Continuous Session Profile)**
  * **Preconditions:** Low-to-mid range target device (e.g. Redmi Note 11).
  * **Steps:** Play through 10-floor Cursed Sewers run continuously for 30 minutes.
  * **Expected Result:** Sustained 60 FPS, peak memory $<180\text{ MB}$, draw calls $<25$, no aggressive thermal throttling.
  * **Actual Result:** Pending physical hardware test.
  * **Status:** `NOT TESTED`

---

## 4. End-to-End Release Smoke Path

The official manual smoke test path for QA testers:

1. **Install:** `adb install Builds/Android/Lattirune-1.0.0.apk`
2. **Launch:** Tap app icon $\rightarrow$ verify splash screen and Main Menu within $2.0\text{s}$.
3. **Hub & Forge:** Main Menu $\rightarrow$ Campfire Hub $\rightarrow$ Blueprint Forge $\rightarrow$ inspect 12 blueprints $\rightarrow$ Return to Hub.
4. **Start Run:** Tap "Start New Run" $\rightarrow$ enter Biome 1 Cursed Sewers.
5. **Floor 1 (Skirmish):** Place starting dagger and rune on $5 \times 5$ lattice $\rightarrow$ Start Battle $\rightarrow$ defeat Sewer Rat $\rightarrow$ choose reward from 3-card draft.
6. **Floors 2-3:** Advance through Drain Basin and Slime Cavern $\rightarrow$ test item rotation ($90^\circ$) and spatial inventory placement.
7. **Floor 4 (Merchant):** Purchase 1 item, 1 rune, and 1 bag slot expansion $\rightarrow$ verify Gold deducts accurately.
8. **Floors 5-7:** Defeat Armored Skeleton and Necromancer $\rightarrow$ test 2-beam elemental reactions (e.g. Steam, Superconductor).
9. **Floor 8 (Campfire Rest):** Choose Option A (Heal 40% HP) or Option B (Rune Reforge +2 power) $\rightarrow$ confirm single-selection locking.
10. **Floor 9 (Pre-Boss):** Defeat Venomous Spider swarm $\rightarrow$ prepare final grid layout.
11. **Floor 10 (Boss Sanctum):** Engage The Lich Lord (750 HP) $\rightarrow$ trigger Phase 2 Soul Harvest ($66\%$) and Phase 3 Necrotic Inversion ($33\%$) $\rightarrow$ defeat boss.
12. **Run Complete:** Receive $80-120$ persistent Dungeon Embers $\rightarrow$ view Victory screen $\rightarrow$ tap "Return to Campfire Hub" $\rightarrow$ confirm Embers balance updated in Forge.

---

## 5. Release Blocker Classification Rules

* **`BLOCKER` (Prevents Release):**
  * APK installation fails on target Android versions (Android 10+).
  * Application crashes on launch, during combat, or upon saving.
  * Save corruption causing lost run or meta-progression data.
  * Lich Lord boss encounter freezes or cannot be completed.
  * Android Back button causes unintended run surrender or crash.
  * Any security leak (committed private keys, tokens, or credentials).
* **`NON-BLOCKING` (Permitted for MVP 1.0 Release):**
  * Minor visual text clipping on rare non-standard aspect ratios ($>21:9$).
  * Slight animation frame jitter during non-gameplay screen transitions.
  * Minor sound volume balance variance between device speakers.
