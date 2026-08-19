# Lattirune MVP 1.0 External Action Tracker

**Date:** August 19, 2026  
**Application:** Lattirune  
**Target Package:** `com.developer.lattirune`  
**Version:** `1.0.0` (Build `1`)  
**Save Version:** `1` (AES-256 Encrypted)  
**Repository State:** `READY` (511 / 511 tests passing)  
**External Release Gate:** `BLOCKED`

---

## External Actions Register

| ID | Action | Description | Required Evidence | Status | Owner |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **EXT-01** | Android QA | Execute 26-point manual test checklist on physical hardware | Completed and signed-off QA checklist ([`Docs/MVP1.0-Manual-QA-Checklist.md`](./MVP1.0-Manual-QA-Checklist.md)) | `BLOCKED` | QA Team / Hardware Lab |
| **EXT-02** | Privacy Hosting | Host privacy policy document on a publicly accessible HTTPS web page | Active public HTTPS URL serving [`Docs/MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md) | `BLOCKED` | Publisher / Web Ops |
| **EXT-03** | App Icon | Provide production high-resolution application icon | $512 \times 512$ 32-bit PNG asset complying with [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md) | `BLOCKED` | Art / UI Designer |
| **EXT-04** | Feature Graphic | Create marketing promotional banner for Google Play Store | $1024 \times 500$ 24-bit PNG/JPEG asset complying with [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md) | `BLOCKED` | Marketing / Art Team |
| **EXT-05** | Screenshots | Capture 12 portrait screenshots from live application screens | 12 clean portrait captures ($1080 \times 1920$ PNG/JPEG) complying with [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./MVP1.0-Screenshot-Capture-Plan.md) | `BLOCKED` | QA / Marketing Team |
| **EXT-06** | AAB Generation | Run Unity build pipeline to produce the release App Bundle | Actual `Builds/Android/Lattirune-1.0.0.aab` binary artifact generated via `BuildProductionAAB()` | `BLOCKED` | Build Engineer / CI |
| **EXT-07** | Signing | Cryptographically sign the production AAB using release credentials | Secure signed `.aab` package with production keystore credentials supplied outside Git | `BLOCKED` | Release Engineer |
| **EXT-08** | Play Submission | Complete Google Play Console release track setup and publish | Google Play Console submission evidence with active rollout status | `BLOCKED` | Publisher Account Admin |

---

## Action Verification Notes

- **EXT-01 (Android QA):** Cannot be verified using automated test runners; requires a physical Android device running Android 7.0+ (API Level 24+).
- **EXT-02 (Privacy Hosting):** Must be hosted prior to submission in Google Play Console Data Safety & App Content sections.
- **EXT-03 to EXT-05 (Store Visuals):** Required mandatory graphics for Google Play Store listing publication.
- **EXT-06 & EXT-07 (Binary & Signing):** AAB must be built from the clean repository state and signed using an isolated keystore.
- **EXT-08 (Play Submission):** Final manual step after EXT-01 through EXT-07 are completed.
