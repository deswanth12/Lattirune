# Lattirune MVP 1.0 External Release Checklist

**Date:** August 19, 2026  
**Application:** Lattirune  
**Package ID:** `com.developer.lattirune`  
**Version:** `1.0.0` (Build `1`)  
**Save Version:** `1`  
**Purpose:** Step-by-step external action checklist for completing MVP 1.0 Google Play release

---

## EXT-01 — Physical Android QA

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (QA Team / Hardware Lab) |
| **Required Input** | Android device (API ≥ 24), ADB-enabled USB or wireless debug connection |
| **Exact Action** | `adb install -r "Builds/Android/Lattirune-1.0.0.apk"` then execute every item in [`Docs/MVP1.0-Manual-QA-Checklist.md`](./MVP1.0-Manual-QA-Checklist.md) |
| **Expected Output** | All 26 checklist items marked PASS; no crashes, ANRs, or blocking defects |
| **Verification Method** | Physically observe each test; record in [`Docs/MVP1.0-Physical-Android-QA-Record.md`](./MVP1.0-Physical-Android-QA-Record.md) |
| **Status** | `BLOCKED` |

---

## EXT-02 — Privacy Policy Hosting

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Publisher / Web Ops) |
| **Required Input** | Web hosting account (GitHub Pages, Netlify, or custom domain) |
| **Exact Action** | Deploy content of `Docs/MVP1.0-Privacy-Policy.md` to a public HTTPS endpoint following [`Docs/MVP1.0-Privacy-Policy-Hosting-Guide.md`](./MVP1.0-Privacy-Policy-Hosting-Guide.md) |
| **Expected Output** | Public HTTPS URL returning 200 OK with privacy policy content |
| **Verification Method** | `curl -I <URL>` returns 200; browser confirms correct content; no redirect loops |
| **Status** | `BLOCKED` |

---

## EXT-03 — App Icon

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Art / UI Designer) |
| **Required Input** | Art tool capable of exporting 512×512 PNG (e.g., Photoshop, Figma, Aseprite) |
| **Exact Action** | Export final application icon as `Assets/Icon_512x512.png` per spec in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md) |
| **Expected Output** | `Assets/Icon_512x512.png` — 32-bit RGBA PNG, exactly $512 \times 512$ px, max 1024 KB |
| **Verification Method** | `file Assets/Icon_512x512.png` confirms PNG; image editor confirms $512 \times 512$ dimensions |
| **Status** | `BLOCKED` |

---

## EXT-04 — Feature Graphic

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Marketing / Art Team) |
| **Required Input** | Graphic design tool capable of exporting 1024×500 PNG/JPEG |
| **Exact Action** | Create promotional banner and export as `Assets/FeatureGraphic_1024x500.png` per spec in [`Docs/MVP1.0-Store-Asset-Manifest.md`](./MVP1.0-Store-Asset-Manifest.md) |
| **Expected Output** | `Assets/FeatureGraphic_1024x500.png` — 24-bit RGB PNG or JPEG, exactly $1024 \times 500$ px, max 15 MB, no alpha |
| **Verification Method** | `file Assets/FeatureGraphic_1024x500.png` confirms PNG/JPEG; image editor confirms dimensions |
| **Status** | `BLOCKED` |

---

## EXT-05 — Screenshots

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (QA / Marketing Team) |
| **Required Input** | Physical Android device or Unity Editor with screenshot capability |
| **Exact Action** | Follow every step in [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./MVP1.0-Screenshot-Capture-Plan.md); capture all 12 portrait screens; save to `Screenshots/` directory |
| **Expected Output** | 12 PNG/JPEG files in `Screenshots/` — portrait orientation ($1080 \times 1920$ or $9:16$), zero debug overlays, clean production UI |
| **Verification Method** | Confirm 12 files present; confirm each matches the named screen in capture plan; confirm no dev UI, no FPS counters, no cheat menus |
| **Status** | `BLOCKED` |

---

## EXT-06 — Release AAB Generation

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Build Engineer / CI Pipeline) |
| **Required Input** | Unity Editor installation with Android Build Support module, Android SDK, JDK |
| **Exact Action** | Run: `<UNITY_PATH>/Unity.exe -quit -batchmode -projectPath "<REPO_PATH>" -executeMethod Lattirune.Editor.AndroidBuildScript.BuildProductionAAB -logFile build_aab.log` |
| **Expected Output** | `Builds/Android/Lattirune-1.0.0.aab` |
| **Verification Method** | Confirm file exists; verify with `bundletool build-apks` or upload attempt to Play Console |
| **Status** | `BLOCKED` |

> [!CAUTION]
> The `.aab` file must NOT be committed to git. It is excluded by `.gitignore`.

---

## EXT-07 — Production Signing

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Release Engineer / CI Environment) |
| **Required Input** | Production release keystore — stored ONLY in secure CI/CD secrets vault, NEVER in git |
| **Exact Action** | Sign the AAB from EXT-06 using `jarsigner` or `apksigner` with production keystore credentials stored in CI/CD environment variables |
| **Expected Output** | Signed `Lattirune-1.0.0-release.aab` verified by `apksigner verify` |
| **Verification Method** | `apksigner verify --print-certs <signed.aab>` shows correct signing certificate fingerprint |
| **Status** | `BLOCKED` |

> [!CAUTION]
> Never commit `.keystore`, `.jks`, `.p12`, `.pem`, or any private key to git. Use Google Play App Signing to further protect the upload key.

---

## EXT-08 — Google Play Console Submission

| Field | Detail |
| :--- | :--- |
| **Owner** | EXTERNAL (Publisher Account Admin) |
| **Required Input** | Active Google Play developer account, all EXT-01 through EXT-07 completed |
| **Exact Action** | In Google Play Console: create app listing, populate metadata from [`Docs/MVP1.0-Google-Play-Store-Listing.md`](./MVP1.0-Google-Play-Store-Listing.md), upload icon (EXT-03), feature graphic (EXT-04), screenshots (EXT-05), privacy URL (EXT-02), signed AAB (EXT-07); complete content rating and data safety questionnaires; submit for review |
| **Expected Output** | Active release track in Google Play Console (Internal ➜ Closed Testing ➜ Production) |
| **Verification Method** | Play Console shows "In review" or "Published" status for `com.developer.lattirune` version `1` |
| **Status** | `BLOCKED` (awaiting EXT-01 through EXT-07) |
