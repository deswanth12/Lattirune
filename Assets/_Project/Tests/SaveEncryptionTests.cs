using System.Text;
using NUnit.Framework;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class SaveEncryptionTests
    {
        [Test]
        public void SaveEncryption_RoundTrip_ReturnsOriginalString()
        {
            string plainText = "{\"version\":1,\"items\":[{\"itemId\":\"item_training_sword\",\"gridX\":2,\"gridY\":2}]}";

            byte[] encrypted = SaveEncryption.EncryptStringToBytes(plainText);
            Assert.IsNotNull(encrypted);
            Assert.IsTrue(encrypted.Length > 0);

            string decrypted = SaveEncryption.DecryptBytesToString(encrypted);
            Assert.AreEqual(plainText, decrypted);
        }

        [Test]
        public void SaveEncryption_EncryptedBytes_DoNotMatchPlainJson()
        {
            string plainText = "{\"secret\":\"player_data_progress\"}";
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = SaveEncryption.EncryptStringToBytes(plainText);

            Assert.AreNotEqual(plainBytes, encrypted);
        }

        [Test]
        public void SaveEncryption_CorruptBytes_ReturnsNullGracefully()
        {
            byte[] corruptData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x12, 0x34 };

            string result = SaveEncryption.DecryptBytesToString(corruptData);
            Assert.IsNull(result);
        }

        [Test]
        public void SaveEncryption_EmptyOrNullInput_HandledSafely()
        {
            Assert.IsNull(SaveEncryption.EncryptStringToBytes(null));
            Assert.IsNull(SaveEncryption.EncryptStringToBytes(""));
            Assert.IsNull(SaveEncryption.DecryptBytesToString(null));
            Assert.IsNull(SaveEncryption.DecryptBytesToString(new byte[0]));
        }
    }
}
