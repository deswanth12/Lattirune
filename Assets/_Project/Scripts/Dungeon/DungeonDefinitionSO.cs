using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Static ScriptableObject defining the complete multi-floor structure of a Dungeon.
    /// Maps ordered floors and encounters according to PLAN.md Phase 2 and Phase 3 specifications.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_", menuName = "Lattirune/Dungeon/Dungeon Definition")]
    public class DungeonDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string dungeonId = "dungeon_cursed_sewers_slice";
        [SerializeField] private string dungeonName = "The Cursed Sewers";

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
            EncounterDefinitionSO enc1 = EncounterDefinitionSO.CreateSewerRat();
            f1.Initialize(1, "floor_01", "Floor 1: Sewer Entry", new List<EncounterDefinitionSO> { enc1 });
            floorList.Add(f1);

            // Floor 2: Elite Encounter (Armored Skeleton)
            DungeonFloorDefinitionSO f2 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc2 = EncounterDefinitionSO.CreateArmoredSkeleton();
            f2.Initialize(2, "floor_02", "Floor 2: Armory Cellar", new List<EncounterDefinitionSO> { enc2 });
            floorList.Add(f2);

            // Floor 3: Boss Encounter (The Lich Lord)
            DungeonFloorDefinitionSO f3 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc3 = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc3.Initialize("enc_f3_lich", "Boss Chamber: The Lich Lord", "The Lich Lord", hp: 750, armor: 10, attack: 8, interval: 2.5f, boss: true);
            f3.Initialize(3, "floor_03", "Floor 3: Boss Sanctum", new List<EncounterDefinitionSO> { enc3 });
            floorList.Add(f3);

            dungeon.Initialize("dungeon_cursed_sewers_slice", "The Cursed Sewers (Vertical Slice)", floorList);
            return dungeon;
        }

        /// <summary>
        /// Creates the complete 10-Floor Biome 1 ("The Cursed Sewers") Dungeon as specified in PLAN.md Section 11.
        /// </summary>
        public static DungeonDefinitionSO Create10FloorCursedSewersDungeon()
        {
            DungeonDefinitionSO dungeon = ScriptableObject.CreateInstance<DungeonDefinitionSO>();
            List<DungeonFloorDefinitionSO> floorList = new List<DungeonFloorDefinitionSO>();

            // Floor 1: Sewer Rat Skirmish
            DungeonFloorDefinitionSO f1 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f1.Initialize(1, "floor_01", "Floor 1: Sewer Entry", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateSewerRat() });
            floorList.Add(f1);

            // Floor 2: Goblin Thief Ambush
            DungeonFloorDefinitionSO f2 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f2.Initialize(2, "floor_02", "Floor 2: Drain Basin", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateGoblinThief() });
            floorList.Add(f2);

            // Floor 3: Elite: Acid Slime
            DungeonFloorDefinitionSO f3 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f3.Initialize(3, "floor_03", "Floor 3: Slime Cavern", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateAcidSlime() });
            floorList.Add(f3);

            // Floor 4: Merchant Stall / Outpost Skirmish
            DungeonFloorDefinitionSO f4 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc4 = EncounterDefinitionSO.CreateGoblinThief();
            f4.Initialize(4, "floor_04", "Floor 4: Merchant Stall", new List<EncounterDefinitionSO> { enc4 });
            floorList.Add(f4);

            // Floor 5: Mid-Boss Challenge: Armored Skeleton
            DungeonFloorDefinitionSO f5 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f5.Initialize(5, "floor_05", "Floor 5: Armory Gate", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateArmoredSkeleton() });
            floorList.Add(f5);

            // Floor 6: Treasure Vault Guard: Sewer Rat Pack
            DungeonFloorDefinitionSO f6 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc6 = EncounterDefinitionSO.CreateSewerRat();
            f6.Initialize(6, "floor_06", "Floor 6: Treasure Vault", new List<EncounterDefinitionSO> { enc6 });
            floorList.Add(f6);

            // Floor 7: Elite: Necromancer
            DungeonFloorDefinitionSO f7 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f7.Initialize(7, "floor_07", "Floor 7: Bone Crypt", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateNecromancer() });
            floorList.Add(f7);

            // Floor 8: Campfire Rest Site / Spider Sentry
            DungeonFloorDefinitionSO f8 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc8 = EncounterDefinitionSO.CreateVenomousSpider();
            f8.Initialize(8, "floor_08", "Floor 8: Campfire Rest Site", new List<EncounterDefinitionSO> { enc8 });
            floorList.Add(f8);

            // Floor 9: Pre-Boss Nest: Venomous Spider
            DungeonFloorDefinitionSO f9 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            f9.Initialize(9, "floor_09", "Floor 9: Spider Nest", new List<EncounterDefinitionSO> { EncounterDefinitionSO.CreateVenomousSpider() });
            floorList.Add(f9);

            // Floor 10: Boss Chamber: The Lich Lord
            DungeonFloorDefinitionSO f10 = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            EncounterDefinitionSO enc10 = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc10.Initialize("enc_f10_lich", "Boss Chamber: The Lich Lord", "The Lich Lord", hp: 750, armor: 10, attack: 8, interval: 2.5f, boss: true);
            f10.Initialize(10, "floor_10", "Floor 10: Boss Sanctum", new List<EncounterDefinitionSO> { enc10 });
            floorList.Add(f10);

            dungeon.Initialize("dungeon_cursed_sewers_full", "The Cursed Sewers (Full 10 Floors)", floorList);
            return dungeon;
        }
    }
}
