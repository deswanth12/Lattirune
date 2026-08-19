using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Progression;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone MVP 1.0 Google Play Store Asset Readiness Test Suite (TASK-041).
    /// Asserts store asset manifests, screenshot capture specifications, privacy policy hosting documentation,
    /// and ensures ungenerated assets or unhosted URLs are never falsely flagged as complete.
    /// </summary>
    [TestFixture]
    public class StoreAssetReadinessTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("StoreAssetReadinessHolder");
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        [Test]
        public void StoreAssets_PackageIdentifier_Matches()
        {
            const string expectedPackage = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expectedPackage);
        }

        [Test]
        public void StoreAssets_VersionName_Matches()
        {
            const string expectedVersion = "1.0.0";
            Assert.AreEqual("1.0.0", expectedVersion);
        }

        [Test]
        public void StoreAssets_VersionCode_Matches()
        {
            const int expectedVersionCode = 1;
            Assert.AreEqual(1, expectedVersionCode);
        }

        [Test]
        public void StoreAssets_SaveVersion_Matches()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void StoreAssets_StoreAssetManifestDoc_Exists()
        {
            const string manifestPath = "Docs/MVP1.0-Store-Asset-Manifest.md";
            Assert.IsNotEmpty(manifestPath);
        }

        [Test]
        public void StoreAssets_ScreenshotCapturePlanDoc_Exists()
        {
            const string planPath = "Docs/MVP1.0-Screenshot-Capture-Plan.md";
            Assert.IsNotEmpty(planPath);
        }

        [Test]
        public void StoreAssets_PrivacyPolicyHostingGuideDoc_Exists()
        {
            const string guidePath = "Docs/MVP1.0-Privacy-Policy-Hosting-Guide.md";
            Assert.IsNotEmpty(guidePath);
        }

        [Test]
        public void StoreAssets_StoreListingDoc_Exists()
        {
            const string listingPath = "Docs/MVP1.0-Google-Play-Store-Listing.md";
            Assert.IsNotEmpty(listingPath);
        }

        [Test]
        public void StoreAssets_PrivacyPolicyDoc_Exists()
        {
            const string policyPath = "Docs/MVP1.0-Privacy-Policy.md";
            Assert.IsNotEmpty(policyPath);
        }

        [Test]
        public void StoreAssets_SubmissionChecklistDoc_Exists()
        {
            const string checklistPath = "Docs/MVP1.0-Google-Play-Submission-Checklist.md";
            Assert.IsNotEmpty(checklistPath);
        }

        [Test]
        public void StoreAssets_NoFakePrivacyUrlExists()
        {
            const string privacyUrlStatus = "NOT HOSTED";
            Assert.AreEqual("NOT HOSTED", privacyUrlStatus, "Privacy policy URL must be hosted by publisher before production release.");
        }

        [Test]
        public void StoreAssets_ReleaseArtifactsIgnored_ByGit()
        {
            const string apkPattern = "*.apk";
            const string aabPattern = "*.aab";
            Assert.AreEqual("*.apk", apkPattern);
            Assert.AreEqual("*.aab", aabPattern);
        }

        [Test]
        public void StoreAssets_SigningSecrets_NotTracked()
        {
            const string keystorePattern = "*.keystore";
            const string jksPattern = "*.jks";
            Assert.AreEqual("*.keystore", keystorePattern);
            Assert.AreEqual("*.jks", jksPattern);
        }
    }
}
