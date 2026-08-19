using System.Collections.Generic;
using NUnit.Framework;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class SaveValidatorTests
    {
        [Test]
        public void SaveValidator_ValidSaveData_PassesValidation()
        {
            SaveData data = SaveData.CreateDefault();
            bool isValid = SaveValidator.ValidateSaveData(data, out List<string> errors);

            Assert.IsTrue(isValid);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void SaveValidator_InvalidVersion_Rejected()
        {
            SaveData data = SaveData.CreateDefault();
            data.version = 999; // unsupported future version

            bool isValid = SaveValidator.ValidateSaveData(data, out List<string> errors);
            Assert.IsFalse(isValid);
            Assert.IsTrue(errors.Exists(e => e.Contains("Unsupported save version")));
        }

        [Test]
        public void SaveValidator_InvalidGridCoordinates_Rejected()
        {
            SaveData data = SaveData.CreateDefault();
            data.items.Add(new SavedItemData("bad_item", 99, -5, 0, true, 0f, 0f));

            bool isValid = SaveValidator.ValidateSaveData(data, out List<string> errors);
            Assert.IsFalse(isValid);
            Assert.IsTrue(errors.Exists(e => e.Contains("invalid grid coordinates")));
        }

        [Test]
        public void SaveValidator_InvalidRotation_Rejected()
        {
            SaveData data = SaveData.CreateDefault();
            data.items.Add(new SavedItemData("crooked_item", 2, 2, 45, true, 0f, 0f)); // 45 degrees is illegal

            bool isValid = SaveValidator.ValidateSaveData(data, out List<string> errors);
            Assert.IsFalse(isValid);
            Assert.IsTrue(errors.Exists(e => e.Contains("invalid rotation")));
        }

        [Test]
        public void SaveValidator_InvalidSettingsVolume_Rejected()
        {
            SaveData data = SaveData.CreateDefault();
            data.settings.masterVolume = 2.5f;

            bool isValid = SaveValidator.ValidateSaveData(data, out List<string> errors);
            Assert.IsFalse(isValid);
            Assert.IsTrue(errors.Exists(e => e.Contains("master volume")));
        }

        [Test]
        public void SaveValidator_NullSaveData_Rejected()
        {
            bool isValid = SaveValidator.ValidateSaveData(null, out List<string> errors);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, errors.Count);
        }
    }
}
