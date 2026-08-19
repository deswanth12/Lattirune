# Lattirune MVP 1.0 Privacy Policy Hosting Guide

**Date:** August 19, 2026  
**Application:** Lattirune  
**Target Package:** `com.developer.lattirune`  
**Source Document:** [`Docs/MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md)  
**Status:** `Document READY` / `Public URL NOT HOSTED`

---

## 1. Google Play Privacy Policy Requirements

To satisfy Google Play Store submission policies, every published application must provide a direct, publicly accessible link to an accurate Privacy Policy:
* **HTTPS Protocol:** The URL must use a secure `https://` endpoint with a valid SSL/TLS certificate.
* **No Authentication Walls:** The page must be publicly viewable without requiring user login, account creation, or payment.
* **Direct Landing:** The link must direct users straight to the Lattirune privacy policy content without redirects or landing page ambiguity.
* **Mobile Friendly:** The page must render legibly on mobile web browsers.

---

## 2. Practical Hosting Options

### Option A: GitHub Pages (Recommended / Zero Cost)
1. In the repository settings on GitHub (`https://github.com/deswanth12/Lattirune`), navigate to **Pages**.
2. Set the build source to deploy from the `/docs` folder or `gh-pages` branch.
3. Convert [`MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md) to `index.html` or serve via Jekyll/Markdown.
4. Your resulting public URL will follow the standard pattern:  
   `https://<username>.github.io/Lattirune/privacy-policy.html`

### Option B: Custom Domain / Developer Website
1. Upload the text from [`MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md) to your personal or studio web server.
2. Ensure proper HTTPS configuration.
3. Target URL example: `https://yourstudio.com/lattirune/privacy`

### Option C: Public Static Hosting (Cloudflare Pages / Vercel / Netlify)
1. Link a public repository branch to a static hosting provider.
2. Deploy the static HTML version of the policy.
3. Target URL example: `https://lattirune-privacy.pages.dev`

---

## 3. Pre-Submission Verification Steps

Before pasting the final URL into the Google Play Console:
1. Open the URL in an **Incognito / Private Browsing** window on both desktop and mobile.
2. Confirm the page loads in $<2.0\text{s}$ with zero certificate warnings.
3. Confirm all text matches [`Docs/MVP1.0-Privacy-Policy.md`](./MVP1.0-Privacy-Policy.md) identically.
4. Paste the verified HTTPS URL into the **Google Play Console $\rightarrow$ App Content $\rightarrow$ Privacy Policy** field.
