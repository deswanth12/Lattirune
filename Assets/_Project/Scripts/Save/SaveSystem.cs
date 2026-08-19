using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Lattirune.Save
{
    /// <summary>
    /// Coordinates local persistent storage, atomic file writing (.tmp -> .dat),
    /// AES-256 encryption at rest, backup recovery, and fail-safe deserialization.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public const string PRIMARY_SAVE_FILENAME = "lattirune_save.dat";
        public const string BACKUP_SAVE_FILENAME = "lattirune_save.backup.dat";
        public const string TEMP_SAVE_FILENAME = "lattirune_save.tmp";

        private string _customDirectory = null;

        public string SaveDirectory => !string.IsNullOrEmpty(_customDirectory) ? _customDirectory : Application.persistentDataPath;
        public string PrimarySavePath => Path.Combine(SaveDirectory, PRIMARY_SAVE_FILENAME);
        public string BackupSavePath => Path.Combine(SaveDirectory, BACKUP_SAVE_FILENAME);
        public string TempSavePath => Path.Combine(SaveDirectory, TEMP_SAVE_FILENAME);

        public void SetCustomDirectory(string dir)
        {
            _customDirectory = dir;
            if (!string.IsNullOrEmpty(_customDirectory) && !Directory.Exists(_customDirectory))
            {
                Directory.CreateDirectory(_customDirectory);
            }
        }

        public bool HasSave()
        {
            return File.Exists(PrimarySavePath) || File.Exists(BackupSavePath);
        }

        /// <summary>
        /// Saves player data atomically with AES encryption and backup creation.
        /// </summary>
        public SaveResult Save(SaveData data)
        {
            if (data == null)
            {
                return SaveResult.Failed("Cannot save null SaveData.");
            }

            // 1. Validate Data
            if (!SaveValidator.ValidateSaveData(data, out List<string> errors))
            {
                string errorSummary = string.Join("; ", errors);
                Debug.LogWarning($"[Lattirune.Save] Save validation failed: {errorSummary}");
                return SaveResult.Failed($"Validation failed: {errorSummary}");
            }

            // 2. Serialize to JSON
            string json = SaveSerializer.SerializeToJson(data);
            if (string.IsNullOrEmpty(json))
            {
                return SaveResult.Failed("Serialization returned empty JSON.");
            }

            // 3. Encrypt to Bytes
            byte[] encryptedBytes = SaveEncryption.EncryptStringToBytes(json);
            if (encryptedBytes == null || encryptedBytes.Length == 0)
            {
                return SaveResult.Failed("Encryption failed.");
            }

            // 4. Atomic Write (.tmp -> .backup -> .dat)
            try
            {
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // Write to temp file
                File.WriteAllBytes(TempSavePath, encryptedBytes);

                // If primary save exists, copy to backup
                if (File.Exists(PrimarySavePath))
                {
                    File.Copy(PrimarySavePath, BackupSavePath, overwrite: true);
                }

                // Replace primary with temp
                if (File.Exists(PrimarySavePath))
                {
                    File.Delete(PrimarySavePath);
                }
                File.Move(TempSavePath, PrimarySavePath);

                Debug.Log("[Lattirune.Save] Save file written successfully.");
                return SaveResult.Success();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Lattirune.Save] File write error: {ex.Message}");
                return SaveResult.Failed($"File I/O error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads saved data with decryption, validation, and automatic backup recovery on corruption.
        /// </summary>
        public LoadResult Load()
        {
            // 1. Check if any save exists
            if (!HasSave())
            {
                SaveData defaultSave = SaveData.CreateDefault();
                return LoadResult.NoSave(defaultSave);
            }

            // 2. Try loading primary save
            if (File.Exists(PrimarySavePath))
            {
                SaveData primaryData = ReadAndProcessFile(PrimarySavePath);
                if (primaryData != null)
                {
                    return LoadResult.Success(primaryData);
                }
                Debug.LogWarning("[Lattirune.Save] Primary save corrupted or unreadable. Attempting backup recovery...");
            }

            // 3. Try loading backup save
            if (File.Exists(BackupSavePath))
            {
                SaveData backupData = ReadAndProcessFile(BackupSavePath);
                if (backupData != null)
                {
                    Debug.Log("[Lattirune.Save] Successfully recovered save from backup.");
                    // Restore backup to primary
                    File.Copy(BackupSavePath, PrimarySavePath, overwrite: true);
                    return LoadResult.RecoveredFromBackup(backupData);
                }
            }

            // 4. Both primary and backup corrupted: Fallback to safe default save
            Debug.LogError("[Lattirune.Save] Both primary and backup saves are corrupt. Creating clean default profile.");
            SaveData fallbackDefault = SaveData.CreateDefault();
            Save(fallbackDefault);
            return LoadResult.Corrupt(fallbackDefault, "Primary and backup saves corrupt. Reset to clean default.");
        }

        public bool DeleteSave()
        {
            try
            {
                if (File.Exists(PrimarySavePath)) File.Delete(PrimarySavePath);
                if (File.Exists(BackupSavePath)) File.Delete(BackupSavePath);
                if (File.Exists(TempSavePath)) File.Delete(TempSavePath);
                Debug.Log("[Lattirune.Save] Save files deleted.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Lattirune.Save] Delete save error: {ex.Message}");
                return false;
            }
        }

        private SaveData ReadAndProcessFile(string filePath)
        {
            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(filePath);
                if (encryptedBytes == null || encryptedBytes.Length == 0) return null;

                string json = SaveEncryption.DecryptBytesToString(encryptedBytes);
                if (string.IsNullOrEmpty(json)) return null;

                SaveData data = SaveSerializer.DeserializeFromJson(json);
                if (data == null) return null;

                if (!SaveValidator.ValidateSaveData(data, out List<string> errors))
                {
                    Debug.LogWarning($"[Lattirune.Save] File '{filePath}' failed validation: {string.Join("; ", errors)}");
                    return null;
                }

                data = SaveMigration.MigrateIfNeeded(data);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Lattirune.Save] Failed reading '{filePath}': {ex.Message}");
                return null;
            }
        }
    }
}
