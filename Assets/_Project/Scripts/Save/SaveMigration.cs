using UnityEngine;

namespace Lattirune.Save
{
    /// <summary>
    /// Handles future save data schema migrations across version boundaries.
    /// </summary>
    public static class SaveMigration
    {
        public static SaveData MigrateIfNeeded(SaveData data)
        {
            if (data == null) return null;

            if (data.version == SaveVersion.CURRENT_VERSION)
            {
                return data;
            }

            // Future schema migrations will be handled here sequentially:
            // if (data.version == 1) { MigrateV1ToV2(data); }

            Debug.Log($"[Lattirune.Save] Migrated save data from version {data.version} to {SaveVersion.CURRENT_VERSION}.");
            data.version = SaveVersion.CURRENT_VERSION;
            return data;
        }
    }
}
