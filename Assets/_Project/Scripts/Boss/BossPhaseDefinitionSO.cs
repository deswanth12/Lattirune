using UnityEngine;
using Lattirune.Combat.Effects;

namespace Lattirune.Boss
{
    /// <summary>
    /// Static ScriptableObject defining an individual phase in a multi-phase Boss encounter.
    /// Configures HP percentage thresholds and stat modifiers without holding runtime state.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPhase_", menuName = "Lattirune/Boss/Boss Phase Definition")]
    public class BossPhaseDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string phaseId = "phase_1";
        [SerializeField] private string displayName = "Phase 1: Frost Warden";

        [Header("Phase Activation Threshold")]
        [Range(0f, 1f)]
        [SerializeField] private float hpThresholdPercentage = 1.0f; // Activates at or below this HP%

        [Header("Stat Modifiers")]
        [SerializeField] private int bonusArmor = 0;
        [SerializeField] private int bonusAttack = 0;
        [SerializeField] private float attackIntervalMultiplier = 1.0f;

        [Header("Optional Phase Effect")]
        [SerializeField] private CombatEffectDefinitionSO phaseEffect;

        public string PhaseId => phaseId;
        public string DisplayName => displayName;
        public string PhaseName => displayName;
        public float HpThresholdPercentage => hpThresholdPercentage;
        public int BonusArmor => bonusArmor;
        public int ArmorBonus => bonusArmor;
        public int BonusAttack => bonusAttack;
        public int AttackBonus => bonusAttack;
        public float AttackIntervalMultiplier => attackIntervalMultiplier;
        public CombatEffectDefinitionSO PhaseEffect => phaseEffect;

        public void Initialize(
            string id,
            string name,
            float threshold,
            int extraArmor = 0,
            int extraAttack = 0,
            float speedMult = 1.0f,
            CombatEffectDefinitionSO effect = null)
        {
            phaseId = id;
            displayName = name;
            hpThresholdPercentage = Mathf.Clamp01(threshold);
            bonusArmor = extraArmor;
            bonusAttack = extraAttack;
            attackIntervalMultiplier = Mathf.Max(0.1f, speedMult);
            phaseEffect = effect;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(phaseId))
            {
                error = "Phase ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (hpThresholdPercentage < 0f || hpThresholdPercentage > 1f)
            {
                error = "HP threshold must be between 0.0 and 1.0.";
                return false;
            }
            if (attackIntervalMultiplier <= 0f)
            {
                error = "Attack interval multiplier must be greater than 0.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
