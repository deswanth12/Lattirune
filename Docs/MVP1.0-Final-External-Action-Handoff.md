# Lattirune MVP 1.0 Final External Action Handoff

**Date:** August 19, 2026  
**Application:** Lattirune  
**Version:** `1.0.0`  
**Version Code:** `1`  
**Package ID:** `com.developer.lattirune`  
**Save Version:** `1`  
**Task:** TASK-048 — Final MVP 1.0 External Action Handoff

---

## Release State

| Dimension | Status |
| :--- | :--- |
| **Repository** | `COMPLETE` |
| **Automated QA** | `PASS (589 / 589)` |
| **Security Audit** | `PASS` |
| **External Release** | `BLOCKED` |

> [!IMPORTANT]
> All 8 external blockers must be resolved before Google Play submission. The repository requires no further engineering changes for these external actions.

---

## Action Matrix

| ID | Action | Current Status | Required Evidence | Dependency |
| :--- | :--- | :--- | :--- | :--- |
| **EXT-01** | Physical Android QA | `BLOCKED` | 26/26 physical checklist items PASS — signed off in [`Docs/MVP1.0-Physical-Android-QA-Record.md`](./MVP1.0-Physical-Android-QA-Record.md) | Physical Android device (API ≥ 24) |
| **EXT-02** | Privacy Policy URL | `BLOCKED` | Verified public HTTPS URL serving [`Docs/MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md) | Web hosting / GitHub Pages |
| **EXT-03** | App Icon | `BLOCKED` | `Assets/Icon_512x512.png` — final $512 \times 512$ 32-bit PNG | Art / UI Designer |
| **EXT-04** | Feature Graphic | `BLOCKED` | `Assets/FeatureGraphic_1024x500.png` — final $1024 \times 500$ 24-bit PNG/JPEG | Marketing / Art Team |
| **EXT-05** | Screenshots | `BLOCKED` | 12 portrait captures in `Screenshots/` complying with [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./MVP1.0-Screenshot-Capture-Plan.md) | Physical device or Unity Editor |
| **EXT-06** | AAB | `BLOCKED` | `Builds/Android/Lattirune-1.0.0.aab` — generated via `BuildProductionAAB()` | Unity Editor batch mode runtime |
| **EXT-07** | Signing | `BLOCKED` | Secure signed `.aab` from isolated production keystore in CI/CD — never stored in git | Secure CI/CD environment |
| **EXT-08** | Play Submission | `BLOCKED` | Active Google Play Console release track entry with all assets uploaded | EXT-01 through EXT-07 complete |

---

## Recommended Execution Order

The following order minimises iteration cost and satisfies all dependencies:

### Step 1 — EXT-03: App Icon
* **Why first:** No dependencies on any other blocker. Can be created independently on any workstation.
* **Action:** Art team to export final $512 \times 512$ 32-bit PNG per spec in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md).
* **Deliver to:** `Assets/Icon_512x512.png` in the repository.

### Step 2 — EXT-04: Feature Graphic
* **Why second:** No dependencies. Parallel to EXT-03 if multiple designers are available.
* **Action:** Marketing team to create final $1024 \times 500$ PNG/JPEG banner per spec in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md).
* **Deliver to:** `Assets/FeatureGraphic_1024x500.png` in the repository.

### Step 3 — EXT-02: Privacy Policy Hosting
* **Why third:** No dependency on device hardware. Requires web hosting account only.
* **Action:** Deploy `Docs/MVP1.0-Privacy-Policy.md` to a public HTTPS domain per [`Docs/MVP1.0-Privacy-Policy-Hosting-Guide.md`](./MVP1.0-Privacy-Policy-Hosting-Guide.md).
* **Verify:** Curl or browser-confirm the URL returns 200 OK with correct content.
* **Record:** Verified URL in release documentation.

### Step 4 — EXT-01: Physical Android QA
* **Why fourth:** Requires the release APK which is already available. No dependency on AAB.
* **Action:** Connect Android device (API ≥ 24), install `Builds/Android/Lattirune-1.0.0.apk` via ADB, execute all 26 items in [`Docs/MVP1.0-Manual-QA-Checklist.md`](./MVP1.0-Manual-QA-Checklist.md).
* **Record:** Complete execution results in [`Docs/MVP1.0-Physical-Android-QA-Record.md`](./MVP1.0-Physical-Android-QA-Record.md).

### Step 5 — EXT-05: Screenshots
* **Why fifth:** Can be captured during or after EXT-01 physical QA session using the same device.
* **Action:** Capture 12 portrait screenshots per [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./MVP1.0-Screenshot-Capture-Plan.md). Save to `Screenshots/`.

### Step 6 — EXT-06: AAB Generation
* **Why sixth:** Requires Unity Editor available on a build machine. Order not critical relative to EXT-01–05, but must precede EXT-07/08.
* **Action:** Execute `BuildProductionAAB()` in Unity Editor batch mode.
  ```bash
  <UNITY_PATH>/Unity.exe -quit -batchmode -projectPath "<REPO_PATH>" \
    -executeMethod Lattirune.Editor.AndroidBuildScript.BuildProductionAAB \
    -logFile build_aab.log
  ```
* **Expected output:** `Builds/Android/Lattirune-1.0.0.aab`
* **Important:** The AAB must NOT be committed to git.

### Step 7 — EXT-07: Production Signing
* **Why seventh:** Requires the AAB from EXT-06.
* **Action:** Sign the AAB using a secure production keystore in CI/CD (e.g., GitHub Actions Secrets, Google Play App Signing). **Never commit the keystore to git.**
* **Verify:** Signed APK/AAB validates with `apksigner verify`.

### Step 8 — EXT-08: Google Play Console Submission
* **Why last:** All prerequisites must be complete.
* **Action:** Publisher to upload signed AAB, store listing copy, privacy URL, app icon, feature graphic, and 12 screenshots to Google Play Console. Complete content rating and data safety questionnaires.
* **Evidence:** Active release track entry visible in Play Console.

---

## Definition of Done

The MVP 1.0 release is **READY** only when all of the following are confirmed:

| Gate | Condition |
| :--- | :--- |
| **Physical QA** | 26/26 manual checklist items PASS on physical hardware |
| **Privacy URL** | Verified public HTTPS URL active and confirmed |
| **App Icon** | $512 \times 512$ PNG accepted by Play Console upload |
| **Feature Graphic** | $1024 \times 500$ PNG/JPEG accepted by Play Console upload |
| **12 Screenshots** | All 12 portrait captures uploaded and accepted |
| **AAB** | `Builds/Android/Lattirune-1.0.0.aab` generated and verified |
| **Production Signing** | AAB signed with secure production keystore |
| **Play Submission** | Release track live in Google Play Console |

> [!CAUTION]
> Until every row above is confirmed PASS, **RELEASE STATUS = BLOCKED**. Do not claim submission or release without direct publisher evidence.
