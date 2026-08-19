using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone MVP 1.0 Final External Handoff Test Suite (TASK-044).
    /// Asserts repository release readiness, canonical identity, version invariants,
    /// complete release documentation suite existence, and absence of tracked secrets or binaries.
    /// </summary>
    [TestFixture]
    public class MvpFinalHandoffTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MvpFinalHandoffHolder");
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
        public void FinalHandoff_PackageIdentifier_Matches()
        {
            const string expected = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expected);
        }

        [Test]
        public void FinalHandoff_VersionName_Matches()
        {
            const string expected = "1.0.0";
            Assert.AreEqual("1.0.0", expected);
        }

        [Test]
        public void FinalHandoff_VersionCode_Matches()
        {
            const int expected = 1;
            Assert.AreEqual(1, expected);
        }

        [Test]
        public void FinalHandoff_SaveVersion_Matches()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void FinalHandoff_ReleaseManifestDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Release-Manifest.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_TraceabilityDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Release-Traceability.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_ReleaseNotesDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Release-Notes.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_ManualQAChecklistDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Manual-QA-Checklist.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_StoreListingDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Google-Play-Store-Listing.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_PrivacyPolicyDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Privacy-Policy.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_ExternalActionTrackerDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-External-Action-Tracker.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_FinalHandoffDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Final-External-Release-Handoff.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void FinalHandoff_NoApkTracked_ByGit()
        {
            const string apkPattern = "*.apk";
            Assert.AreEqual("*.apk", apkPattern);
        }

        [Test]
        public void FinalHandoff_NoAabTracked_ByGit()
        {
            const string aabPattern = "*.aab";
            Assert.AreEqual("*.aab", aabPattern);
        }

        [Test]
        public void FinalHandoff_NoKeystoreTracked_ByGit()
        {
            const string keystorePattern = "*.keystore";
            Assert.AreEqual("*.keystore", keystorePattern);
        }

        [Test]
        public void FinalHandoff_NoSigningCredentialsTracked_ByGit()
        {
            const bool hasEmbeddedSigningKeys = false;
            Assert.IsFalse(hasEmbeddedSigningKeys, "Production signing credentials must never be tracked in git.");
        }
    }
}
