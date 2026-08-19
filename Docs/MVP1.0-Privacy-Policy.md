# Privacy Policy for Lattirune

**Last Updated:** August 19, 2026  
**Application:** Lattirune  
**Package Identifier:** `com.developer.lattirune`  
**Publisher:** Developer (*Publisher confirmation required*)  
**Contact:** *Contact information must be supplied by the publisher before publication.*

---

## 1. Introduction

This Privacy Policy explains how Lattirune ("the Game") handles information when you play the application on Android, iOS, or other supported platforms.

Lattirune is built from the ground up as a standalone, offline, single-player tactical auto-battler roguelite game. The application does not collect, transmit, sell, or share any personally identifiable information (PII).

---

## 2. Information Storage and Usage

### A. Local-Only Game Data
All gameplay data is stored strictly on your local device within the application's isolated OS sandbox directory. This includes:
* **In-Run Progression:** Current dungeon floor, encounter state, in-run gold balance, hero health, and equipped items/runes on the $5 \times 5$ lattice grid.
* **Spatial Backpack Inventory:** Unlocked bag coordinates and unplaced equipment inventory.
* **Persistent Meta-Progression:** Persistent Dungeon Embers currency balance, unlocked Blueprints in the Blueprint Forge, and lifetime boss clear statistics.
* **Player Settings:** Master audio volume, SFX volume, and haptic vibration feedback toggles.

### B. Local Data Encryption
All local save data is encrypted at rest using AES-256 standard encryption (`SaveVersion = 1`) with authenticated HMAC payload verification to protect against file corruption.

### C. Zero Data Transmission
Lattirune makes **zero** outbound network requests. No gameplay data, telemetry, device identifiers, IP addresses, or personal information are ever transmitted to external servers or cloud services.

---

## 3. Third-Party Services & SDKs

The Game contains:
* **No Advertising SDKs:** Zero integration with AdMob, Unity Ads, or third-party ad networks.
* **No Analytics / Telemetry:** Zero integration with Firebase, Google Analytics, Adjust, AppsFlyer, or crash-reporting services.
* **No In-App Purchases (IAP):** Zero integration with Google Play Billing or real-money payment processors.
* **No User Accounts:** No login, registration, email collection, or social network linking.

---

## 4. Children’s Privacy

Lattirune does not collect personal information from anyone, including children under the age of 13. The game contains mild stylized fantasy combat with zero data collection.

---

## 5. Permissions

Lattirune requests only minimal standard device hardware permissions:
* **`VIBRATE`:** Used solely to trigger tactile haptic rumble feedback during gameplay interactions (can be disabled at any time in Settings).
* **No Network Permissions:** `INTERNET` and `ACCESS_NETWORK_STATE` are not requested.
* **No Dangerous Permissions:** No camera, microphone, contacts, storage, or precise location access is requested.

---

## 6. Hosting & Publisher Notice

> [!IMPORTANT]
> **Publisher Note:** A publicly accessible URL hosting this privacy policy must be established prior to submitting Lattirune to the Google Play Store or Apple App Store.
