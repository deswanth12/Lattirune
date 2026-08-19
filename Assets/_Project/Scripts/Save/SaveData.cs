using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Save
{
    [Serializable]
    public class SavedItemData
    {
        public string itemId;
        public int gridX;
        public int gridY;
        public int rotationDegrees;
        public bool isPlacedOnGrid;
        public float stagingPosX;
        public float stagingPosY;

        public SavedItemData() { }

        public SavedItemData(string id, int x, int y, int rot, bool placed, float stageX, float stageY)
        {
            itemId = id;
            gridX = x;
            gridY = y;
            rotationDegrees = rot;
            isPlacedOnGrid = placed;
            stagingPosX = stageX;
            stagingPosY = stageY;
        }
    }

    [Serializable]
    public class SavedRuneData
    {
        public string runeId;
        public int gridX;
        public int gridY;
        public int direction;
        public int element;
        public int range;

        public SavedRuneData() { }

        public SavedRuneData(string id, int x, int y, int dir, int elem, int r)
        {
            runeId = id;
            gridX = x;
            gridY = y;
            direction = dir;
            element = elem;
            range = r;
        }
    }

    [Serializable]
    public class SavedRunData
    {
        public bool hasActiveRun;
        public int currentFloorIndex;
        public int currentEncounterIndex;
        public int runState;

        public SavedRunData() { }

        public SavedRunData(bool active, int floorIdx, int encIdx, int state)
        {
            hasActiveRun = active;
            currentFloorIndex = floorIdx;
            currentEncounterIndex = encIdx;
            runState = state;
        }
    }

    [Serializable]
    public class SavedSettingsData
    {
        public float masterVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public bool isMuted = false;
        public bool hapticsEnabled = true;

        public SavedSettingsData() { }

        public SavedSettingsData(float master, float sfx, bool muted, bool haptics)
        {
            masterVolume = Mathf.Clamp01(master);
            sfxVolume = Mathf.Clamp01(sfx);
            isMuted = muted;
            hapticsEnabled = haptics;
        }
    }

    /// <summary>
    /// Root serializable data transfer object for player state, run progression, and settings persistence.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = SaveVersion.CURRENT_VERSION;
        public string timestamp;
        public List<SavedItemData> items = new List<SavedItemData>();
        public List<SavedRuneData> runes = new List<SavedRuneData>();
        public SavedRunData run = new SavedRunData();
        public SavedSettingsData settings = new SavedSettingsData();

        public SaveData()
        {
            timestamp = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Constructs a deterministic baseline starting state for new games or recovery fallback.
        /// </summary>
        public static SaveData CreateDefault()
        {
            SaveData defaultSave = new SaveData
            {
                version = SaveVersion.CURRENT_VERSION,
                timestamp = DateTime.UtcNow.ToString("o"),
                run = new SavedRunData(false, 0, 0, 0),
                settings = new SavedSettingsData(1.0f, 1.0f, false, true)
            };

            // Starting Runes: Fire Rune at (2,1) emitting North
            defaultSave.runes.Add(new SavedRuneData("fire_rune_01", 2, 1, 1 /* North */, 1 /* Fire */, 3));

            // Starting Items: 5 baseline prototype items in staging
            defaultSave.items.Add(new SavedItemData("item_training_sword", -1, -1, 0, false, -2.2f + (0 * 1.1f), -4.0f));
            defaultSave.items.Add(new SavedItemData("item_ember_blade", -1, -1, 0, false, -2.2f + (1 * 1.1f), -4.0f));
            defaultSave.items.Add(new SavedItemData("item_guard_plate", -1, -1, 0, false, -2.2f + (2 * 1.1f), -4.0f));
            defaultSave.items.Add(new SavedItemData("item_arcane_relic", -1, -1, 0, false, -2.2f + (3 * 1.1f), -4.0f));
            defaultSave.items.Add(new SavedItemData("item_vital_flask", -1, -1, 0, false, -2.2f + (4 * 1.1f), -4.0f));

            return defaultSave;
        }
    }
}
