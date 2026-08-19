# Lattirune MVP 1.0 Google Play Submission Readiness Checklist

**Date:** August 19, 2026  
**Application:** Lattirune  
**Target Package:** `com.developer.lattirune`  
**Version:** `1.0.0` (Version Code `1`)  
**Save Version:** `1` (AES-256 Encrypted Local Persistence)  
**Status Key:** `PASS` | `FAIL` | `NOT VERIFIED` | `NOT APPLICABLE` | `BLOCKED`

---

## 1. App Identity
* **App Name:** Lattirune (`PASS`)
* **Package Identifier:** `com.developer.lattirune` (`PASS`)
* **Default Language:** English (United States) (`PASS`)
* **App Category:** Games / Roguelite & Strategy (`PASS`)

## 2. Version Information
* **Version Name:** `1.0.0` (`PASS`)
* **Version Code:** `1` (`PASS`)
* **Save Schema Version:** `1` (`PASS`)
* **Consolidated Version Declarations:** Single source of truth in `PlayerSettings` and `SaveVersion.cs` (`PASS`)

## 3. Android Configuration
* **Default Orientation:** Portrait ($1080 \times 1920$ reference canvas) (`PASS`)
* **Autorotation:** Restricted strictly to portrait; landscape disabled (`PASS`)
* **Minimum API Level:** Android 7.0 (API Level 24) (`PASS`)
* **Target API Level:** Android 14 (API Level 34) (`PASS`)
* **Touch Target Standard:** All interactive UI elements satisfy $\ge 52\text{ dp}$ (`PASS`)

## 4. Release Artifacts
* **Development / Verification APK:** `Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk` (`PASS`)
* **Production Versioned APK:** `Builds/Android/Lattirune-1.0.0.apk` (`PASS`)
* **Android App Bundle (.aab):** Not configured in build pipeline (`BLOCKED` for Play Store Production Track)
* **Artifact Tracking in Git:** Excluded via `.gitignore` (`PASS`)

## 5. Store Listing
* **Short Description:** Prepared (76 characters, max 80) (`PASS`)
* **Full Description:** Prepared with accurate MVP 1.0 feature set (`PASS`)
* **Draft Location:** [`Docs/MVP1.0-Google-Play-Store-Listing.md`](./MVP1.0-Google-Play-Store-Listing.md) (`PASS`)

## 6. App Description Review
* **Accurate Feature Description:** Grid building, 20 items, 10 runes, 6 enemies, Lich Lord, Blueprint Forge (`PASS`)
* **Unsupported Feature Claims:** Zero claims of online multiplayer, cloud saves, or paid loot (`PASS`)

## 7. Graphics Requirements
* **App Icon:** $512 \times 512$ PNG (32-bit color) (`NOT VERIFIED` - pending art asset finalization)
* **Feature Graphic:** $1024 \times 500$ PNG/JPEG (`NOT VERIFIED` - pending marketing asset package)
* **Screenshots:** Minimum 4 portrait screenshots ($1080 \times 1920$) (`NOT VERIFIED` - pending device capture)

## 8. App Access
* **Login / Authentication:** None required (`PASS`)
* **Special Credentials:** No restricted access, paywalls, or geo-blocking (`PASS`)
* **Declaration:** "All functionality is available without special access" (`PASS`)

## 9. Content Rating
* **Questionnaire Preparation Document:** [`Docs/MVP1.0-Content-Rating-Preparation.md`](./MVP1.0-Content-Rating-Preparation.md) (`PASS`)
* **Violence:** Mild fantasy combat (non-realistic animations, no gore) (`PASS`)
* **Gambling / Real Money:** None (`PASS`)
* **User-Generated Content:** None (`PASS`)
* **Final Rating Status:** Must be completed through Google Play Console questionnaire (`NOT VERIFIED`)

## 10. Data Safety
* **Personal Data Collection:** None (`PASS`)
* **Data Transmission:** Zero network transmission (`PASS`)
* **Third-Party SDKs:** Zero analytics, advertising, or tracking libraries (`PASS`)
* **Local Storage:** Encrypted JSON save file stored solely in app sandbox (`PASS`)
* **Data Safety Questionnaire:** Must be confirmed manually in Play Console (`NOT VERIFIED`)

## 11. Privacy Policy Requirement
* **Public URL:** A publicly accessible privacy policy URL must be hosted before production release (`BLOCKED`)
* **Offline Evidence:** Codebase contains zero telemetry, tracking, or network requests (`PASS`)

## 12. Ads Declaration
* **AdMob / Unity Ads:** Not present in codebase (`PASS`)
* **Declaration:** "No, my app does not contain ads" (`PASS`)

## 13. In-App Purchases & Monetization
* **Google Play Billing / IAP:** Not present in codebase (`PASS`)
* **Declaration:** Free download with zero in-app purchases or pay-to-win microtransactions (`PASS`)

## 14. Permissions
* **Dangerous Permissions:** None requested (`PASS`)
* **Vibrate Permission:** Standard vibration for haptic feedback (`PASS`)
* **Network Permissions:** `INTERNET` and `ACCESS_NETWORK_STATE` not declared (`PASS`)

## 15. Target SDK & Google Play Compliance
* **64-bit Architecture:** ARM64 / IL2CPP compliant (`PASS`)
* **Target SDK:** API Level 34+ compliant (`PASS`)

## 16. Release Signing
* **Production Keystore:** Must remain outside git repository (`PASS`)
* **Play App Signing:** Recommended for production AAB submission (`NOT VERIFIED`)

## 17. Testing Tracks
* **Internal Testing Track:** Recommended for first device verification (`PASS`)
* **Closed Testing Track:** Recommended for 14-day 20-tester testing requirement (`PASS`)
* **Production Track:** Target for full release (`BLOCKED` pending testing tracks)

## 18. Manual QA Dependency
* **Automated Unit & Integration Tests:** 425 / 425 PASS (`PASS`)
* **Physical Android Device Verification:** Pending hardware lab execution (`BLOCKED`)

## 19. Release Blockers Table
| Blocker ID | Description | Severity | Remediation Requirement |
| :--- | :--- | :--- | :--- |
| **BLK-001** | Physical Android device QA not completed | `CRITICAL` | Execute manual smoke checklist on physical hardware |
| **BLK-002** | Android App Bundle (.aab) pipeline not configured | `CRITICAL` | Configure AAB export in `AndroidBuildScript.cs` |
| **BLK-003** | Public Privacy Policy URL not hosted | `REQUIRED` | Host static policy URL for Play Console listing |
| **BLK-004** | Store listing graphic assets not finalized | `REQUIRED` | Provide 512×512 icon, 1024×500 feature graphic, and screenshots |

## 20. Final Submission Checklist
* [x] Core gameplay, 10 floors, 20 items, 10 runes, 6 enemies, Lich Lord implemented and verified.
* [x] 425 automated regression tests passing with 0 compilation and console errors.
* [x] Package identity (`com.developer.lattirune`), version name (`1.0.0`), and version code (`1`) unified.
* [x] Zero ads, zero IAP, zero analytics, zero network permissions.
* [ ] Physical Android device smoke test executed and signed off.
* [ ] Android App Bundle (.aab) generated and signed.
* [ ] Google Play Console store listing, graphics, and privacy URL submitted.
