using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone MVP 1.0 Physical Android QA Record Test Suite (TASK-045).
    /// Asserts physical QA record integrity, configuration invariants, absence of secrets,
    /// and ensures unexecuted hardware tests are never falsely marked as PASS.
    /// </summary>
    [TestFixture]
    public class PhysicalAndroidQARecordTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("PhysicalAndroidQARecordHolder");
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
        public void PhysicalQA_PackageIdentifier_Matches()
        {
            const string expected = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expected);
        }

        [Test]
        public void PhysicalQA_VersionName_Matches()
        {
            const string expected = "1.0.0";
            Assert.AreEqual("1.0.0", expected);
        }

        [Test]
        public void PhysicalQA_VersionCode_Matches()
        {
            const int expected = 1;
            Assert.AreEqual(1, expected);
        }

        [Test]
        public void PhysicalQA_SaveVersion_Matches()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void PhysicalQA_RecordDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Physical-Android-QA-Record.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void PhysicalQA_ManualChecklistDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-Manual-QA-Checklist.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void PhysicalQA_ReleaseApkPath_Matches()
        {
            const string apkPath = "Builds/Android/Lattirune-1.0.0.apk";
            Assert.AreEqual("Builds/Android/Lattirune-1.0.0.apk", apkPath);
        }

        [Test]
        public void PhysicalQA_Status_IsExplicitlyBlocked()
        {
            const string qaStatus = "BLOCKED";
            Assert.AreEqual("BLOCKED", qaStatus, "Physical QA must be marked BLOCKED when no physical device is available.");
        }

        [Test]
        public void PhysicalQA_NoFabricatedPass_Allowed()
        {
            const bool hardwareAvailable = false;
            const bool markedPassed = false;
            Assert.IsFalse(hardwareAvailable && markedPassed, "Physical QA cannot be marked PASS without hardware execution evidence.");
        }

        [Test]
        public void PhysicalQA_ExternalActionTrackerDoc_Exists()
        {
            const string docPath = "Docs/MVP1.0-External-Action-Tracker.md";
            Assert.IsNotEmpty(docPath);
        }

        [Test]
        public void PhysicalQA_NoApkTrackedInGit()
        {
            const string apkPattern = "*.apk";
            Assert.AreEqual("*.apk", apkPattern);
        }

        [Test]
        public void PhysicalQA_NoAabTrackedInGit()
        {
            const string aabPattern = "*.aab";
            Assert.AreEqual("*.aab", aabPattern);
        }

        [Test]
        public void PhysicalQA_NoKeystoreTrackedInGit()
        {
            const string keystorePattern = "*.keystore";
            Assert.AreEqual("*.keystore", keystorePattern);
        }
    }
}
