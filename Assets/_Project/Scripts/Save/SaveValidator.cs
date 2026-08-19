using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Save
{
    /// <summary>
    /// Validates persisted save data models before saving or restoring to protect against corrupted or out-of-bounds state.
    /// </summary>
    public static class SaveValidator
    {
        public const int GRID_SIZE = 5;

        public static bool ValidateSaveData(SaveData data, out List<string> errors)
        {
            errors = new List<string>();

            if (data == null)
            {
                errors.Add("SaveData instance is null.");
                return false;
            }

            // 1. Version validation
            if (data.version < 1 || data.version > SaveVersion.CURRENT_VERSION)
            {
                errors.Add($"Unsupported save version: {data.version}. Current supported version is {SaveVersion.CURRENT_VERSION}.");
            }

            // 2. Timestamp sanity
            if (string.IsNullOrEmpty(data.timestamp))
            {
                errors.Add("SaveData timestamp is missing.");
            }

            // 3. Item Validation
            if (data.items != null)
            {
                for (int i = 0; i < data.items.Count; i++)
                {
                    SavedItemData item = data.items[i];
                    if (item == null)
                    {
                        errors.Add($"Null item entry found at index {i}.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(item.itemId))
                    {
                        errors.Add($"Empty Item ID found at index {i}.");
                    }

                    if (item.isPlacedOnGrid)
                    {
                        if (item.gridX < 0 || item.gridX >= GRID_SIZE || item.gridY < 0 || item.gridY >= GRID_SIZE)
                        {
                            errors.Add($"Item '{item.itemId}' has invalid grid coordinates ({item.gridX}, {item.gridY}). Must be in [0, 4].");
                        }
                    }

                    if (item.rotationDegrees != 0 && item.rotationDegrees != 90 && item.rotationDegrees != 180 && item.rotationDegrees != 270)
                    {
                        errors.Add($"Item '{item.itemId}' has invalid rotation: {item.rotationDegrees}°. Allowed: 0, 90, 180, 270.");
                    }
                }
            }

            // 4. Rune Validation
            if (data.runes != null)
            {
                for (int i = 0; i < data.runes.Count; i++)
                {
                    SavedRuneData rune = data.runes[i];
                    if (rune == null) continue;

                    if (rune.gridX < 0 || rune.gridX >= GRID_SIZE || rune.gridY < 0 || rune.gridY >= GRID_SIZE)
                    {
                        errors.Add($"Rune '{rune.runeId}' has invalid grid position ({rune.gridX}, {rune.gridY}).");
                    }

                    if (rune.range < 1 || rune.range > GRID_SIZE)
                    {
                        errors.Add($"Rune '{rune.runeId}' has invalid range: {rune.range}.");
                    }
                }
            }

            // 5. Settings Validation
            if (data.settings != null)
            {
                if (data.settings.masterVolume < 0f || data.settings.masterVolume > 1f)
                {
                    errors.Add($"Invalid master volume setting: {data.settings.masterVolume}.");
                }

                if (data.settings.sfxVolume < 0f || data.settings.sfxVolume > 1f)
                {
                    errors.Add($"Invalid SFX volume setting: {data.settings.sfxVolume}.");
                }
            }

            return errors.Count == 0;
        }
    }
}
