using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Lattirune.Save
{
    /// <summary>
    /// Cryptographic protection layer for local save data at rest using AES-256-CBC.
    /// Protects against casual file inspection and manual tampering.
    /// Note: Local client-side encryption is not a complete guarantee against determined reverse-engineering on rooted devices.
    /// </summary>
    public static class SaveEncryption
    {
        // 16-byte fixed IV seed and 32-byte key derived deterministically for local installation
        private static readonly byte[] Salt = new byte[] { 0x4C, 0x61, 0x74, 0x74, 0x69, 0x72, 0x75, 0x6E, 0x65, 0x53, 0x61, 0x76, 0x65, 0x32, 0x30, 0x32 }; // "LattiruneSave202"
        private static readonly string SecretPassphrase = "Lattirune_Lattice_Rune_Engine_Key_V1";

        public static byte[] EncryptStringToBytes(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return null;
            }

            try
            {
                using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(SecretPassphrase, Salt, 1000))
                {
                    byte[] key = keyDerivation.GetBytes(32); // AES-256
                    byte[] iv = keyDerivation.GetBytes(16);  // 128-bit block IV

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                                cs.Write(plainBytes, 0, plainBytes.Length);
                                cs.FlushFinalBlock();
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Lattirune.Save] Encryption error: {ex.Message}");
                return null;
            }
        }

        public static string DecryptBytesToString(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
            {
                return null;
            }

            try
            {
                using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(SecretPassphrase, Salt, 1000))
                {
                    byte[] key = keyDerivation.GetBytes(32);
                    byte[] iv = keyDerivation.GetBytes(16);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        using (MemoryStream ms = new MemoryStream(cipherBytes))
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                            {
                                using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                                {
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Lattirune.Save] Decryption failed (file corrupted or modified): {ex.Message}");
                return null;
            }
        }
    }
}
