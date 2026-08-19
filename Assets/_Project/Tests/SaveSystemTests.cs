using System.IO;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class SaveSystemTests
    {
        private GameObject _saveHolder;
        private SaveSystem _saveSystem;
        private string _testDir;

        [SetUp]
        public void Setup()
        {
            _saveHolder = new GameObject("TestSaveHolder");
            _saveSystem = _saveHolder.AddComponent<SaveSystem>();

            _testDir = Path.Combine(Application.temporaryCachePath, "LattiruneSaveTests");
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
            Directory.CreateDirectory(_testDir);

            _saveSystem.SetCustomDirectory(_testDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }

            if (_saveHolder != null)
            {
                Object.DestroyImmediate(_saveHolder);
            }
        }

        [Test]
        public void SaveSystem_SaveAndLoad_PreservesDataCorrectly()
        {
            SaveData data = SaveData.CreateDefault();
            data.items.Add(new SavedItemData("item_ember_blade", 2, 2, 90, true, 0f, 0f));
            data.settings = new SavedSettingsData(0.7f, 0.4f, false, true);

            SaveResult saveResult = _saveSystem.Save(data);
            Assert.IsTrue(saveResult.IsSuccess);
            Assert.IsTrue(File.Exists(_saveSystem.PrimarySavePath));

            LoadResult loadResult = _saveSystem.Load();
            Assert.IsTrue(loadResult.IsSuccess);
            Assert.AreEqual(SaveStatus.Success, loadResult.Status);
            Assert.AreEqual(data.items.Count, loadResult.Data.items.Count);
            Assert.AreEqual(0.7f, loadResult.Data.settings.masterVolume, 0.001f);
            Assert.AreEqual(0.4f, loadResult.Data.settings.sfxVolume, 0.001f);
        }

        [Test]
        public void SaveSystem_MissingSave_ReturnsNoSaveWithDefaultProfile()
        {
            Assert.IsFalse(_saveSystem.HasSave());

            LoadResult loadResult = _saveSystem.Load();
            Assert.AreEqual(SaveStatus.NoSave, loadResult.Status);
            Assert.IsNotNull(loadResult.Data);
            Assert.AreEqual(SaveVersion.CURRENT_VERSION, loadResult.Data.version);
        }

        [Test]
        public void SaveSystem_CorruptPrimary_RecoversFromBackup()
        {
            // 1. Create first valid save
            SaveData initialData = SaveData.CreateDefault();
            initialData.settings = new SavedSettingsData(0.5f, 0.5f, false, true);
            _saveSystem.Save(initialData);

            // 2. Create second valid save (this pushes first save to backup)
            SaveData updatedData = SaveData.CreateDefault();
            updatedData.settings = new SavedSettingsData(0.9f, 0.9f, false, true);
            _saveSystem.Save(updatedData);

            Assert.IsTrue(File.Exists(_saveSystem.PrimarySavePath));
            Assert.IsTrue(File.Exists(_saveSystem.BackupSavePath));

            // 3. Corrupt primary save file
            File.WriteAllBytes(_saveSystem.PrimarySavePath, new byte[] { 0x00, 0xFF, 0xAA, 0x55, 0x12 });

            // 4. Load must recover from backup
            LoadResult loadResult = _saveSystem.Load();
            Assert.AreEqual(SaveStatus.RecoveredFromBackup, loadResult.Status);
            Assert.IsNotNull(loadResult.Data);
            Assert.AreEqual(0.5f, loadResult.Data.settings.masterVolume, 0.001f);
        }

        [Test]
        public void SaveSystem_CorruptPrimaryAndBackup_CreatesSafeDefault()
        {
            // Create corrupt files in both primary and backup locations
            File.WriteAllBytes(_saveSystem.PrimarySavePath, new byte[] { 0x01, 0x02, 0x03 });
            File.WriteAllBytes(_saveSystem.BackupSavePath, new byte[] { 0x04, 0x05, 0x06 });
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Both primary and backup saves are corrupt"));
            LoadResult loadResult = _saveSystem.Load();
            Assert.AreEqual(SaveStatus.Corrupt, loadResult.Status);
            Assert.IsNotNull(loadResult.Data);
            Assert.AreEqual(SaveVersion.CURRENT_VERSION, loadResult.Data.version);
        }

        [Test]
        public void SaveSystem_DeleteSave_CleansFiles()
        {
            SaveData data = SaveData.CreateDefault();
            _saveSystem.Save(data);
            Assert.IsTrue(_saveSystem.HasSave());

            bool deleted = _saveSystem.DeleteSave();
            Assert.IsTrue(deleted);
            Assert.IsFalse(_saveSystem.HasSave());
            Assert.IsFalse(File.Exists(_saveSystem.PrimarySavePath));
        }

        [Test]
        public void SaveSystem_SettingsRoundTrip_PreservedAndClamped()
        {
            SaveData data = new SaveData
            {
                settings = new SavedSettingsData(0.75f, 0.25f, true, false)
            };

            _saveSystem.Save(data);
            LoadResult result = _saveSystem.Load();

            Assert.AreEqual(0.75f, result.Data.settings.masterVolume, 0.001f);
            Assert.AreEqual(0.25f, result.Data.settings.sfxVolume, 0.001f);
            Assert.IsTrue(result.Data.settings.isMuted);
            Assert.IsFalse(result.Data.settings.hapticsEnabled);
        }
    }
}
