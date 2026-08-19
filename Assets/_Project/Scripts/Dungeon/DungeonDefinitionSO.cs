using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Static ScriptableObject defining the complete multi-floor structure of a Dungeon.
    /// Maps ordered floors and encounters according to PLAN.md Phase 2 Vertical Slice specifications.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_", menuName = "Lattirune/Dungeon/Dungeon Definition")]
    public class DungeonDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string dungeonId = "dungeon_cursed_sewers_slice";
        [SerializeField] private string dungeonName = "The Cursed Sewers (Phase 2 Slice)";

        [Header("Floors")]
        [SerializeField] private List<DungeonFloorDefinitionSO> floors = new List<DungeonFloorDefinitionSO>();

        public string DungeonId => dungeonId;
        public string DungeonName => dungeonName;
        public IReadOnlyList<DungeonFloorDefinitionSO> Floors => floors;
        public int TotalFloorCount => floors != null ? floors.Count : 0;

        public void Initialize(string id, string name, List<DungeonFloorDefinitionSO> floorList)
        {
            dungeonId = id;
            dungeonName = name;
            floors = floorList ?? new List<DungeonFloorDefinitionSO>();
        }

        public DungeonFloorDefinitionSO GetFloor(int index)
        {
            if (floors == null || index < 0 || index >= floors.Count) return null;
            return floors[index];
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(dungeonId))
            {
                error = "Dungeon ID cannot be empty.";
                return false;
            }
            if (floors == null || floors.Count == 0)
            {
                error = "Dungeon must contain at least one floor.";
                return false;
            }

            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i] == null)
                {
                    error = $"Null floor at index {i}.";
                    return false;
                }
                if (!floors[i].IsValid(out string floorErr))
                {
                    error = $"Floor at index {i} is invalid: {floorErr}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Creates the canonical 3-floor Phase 2 Vertical Slice Dungeon as specified in PLAN.md Section 1.2 / 2.
        /// </summary>
        public static DungeonDefinitionSO CreateDefaultPhase2Dungeon()
        {
            DungeonDefinitionSO dungeon = ScriptableObject.CreateInstance<DungeonDefinitionSO>();
            List<DungeonFloorDefinitionSO> floorList = new List<DungeonFloorDefinitionSO>();

            // Floor 1: Normal Encounter (Sewer Rat)
            DungeonFloorDefinitionSO f1 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc1 = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc1.Initialize("enc_f1_rat", "Sewer Rat Skirmish", "Sewer Rat", hp: 40, armor: 1, attack: 3, interval: 1.5f, boss: false);
            f1.Initialize(1, "floor_01", "Floor 1: Sewer Entry", new List<EncounterDefinitionSO> { enc1 });
            floorList.Add(f1);

            // Floor 2: Elite Encounter (Armored Skeleton)
            DungeonFloorDefinitionSO f2 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc2 = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc2.Initialize("enc_f2_skeleton", "Armored Skeleton Horde", "Armored Skeleton", hp: 60, armor: 3, attack: 5, interval: 1.4f, boss: false);
            f2.Initialize(2, "floor_02", "Floor 2: Armory Cellar", new List<EncounterDefinitionSO> { enc2 });
            floorList.Add(f2);

            // Floor 3: Boss Encounter (The Lich Lord)
            DungeonFloorDefinitionSO f3 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc3 = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc3.Initialize("enc_f3_lich", "Boss Chamber: The Lich Lord", "Lich Lord", hp: 100, armor: 4, attack: 8, interval: 1.2f, boss: true);
            f3.Initialize(3, "floor_03", "Floor 3: Boss Chamber", new List<EncounterDefinitionSO> { enc3 });
            floorList.Add(f3);

            dungeon.Initialize("dungeon_cursed_sewers_slice", "The Cursed Sewers", floorList);
            return dungeon;
        }
    }
}
