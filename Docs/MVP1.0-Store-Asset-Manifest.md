# Lattirune MVP 1.0 Google Play Store Asset Manifest

**Date:** August 19, 2026  
**Application:** Lattirune  
**Target Package:** `com.developer.lattirune`  
**Version:** `1.0.0` (Build `1`)  
**Status Key:** `READY` | `PENDING ASSET CREATION` | `PENDING CAPTURE`

---

## 1. High-Resolution App Icon

* **Required Specification:**
  * **Dimensions:** $512 \times 512$ pixels
  * **Format:** 32-bit PNG (with alpha channel)
  * **Max File Size:** $1024\text{ KB}$
  * **Visual Design:** Stylized radiant Ember Rune symbol centered on a dark obsidian lattice tile with glowing orange/gold circuit conduits.
* **Android Launcher Densities:**
  * `mipmap-mdpi`: $48 \times 48\text{ px}$
  * `mipmap-hdpi`: $72 \times 72\text{ px}$
  * `mipmap-xhdpi`: $96 \times 96\text{ px}$
  * `mipmap-xxhdpi`: $144 \times 144\text{ px}$
  * `mipmap-xxxhdpi`: $192 \times 192\text{ px}$
* **Current Status:** `PENDING FINAL ASSET PACK`

---

## 2. Feature Graphic

* **Required Specification:**
  * **Dimensions:** $1024 \times 500$ pixels
  * **Format:** 24-bit JPEG or PNG (no transparency)
  * **Max File Size:** $15\text{ MB}$
  * **Safe Zone:** Core branding and art positioned within central $800 \times 400$ area.
  * **Visual Design:** Dark subterranean sewer dungeon aesthetic with fiery Ember and icy Frost conduits converging across a $5 \times 5$ lattice, illuminating the central metallic *Lattirune* logotype.
  * **Compliance:** Zero misleading award badges, zero false review stars, zero "Free Download" sticker overlays.
* **Current Status:** `PENDING ASSET CREATION`

---

## 3. Store Screenshots (12-Screen Portrait Set)

* **Required Specification:**
  * **Orientation:** Portrait ($1080 \times 1920$ or modern $1080 \times 2400$)
  * **Format:** 24-bit PNG or high-quality JPEG
  * **Quantity:** Minimum 4, Target 12 capture screens
  * **Compliance:** Pure in-game UI/gameplay; no letterboxing, stretched ratios, or misleading pre-rendered scenes.
* **Reference Document:** [`Docs/MVP1.0-Screenshot-Capture-Plan.md`](./MVP1.0-Screenshot-Capture-Plan.md)
* **Current Status:** `PENDING PHYSICAL/EDITOR CAPTURE`

---

## 4. Store Asset Tracking Summary Table

| Asset Item | Dimensions | Format | Requirement Level | Current Status |
| :--- | :--- | :--- | :--- | :--- |
| **High-Res Icon** | $512 \times 512\text{ px}$ | PNG | Mandatory (Play Console) | `PENDING FINAL ASSET PACK` |
| **Feature Graphic** | $1024 \times 500\text{ px}$ | PNG/JPEG | Mandatory (Play Console) | `PENDING ASSET CREATION` |
| **Phone Screenshots (Set of 12)** | $1080 \times 1920\text{ px}$ | PNG | Mandatory (Min 4) | `PENDING PHYSICAL/EDITOR CAPTURE` |
| **Store Listing Copy** | UTF-8 Text | Markdown | Mandatory | `READY` ([`MVP1.0-Google-Play-Store-Listing.md`](./MVP1.0-Google-Play-Store-Listing.md)) |
| **Privacy Policy** | UTF-8 Text | Markdown | Mandatory (Hosted URL) | `READY` ([`MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md)) |
| **Content Rating Prep** | UTF-8 Text | Markdown | Mandatory (IARC) | `READY` ([`MVP1.0-Content-Rating-Preparation.md`](./MVP1.0-Content-Rating-Preparation.md)) |
| **AAB Build Pipeline** | C# Build Script | Unity Editor | Mandatory | `CONFIGURED` ([`AndroidBuildScript.cs`](../Assets/_Project/Editor/AndroidBuildScript.cs)) |
