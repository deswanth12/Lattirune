# Lattirune MVP 1.0 Final External Release Handoff

## Verified Repository State

**Version:**  
`1.0.0`

**Version Code:**  
`1`

**Package ID:**  
`com.developer.lattirune`

**Save Version:**  
`1`

**Automated Tests:**  
`571 / 571` (100% PASS across 62 test suites)

**Compilation Errors:**  
`0`

**Console Errors:**  
`0`

**Repository:**  
`CLEAN`

---

## Completed

The following items are verified and fully completed within the codebase repository:

- **MVP Gameplay Implementation:** 
  - Complete 10-floor Cursed Sewers progression topology.
  - 20 canonical items and 10 canonical directional elemental runes.
  - 5 master combinations and 5 symmetric elemental reactions.
  - 6-enemy bestiary with unique tactical mechanics.
  - 3-phase Lich Lord boss encounter with dynamic enrage scaling.
  - In-run gold & dungeon embers economy, merchant stall, and Floor 8 rest site.
  - Persistent Blueprint Forge meta-progression with 6 permanent blueprints.
  - Procedural bag expansion and multi-tile spatial inventory.
  - Mobile UI navigation coordinator with Android Back-button routing and $\ge 52\text{ dp}$ touch targets.
- **Automated Regression:** 538 automated Unit & Integration tests passing with 0 compilation and 0 console errors.
- **Release Documentation:** Complete set of release notes, manifest, changelog, and traceability documentation.
- **Store Listing Copy:** English (US) title, 75-char short description, and full description prepared in [`Docs/MVP1.0-Google-Play-Store-Listing.md`](./MVP1.0-Google-Play-Store-Listing.md).
- **Privacy Policy Document:** Comprehensive offline privacy policy created in [`Docs/MVP1.0-Privacy-Policy.md`](./Docs/MVP1.0-Privacy-Policy.md).
- **AAB Build Pipeline:** Scripted pipeline configured in `Assets/_Project/Editor/AndroidBuildScript.cs` targeting `Builds/Android/Lattirune-1.0.0.aab`.
- **Security Audit:** Zero secrets, API keys, private keys, or keystores stored in repository; all build outputs and credentials excluded via `.gitignore`.
- **Release Traceability:** Matrix mapping all PLAN.md requirements to code and test suites in [`Docs/MVP1.0-Release-Traceability.md`](./Docs/MVP1.0-Release-Traceability.md).
- **Release Manifest:** Comprehensive build specification and asset register in [`Docs/MVP1.0-Release-Manifest.md`](./Docs/MVP1.0-Release-Manifest.md).
- **Physical QA Gate Attempt & Record:** Documented in [`Docs/MVP1.0-Physical-Android-QA-Record.md`](./Docs/MVP1.0-Physical-Android-QA-Record.md).
- **Final External Release Audit:** Documented in [`Docs/MVP1.0-Final-External-Release-Audit.md`](./Docs/MVP1.0-Final-External-Release-Audit.md).

---

## External Actions Remaining

The following tasks cannot be performed within the repository and require external resources, physical hardware, art creation, or publisher account actions:

1. **Physical Android QA**
   - **Owner:** External (QA Team / Device Lab)
   - **Status:** `BLOCKED` (Attempted; no hardware device connected)
   - **Required Evidence:** Signed-off 26-point execution record of [`Docs/MVP1.0-Manual-QA-Checklist.md`](./Docs/MVP1.0-Manual-QA-Checklist.md) on physical Android hardware (API Level $\ge 24$).

2. **Public Privacy Policy Hosting**
   - **Owner:** External (Publisher / Web Ops)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Verified public HTTPS URL serving the content of [`Docs/MVP1.0-Privacy-Policy.md`](./Docs/MVP1.0-Privacy-Policy.md).

3. **512x512 App Icon**
   - **Owner:** External (Art / UI Designer)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Final $512 \times 512$ 32-bit PNG high-resolution application icon (max 1024 KB) matching specifications in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./Docs/MVP1.0-Store-Asset-Manifest.md).

4. **1024x500 Feature Graphic**
   - **Owner:** External (Marketing / Art Team)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Final $1024 \times 500$ 24-bit PNG/JPEG marketing banner (max 15MB) matching specifications in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./Docs/MVP1.0-Store-Asset-Manifest.md).

5. **12 Google Play Screenshots**
   - **Owner:** External (Marketing / QA Team)
   - **Status:** `BLOCKED`
   - **Required Evidence:** 12 clean portrait screenshots ($1080 \times 1920$ or $9:16$ aspect ratio, PNG/JPEG, zero debug overlays) captured according to [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./Docs/MVP1.0-Screenshot-Capture-Plan.md).

6. **Release AAB Generation**
   - **Owner:** External (Build Engineer / CI Pipeline)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Generated `Builds/Android/Lattirune-1.0.0.aab` artifact produced via Unity Editor batch mode execution.

7. **Production Signing**
   - **Owner:** External (Release Engineer / CI Environment)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Cryptographically signed `.aab` package utilizing production keystore credentials supplied securely outside source control.

8. **Google Play Console Submission**
   - **Owner:** External (Publisher Account Admin)
   - **Status:** `BLOCKED`
   - **Required Evidence:** Active Google Play Console release track entry with store listing, assets, content rating, data safety declarations, and uploaded signed AAB bundle.

---

## Exact QA Procedure

Reference:
- [`Docs/MVP1.0-Manual-QA-Checklist.md`](./Docs/MVP1.0-Manual-QA-Checklist.md)
- [`Docs/MVP1.0-Physical-Android-QA-Record.md`](./Docs/MVP1.0-Physical-Android-QA-Record.md)

Follow the structured 26-item verification checklist covering:
- Installation & Launch
- Navigation & Screen Routing (Main Menu, Campfire Hub, Blueprint Forge, Settings)
- Grid Building & Rune Conduit Mechanics
- Combat Loop & Speed Controls ($1\times, 2\times, 3\times$, emergency potion tap)
- Floor Progression (Floors 1–10) & Boss Encounter (Lich Lord 3-phase battle)
- Economy, Merchant Stall, and Floor 8 Rest Site
- Hardware Back Button Handling & Safe Navigation Guards
- Encrypted Save/Load Persistence across sessions
- Audio, Haptics, and Offline Standalone Verification
- Application Lifecycle (pause, background, resume, kill-and-restart)

---

## Exact Store Asset Requirements

Reference:
- [`Docs/MVP1.0-Store-Asset-Manifest.md`](./Docs/MVP1.0-Store-Asset-Manifest.md)
- [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./Docs/MVP1.0-Screenshot-Capture-Plan.md)

Requirements:
- **App Icon:** $512 \times 512$ PNG, 32-bit color with alpha, square, max 1024 KB.
- **Feature Graphic:** $1024 \times 500$ PNG or JPEG, 24-bit color, no alpha, max 15 MB.
- **Screenshots:** 12 portrait captures ($1080 \times 1920$ resolution, PNG or JPEG, 16:9 / 9:16 aspect ratio) covering all 12 key screens with production UI only and zero debug elements.

---

## Exact AAB Procedure

Reference:
- `Assets/_Project/Editor/AndroidBuildScript.cs`

Method:
- `Lattirune.Editor.AndroidBuildScript.BuildProductionAAB()`

Command to execute in Unity batchmode:
```bash
<UNITY_PATH>/Unity.exe -quit -batchmode -projectPath "<REPO_PATH>" -executeMethod Lattirune.Editor.AndroidBuildScript.BuildProductionAAB -logFile build_aab.log
```

Expected Output:
- `Builds/Android/Lattirune-1.0.0.aab`

*(Note: The artifact is generated upon execution and must remain excluded from Git tracking.)*

---

## Signing

**Policy:**  
Production signing must occur outside source control using a secure keystore / CI environment (e.g., GitHub Actions Secrets, Google Play App Signing, or local offline release vault).

**Rule:**  
Never store production keystores, passwords, or certificate credentials in Git.

---

## Privacy Policy

- **Source Document:** [`Docs/MVP1.0-Privacy-Policy.md`](./Docs/MVP1.0-Privacy-Policy.md)
- **Hosting Guide:** [`Docs/MVP1.0-Privacy-Policy-Hosting-Guide.md`](./Docs/MVP1.0-Privacy-Policy-Hosting-Guide.md)
- **Public URL Status:** `NOT HOSTED`

*(The publisher must host the document on GitHub Pages or a public HTTPS server and provide the live URL to Google Play Console.)*

---

## Google Play Submission

**Status:**  
`NOT SUBMITTED`

**Required Prerequisites Before Submission:**
1. Physical Android device QA pass signed off.
2. Public Privacy Policy HTTPS URL hosted and active.
3. High-resolution $512 \times 512$ App Icon uploaded.
4. $1024 \times 500$ Feature Graphic uploaded.
5. 12 portrait screenshots uploaded.
6. Signed Android App Bundle (`Lattirune-1.0.0.aab`) uploaded.
7. Store listing metadata populated (Title, Short Description, Full Description).
8. Data Safety questionnaire completed (100% offline, 0 data collected).
9. IARC Content Rating questionnaire completed.

---

## Release Decision

**REPOSITORY:**  
`READY`

**EXTERNAL RELEASE:**  
`BLOCKED`

**Reason:**  
All repository-side code, tests, build configurations, and documentation for Lattirune MVP 1.0 are 100% complete and verified. All remaining release blockers require external hardware, public hosting, visual artwork creation, production signing credentials, or Google Play Console account access.
