using System;
using UnityEngine;

namespace Lattirune.Save
{
    /// <summary>
    /// Handles JSON serialization and deserialization for SaveData payloads.
    /// </summary>
    public static class SaveSerializer
    {
        public static string SerializeToJson(SaveData data, bool prettyPrint = true)
        {
            if (data == null)
            {
                Debug.LogWarning("[Lattirune.Save] Cannot serialize null SaveData.");
                return null;
            }

            try
            {
                return JsonUtility.ToJson(data, prettyPrint);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Lattirune.Save] Serialization error: {ex.Message}");
                return null;
            }
        }

        public static SaveData DeserializeFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[Lattirune.Save] Cannot deserialize null or empty JSON string.");
                return null;
            }

            try
            {
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Lattirune.Save] Deserialization error: {ex.Message}");
                return null;
            }
        }
    }
}
