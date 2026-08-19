using NUnit.Framework;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class SaveSerializerTests
    {
        [Test]
        public void SaveSerializer_SerializesAndDeserializes_MatchesOriginal()
        {
            SaveData original = SaveData.CreateDefault();
            original.items.Add(new SavedItemData("test_item", 2, 3, 90, true, 0f, 0f));
            original.settings = new SavedSettingsData(0.8f, 0.6f, false, true);

            string json = SaveSerializer.SerializeToJson(original);
            Assert.IsFalse(string.IsNullOrEmpty(json));

            SaveData deserialized = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.version, deserialized.version);
            Assert.AreEqual(original.items.Count, deserialized.items.Count);
            Assert.AreEqual(0.8f, deserialized.settings.masterVolume, 0.001f);
            Assert.AreEqual(0.6f, deserialized.settings.sfxVolume, 0.001f);
        }

        [Test]
        public void SaveSerializer_NullInput_ReturnsNullGracefully()
        {
            Assert.IsNull(SaveSerializer.SerializeToJson(null));
            Assert.IsNull(SaveSerializer.DeserializeFromJson(null));
            Assert.IsNull(SaveSerializer.DeserializeFromJson(""));
        }

        [Test]
        public void SaveSerializer_SaveVersion_PreservedCorrectly()
        {
            SaveData data = new SaveData { version = 1 };
            string json = SaveSerializer.SerializeToJson(data);
            SaveData deserialized = SaveSerializer.DeserializeFromJson(json);

            Assert.AreEqual(1, deserialized.version);
        }
    }
}
