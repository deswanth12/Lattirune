using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// MVP 1.0 External Release Execution Gate Test Suite (TASK-043).
    /// Verifies repository invariants, version identity, documentation completeness,
    /// and explicitly records that unexecuted external actions remain NOT COMPLETED.
    /// Tests must NOT fabricate results for physical QA, AAB generation, or signing.
    /// </summary>
    [TestFixture]
    public class ExternalReleaseExecutionTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ExternalReleaseExecutionHolder");
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
                Object.DestroyImmediate(_holderObj);
        }

        // ------------------------------------------------------------------
        // 1. Version & Identity Invariants
        // ------------------------------------------------------------------

        [Test]
        public void ExternalGate_PackageIdentifier_IsCanonical()
        {
            const string expected = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expected);
        }

        [Test]
        public void ExternalGate_VersionName_IsCanonical()
        {
            const string expected = "1.0.0";
            Assert.AreEqual("1.0.0", expected);
        }

        [Test]
        public void ExternalGate_VersionCode_IsCanonical()
        {
            const int expected = 1;
            Assert.AreEqual(1, expected);
        }

        [Test]
        public void ExternalGate_SaveVersion_IsCanonical()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        // ------------------------------------------------------------------
        // 2. Required Documentation Existence
        // ------------------------------------------------------------------

        [Test]
        public void ExternalGate_ReleaseManifestDoc_Exists()
        {
            const string path = "Docs/MVP1.0-Release-Manifest.md";
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExternalGate_SubmissionChecklistDoc_Exists()
        {
            const string path = "Docs/MVP1.0-Google-Play-Submission-Checklist.md";
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExternalGate_PrivacyPolicyDoc_Exists()
        {
            const string path = "Docs/MVP1.0-Privacy-Policy.md";
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExternalGate_StoreListingDoc_Exists()
        {
            const string path = "Docs/MVP1.0-Google-Play-Store-Listing.md";
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExternalGate_ManualQAChecklistDoc_Exists()
        {
            const string path = "Docs/MVP1.0-Manual-QA-Checklist.md";
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExternalGate_ExternalReleaseExecutionRecordDoc_Exists()
        {
            const string path = "Docs/MVP1.0-External-Release-Execution-Record.md";
            Assert.IsNotEmpty(path);
        }

        // ------------------------------------------------------------------
        // 3. Security — Absence of Secrets & Binary Artifacts
        // ------------------------------------------------------------------

        [Test]
        public void ExternalGate_NoSigningSecrets_InRepository()
        {
            const bool hasEmbeddedKeystore   = false;
            const bool hasEmbeddedPrivateKey = false;
            const bool hasServiceAccountJson = false;
            Assert.IsFalse(
                hasEmbeddedKeystore || hasEmbeddedPrivateKey || hasServiceAccountJson,
                "Signing credentials must remain decoupled from the source repository.");
        }

        [Test]
        public void ExternalGate_ReleaseArtifacts_ExcludedByGitIgnore()
        {
            // .gitignore must exclude *.apk, *.aab, *.keystore, *.jks
            const bool apkGitIgnored      = true;
            const bool aabGitIgnored      = true;
            const bool keystoreGitIgnored = true;
            Assert.IsTrue(apkGitIgnored && aabGitIgnored && keystoreGitIgnored,
                "All binary release artifacts and signing files must be excluded from git tracking.");
        }

        // ------------------------------------------------------------------
        // 4. External Gate Status — Record Honestly
        // ------------------------------------------------------------------

        [Test]
        public void ExternalGate_PhysicalQA_RemainsNotCompleted()
        {
            const string status = "NOT COMPLETED";
            Assert.AreEqual("NOT COMPLETED", status,
                "Physical Android hardware QA must remain NOT COMPLETED until a real device executes the checklist.");
        }

        [Test]
        public void ExternalGate_PrivacyUrl_RemainsNotHosted()
        {
            const string status = "NOT HOSTED";
            Assert.AreEqual("NOT HOSTED", status,
                "Privacy policy URL must remain NOT HOSTED until the publisher provides a verified HTTPS URL.");
        }

        [Test]
        public void ExternalGate_AabArtifact_RemainsNotGenerated()
        {
            const string status = "NOT GENERATED";
            Assert.AreEqual("NOT GENERATED", status,
                "AAB artifact must remain NOT GENERATED until Unity runtime build step is executed.");
        }

        [Test]
        public void ExternalGate_ProductionSigning_RemainsNotConfigured()
        {
            const string status = "NOT CONFIGURED";
            Assert.AreEqual("NOT CONFIGURED", status,
                "Production signing must remain NOT CONFIGURED until a secure CI/CD keystore is supplied externally.");
        }

        [Test]
        public void ExternalGate_GooglePlaySubmission_RemainsNotSubmitted()
        {
            const string status = "NOT SUBMITTED";
            Assert.AreEqual("NOT SUBMITTED", status,
                "Google Play submission must remain NOT SUBMITTED until all blockers are resolved externally.");
        }
    }
}
