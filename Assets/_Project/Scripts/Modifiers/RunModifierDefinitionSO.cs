using UnityEngine;

namespace Lattirune.Modifiers
{
    /// <summary>
    /// Data-driven ScriptableObject defining an immutable run modifier for Lattirune 1.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RunModifier", menuName = "Lattirune/Modifiers/Run Modifier Definition")]
    public class RunModifierDefinitionSO : ScriptableObject
    {
        [SerializeField] private string modifierId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private RunModifierRarity rarity = RunModifierRarity.Common;
        [SerializeField] private RunModifierPolarity polarity = RunModifierPolarity.Positive;
        [SerializeField] private RunModifierType modifierType = RunModifierType.DamageMultiplier;
        [SerializeField] private float effectValue = 1.0f;
        [SerializeField] private Color iconColor = Color.white;

        public string ModifierId => modifierId;
        public string DisplayName => displayName;
        public string Description => description;
        public RunModifierRarity Rarity => rarity;
        public RunModifierPolarity Polarity => polarity;
        public RunModifierType ModifierType => modifierType;
        public float EffectValue => effectValue;
        public Color IconColor => iconColor;

        public void Initialize(
            string id, 
            string name, 
            string desc, 
            RunModifierRarity rar, 
            RunModifierPolarity pol, 
            RunModifierType type, 
            float val, 
            Color color = default)
        {
            modifierId = id;
            displayName = name;
            description = desc;
            rarity = rar;
            polarity = pol;
            modifierType = type;
            effectValue = val;
            iconColor = color == default ? Color.white : color;
        }
    }
}
