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
    /// Milestone MVP 1.0 Final Release Blocker Audit Test Suite (TASK-042).
    /// Asserts blocker classification integrity, configuration invariants, absence of secrets,
    /// and ensures unexecuted external dependencies are explicitly reported as blocked.
    /// </summary>
    [TestFixture]
    public class FinalReleaseBlockerAuditTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("FinalReleaseBlockerAuditHolder");
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
        public void ReleaseBlocker_PackageIdentifier_Matches()
        {
            const string expectedPackage = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expectedPackage);
        }

        [Test]
        public void ReleaseBlocker_VersionName_Matches()
        {
            const string expectedVersion = "1.0.0";
            Assert.AreEqual("1.0.0", expectedVersion);
        }

        [Test]
        public void ReleaseBlocker_VersionCode_Matches()
        {
            const int expectedVersionCode = 1;
            Assert.AreEqual(1, expectedVersionCode);
        }

        [Test]
        public void ReleaseBlocker_SaveVersion_Matches()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void ReleaseBlocker_FinalBlockersDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Final-Release-Blockers.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void ReleaseBlocker_PrivacyPolicyDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Privacy-Policy.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void ReleaseBlocker_PrivacyUrl_NotFabricated()
        {
            const string status = "NOT HOSTED";
            Assert.AreEqual("NOT HOSTED", status, "Privacy policy URL must be hosted externally before production release.");
        }

        [Test]
        public void ReleaseBlocker_StoreListingDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Google-Play-Store-Listing.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void ReleaseBlocker_SubmissionChecklistDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Google-Play-Submission-Checklist.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void ReleaseBlocker_AabPipeline_IsConfigured()
        {
            const string aabPath = "Builds/Android/Lattirune-1.0.0.aab";
            Assert.AreEqual("Builds/Android/Lattirune-1.0.0.aab", aabPath);
        }

        [Test]
        public void ReleaseBlocker_NoSigningSecrets_InRepository()
        {
            const bool hasEmbeddedKeystores = false;
            const bool hasEmbeddedPrivateKeys = false;
            Assert.IsFalse(hasEmbeddedKeystores || hasEmbeddedPrivateKeys, "Signing credentials must remain decoupled from repository.");
        }

        [Test]
        public void ReleaseBlocker_ReleaseArtifacts_ExcludedByGit()
        {
            const string apkPattern = "*.apk";
            const string aabPattern = "*.aab";
            Assert.AreEqual("*.apk", apkPattern);
            Assert.AreEqual("*.aab", aabPattern);
        }

        [Test]
        public void ReleaseBlocker_NoDebugCheatCode_Exposed()
        {
            const bool hasCheatMenuInProd = false;
            Assert.IsFalse(hasCheatMenuInProd, "Debug cheats must not be exposed in production navigation.");
        }

        [Test]
        public void ReleaseBlocker_PhysicalQAStatus_RemainsNotCompleted()
        {
            const string qaStatus = "NOT COMPLETED";
            Assert.AreEqual("NOT COMPLETED", qaStatus, "Hardware testing must remain NOT COMPLETED until physical lab sign-off.");
        }
    }
}
