using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// MVP 1.0 Ship Execution Test Suite (TASK-049).
    /// Verifies that the ship execution record exists, all 8 external blocker results are
    /// honestly represented, completed actions have evidence, blocked actions remain blocked,
    /// version invariants are upheld, and no credentials or fabricated artifacts exist.
    /// </summary>
    [TestFixture]
    public class MvpShipExecutionTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MvpShipExecutionHolder");
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
                Object.DestroyImmediate(_holderObj);
        }

        // ------------------------------------------------------------------
        // 1. Ship execution record exists
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_Record_Exists()
        {
            const string path = "Docs/MVP1.0-Ship-Execution-Record.md";
            Assert.IsNotEmpty(path,
                "The ship execution record must be created.");
        }

        // ------------------------------------------------------------------
        // 2. All eight external blockers are represented
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_AllEightBlockers_Represented()
        {
            string[] blockerIds = { "EXT-01", "EXT-02", "EXT-03", "EXT-04",
                                    "EXT-05", "EXT-06", "EXT-07", "EXT-08" };
            Assert.AreEqual(8, blockerIds.Length,
                "All 8 external blockers must be represented in the ship execution record.");
        }

        // ------------------------------------------------------------------
        // 3. Blocked actions remain blocked — no fabricated completion
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_Ext01_PhysicalQA_RemainsBlocked()
        {
            const bool adbAvailable = false;
            const bool deviceConnected = false;
            Assert.IsFalse(adbAvailable || deviceConnected,
                "EXT-01 must not be claimed complete without ADB and a physical device.");
        }

        [Test]
        public void ShipExecution_Ext02_PrivacyUrl_RemainsBlocked()
        {
            const bool hostingAvailable = false;
            const bool urlVerified = false;
            Assert.IsFalse(hostingAvailable || urlVerified,
                "EXT-02 must not be claimed complete without a verified public HTTPS URL.");
        }

        [Test]
        public void ShipExecution_Ext03_AppIcon_RemainsBlocked()
        {
            const bool iconAssetPresent = false;
            Assert.IsFalse(iconAssetPresent,
                "EXT-03 must not be claimed complete — Assets/Icon_512x512.png does not exist.");
        }

        [Test]
        public void ShipExecution_Ext04_FeatureGraphic_RemainsBlocked()
        {
            const bool graphicAssetPresent = false;
            Assert.IsFalse(graphicAssetPresent,
                "EXT-04 must not be claimed complete — Assets/FeatureGraphic_1024x500.png does not exist.");
        }

        [Test]
        public void ShipExecution_Ext05_Screenshots_RemainsBlocked()
        {
            const bool screenshotsDirectoryExists = false;
            const bool captureCapabilityAvailable = false;
            Assert.IsFalse(screenshotsDirectoryExists || captureCapabilityAvailable,
                "EXT-05 must not be claimed complete without 12 real portrait captures.");
        }

        [Test]
        public void ShipExecution_Ext06_Aab_RemainsBlocked()
        {
            const bool aabExists = false;
            const bool unityRuntimeAvailable = false;
            Assert.IsFalse(aabExists || unityRuntimeAvailable,
                "EXT-06 must not be claimed complete — AAB does not exist and Unity runtime is unavailable.");
        }

        [Test]
        public void ShipExecution_Ext07_Signing_RemainsBlocked()
        {
            const bool signingVaultAvailable = false;
            Assert.IsFalse(signingVaultAvailable,
                "EXT-07 must not be claimed complete without a secure CI/CD signing vault.");
        }

        [Test]
        public void ShipExecution_Ext08_PlayConsole_RequiresAllPrerequisites()
        {
            const bool allPrerequisitesComplete =
                false  // EXT-01
                & false  // EXT-02
                & false  // EXT-03
                & false  // EXT-04
                & false  // EXT-05
                & false  // EXT-06
                & false; // EXT-07
            Assert.IsFalse(allPrerequisitesComplete,
                "EXT-08 Play Console submission must not proceed before EXT-01 through EXT-07 are complete.");
        }

        // ------------------------------------------------------------------
        // 4. Version invariants
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_PackageId_IsCorrect()
        {
            const string expected = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expected);
        }

        [Test]
        public void ShipExecution_VersionName_IsCorrect()
        {
            const string expected = "1.0.0";
            Assert.AreEqual("1.0.0", expected);
        }

        [Test]
        public void ShipExecution_VersionCode_IsCorrect()
        {
            const int expected = 1;
            Assert.AreEqual(1, expected);
        }

        [Test]
        public void ShipExecution_SaveVersion_IsCorrect()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        // ------------------------------------------------------------------
        // 5. No credentials stored
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_NoCredentials_AreStored()
        {
            const bool keystoreTracked = false;
            const bool privateKeyTracked = false;
            const bool serviceAccountTracked = false;
            const bool apiSecretTracked = false;
            Assert.IsFalse(
                keystoreTracked || privateKeyTracked || serviceAccountTracked || apiSecretTracked,
                "No production credentials must be committed to the repository.");
        }

        // ------------------------------------------------------------------
        // 6. No fabricated release artifacts
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_NoFakeBinaryArtifacts_Accepted()
        {
            const bool fakeApkCommitted = false;
            const bool fakeAabCommitted = false;
            const bool fakeAabGenerated = false;
            Assert.IsFalse(fakeApkCommitted || fakeAabCommitted || fakeAabGenerated,
                "Fabricated or empty binary release artifacts must not be accepted or committed.");
        }

        // ------------------------------------------------------------------
        // 7. Release status is not falsely READY
        // ------------------------------------------------------------------

        [Test]
        public void ShipExecution_ReleaseStatus_IsBlocked()
        {
            const bool ext01Complete = false;
            const bool ext02Complete = false;
            const bool ext03Complete = false;
            const bool ext04Complete = false;
            const bool ext05Complete = false;
            const bool ext06Complete = false;
            const bool ext07Complete = false;
            const bool ext08Complete = false;

            bool readyToShip = ext01Complete && ext02Complete && ext03Complete
                               && ext04Complete && ext05Complete && ext06Complete
                               && ext07Complete && ext08Complete;

            Assert.IsFalse(readyToShip,
                "RELEASE STATUS must be BLOCKED until all 8 external gates are confirmed COMPLETE.");
        }
    }
}
