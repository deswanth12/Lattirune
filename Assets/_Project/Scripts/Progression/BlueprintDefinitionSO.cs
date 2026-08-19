using UnityEngine;

namespace Lattirune.Progression
{
    /// <summary>
    /// Static ScriptableObject defining an unlockable blueprint at the Blueprint Forge.
    /// Stores metadata, category, Ember cost, unlock payload, and prerequisites.
    /// Strictly adheres to PLAN.md Section 12 and Section 22.
    /// </summary>
    [CreateAssetMenu(fileName = "Blueprint_", menuName = "Lattirune/Progression/Blueprint Definition")]
    public class BlueprintDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string blueprintId = "bp_shortbow";
        [SerializeField] private string displayName = "Shortbow Blueprint";
        [SerializeField] [TextArea(2, 4)] private string description = "Permanently unlocks the Shortbow into the dungeon item reward pool.";

        [Header("Forge Parameters")]
        [SerializeField] private BlueprintCategory category = BlueprintCategory.Weapon;
        [SerializeField] private int emberCost = 50;
        [SerializeField] private string targetUnlockId = "item_shortbow";
        [SerializeField] private string prerequisiteBlueprintId = null;

        public string BlueprintId => blueprintId;
        public string DisplayName => displayName;
        public string Description => description;
        public BlueprintCategory Category => category;
        public int EmberCost => emberCost;
        public string TargetUnlockId => targetUnlockId;
        public string PrerequisiteBlueprintId => prerequisiteBlueprintId;
        public bool HasPrerequisite => !string.IsNullOrEmpty(prerequisiteBlueprintId);

        public void Initialize(
            string id,
            string name,
            string desc,
            BlueprintCategory cat,
            int cost,
            string targetId,
            string prereqId = null)
        {
            blueprintId = id;
            displayName = name;
            description = desc;
            category = cat;
            emberCost = Mathf.Max(1, cost);
            targetUnlockId = targetId;
            prerequisiteBlueprintId = prereqId;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(blueprintId))
            {
                error = "Blueprint ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(targetUnlockId))
            {
                error = "Target Unlock ID cannot be empty.";
                return false;
            }
            if (emberCost <= 0)
            {
                error = "Ember cost must be greater than 0.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
