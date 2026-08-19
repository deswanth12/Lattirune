using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Static ScriptableObject defining a discrete floor in a multi-floor dungeon run.
    /// Contains ordered encounter configurations for the floor.
    /// </summary>
    [CreateAssetMenu(fileName = "Floor_", menuName = "Lattirune/Dungeon/Floor Definition")]
    public class DungeonFloorDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private int floorNumber = 1;
        [SerializeField] private string floorId = "floor_01";
        [SerializeField] private string displayName = "Floor 1: Sewer Entry";

        [Header("Encounters")]
        [SerializeField] private List<EncounterDefinitionSO> encounters = new List<EncounterDefinitionSO>();

        public int FloorNumber => floorNumber;
        public string FloorId => floorId;
        public string DisplayName => displayName;
        public IReadOnlyList<EncounterDefinitionSO> Encounters => encounters;
        public int EncounterCount => encounters != null ? encounters.Count : 0;

        public void Initialize(int num, string id, string name, List<EncounterDefinitionSO> encs)
        {
            floorNumber = num;
            floorId = id;
            displayName = name;
            encounters = encs ?? new List<EncounterDefinitionSO>();
        }

        public EncounterDefinitionSO GetEncounter(int index)
        {
            if (encounters == null || index < 0 || index >= encounters.Count) return null;
            return encounters[index];
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(floorId))
            {
                error = "Floor ID cannot be empty.";
                return false;
            }
            if (encounters == null || encounters.Count == 0)
            {
                error = "Floor must contain at least one encounter.";
                return false;
            }

            for (int i = 0; i < encounters.Count; i++)
            {
                if (encounters[i] == null)
                {
                    error = $"Null encounter at index {i}.";
                    return false;
                }
                if (!encounters[i].IsValid(out string encErr))
                {
                    error = $"Encounter at index {i} is invalid: {encErr}";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
