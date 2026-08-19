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
        public List<string> activeModifierIds = new List<string>();
        public int highestCombo;

        public SavedRunData() 
        {
            activeModifierIds = new List<string>();
            highestCombo = 0;
        }

        public SavedRunData(bool active, int floorIdx, int encIdx, int state, IEnumerable<string> modifierIds = null, int combo = 0)
        {
            hasActiveRun = active;
            currentFloorIndex = floorIdx;
            currentEncounterIndex = encIdx;
            runState = state;
            activeModifierIds = modifierIds != null ? new List<string>(modifierIds) : new List<string>();
            highestCombo = combo;
        }
    }

    [Serializable]
    public class SavedInventoryData
    {
        public int expansionStep;
        public List<int> unlockedX = new List<int>();
        public List<int> unlockedY = new List<int>();

        public SavedInventoryData() { }

        public SavedInventoryData(int step, IEnumerable<Vector2Int> unlocked)
        {
            expansionStep = step;
            if (unlocked != null)
            {
                foreach (var pos in unlocked)
                {
                    unlockedX.Add(pos.x);
                    unlockedY.Add(pos.y);
                }
            }
        }

        public List<Vector2Int> GetCoordinates()
        {
            List<Vector2Int> list = new List<Vector2Int>();
            int count = Mathf.Min(unlockedX != null ? unlockedX.Count : 0, unlockedY != null ? unlockedY.Count : 0);
            for (int i = 0; i < count; i++)
            {
                list.Add(new Vector2Int(unlockedX[i], unlockedY[i]));
            }
            return list;
        }
    }

    [Serializable]
    public class SavedMetaData
    {
        public int embers;
        public List<string> unlockedBlueprints = new List<string>();
        public int totalBossClears;
        public int totalRunsAttempted;
        public string selectedHeroClass = "class_rune_knight";
        public List<string> unlockedHeroClasses = new List<string>();

        public SavedMetaData() 
        {
            selectedHeroClass = "class_rune_knight";
            unlockedHeroClasses = new List<string> { "class_rune_knight" };
        }

        public SavedMetaData(
            int emberCount, 
            IEnumerable<string> blueprints, 
            int bossClears = 0, 
            int runs = 0,
            string selectedClass = "class_rune_knight",
            IEnumerable<string> unlockedClasses = null)
        {
            embers = Mathf.Max(0, emberCount);
            if (blueprints != null)
            {
                unlockedBlueprints.AddRange(blueprints);
            }
            totalBossClears = bossClears;
            totalRunsAttempted = runs;
            selectedHeroClass = !string.IsNullOrEmpty(selectedClass) ? selectedClass : "class_rune_knight";
            unlockedHeroClasses = unlockedClasses != null ? new List<string>(unlockedClasses) : new List<string> { "class_rune_knight" };
        }
    }

    [Serializable]
    public class SavedSettingsData
    {
        public float masterVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public bool isMuted = false;
        public bool hapticsEnabled = true;
        public bool hasCompletedTutorial = false;

        public SavedSettingsData() 
        {
            hasCompletedTutorial = false;
        }

        public SavedSettingsData(float master, float sfx, bool muted, bool haptics, bool tutorial = false)
        {
            masterVolume = Mathf.Clamp01(master);
            sfxVolume = Mathf.Clamp01(sfx);
            isMuted = muted;
            hapticsEnabled = haptics;
            hasCompletedTutorial = tutorial;
        }
    }

    [Serializable]
    public class SavedCodexData
    {
        public List<string> discoveredEnemies = new List<string>();
        public List<string> enemyKillKeys = new List<string>();
        public List<int> enemyKillValues = new List<int>();
        public List<string> discoveredSynergies = new List<string>();
        public List<string> discoveredReactions = new List<string>();

        public SavedCodexData() { }

        public SavedCodexData(
            IEnumerable<string> enemies,
            IEnumerable<string> killKeys,
            IEnumerable<int> killVals,
            IEnumerable<string> synergies,
            IEnumerable<string> reactions)
        {
            if (enemies != null) discoveredEnemies.AddRange(enemies);
            if (killKeys != null) enemyKillKeys.AddRange(killKeys);
            if (killVals != null) enemyKillValues.AddRange(killVals);
            if (synergies != null) discoveredSynergies.AddRange(synergies);
            if (reactions != null) discoveredReactions.AddRange(reactions);
        }
    }

    /// <summary>
    /// Root serializable data transfer object for player state, inventory expansion, run progression, meta-progression, codex discovery, and settings persistence.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = SaveVersion.CURRENT_VERSION;
        public string timestamp;
        public List<SavedItemData> items = new List<SavedItemData>();
        public List<SavedRuneData> runes = new List<SavedRuneData>();
        public SavedRunData run = new SavedRunData();
        public SavedInventoryData inventory = new SavedInventoryData();
        public SavedMetaData meta = new SavedMetaData();
        public SavedCodexData codex = new SavedCodexData();
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
                inventory = new SavedInventoryData(0, null),
                meta = new SavedMetaData(0, null, 0, 0),
                codex = new SavedCodexData(),
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
