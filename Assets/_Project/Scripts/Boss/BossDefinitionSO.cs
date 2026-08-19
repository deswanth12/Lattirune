using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Boss
{
    /// <summary>
    /// Static ScriptableObject defining complete Boss attributes, base parameters, and multi-phase configurations.
    /// Maps The Lich Lord specifications according to PLAN.md Section 4.2 / 10.
    /// </summary>
    [CreateAssetMenu(fileName = "Boss_", menuName = "Lattirune/Boss/Boss Definition")]
    public class BossDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string bossId = "boss_lich_lord";
        [SerializeField] private string displayName = "The Lich Lord";

        [Header("Base Combat Stats")]
        [SerializeField] private int maxHp = 750;
        [SerializeField] private int baseArmor = 10;
        [SerializeField] private int baseAttack = 8;
        [SerializeField] private float baseAttackInterval = 2.5f;

        [Header("Phases (Ordered descending by threshold)")]
        [SerializeField] private List<BossPhaseDefinitionSO> phases = new List<BossPhaseDefinitionSO>();

        public string BossId => bossId;
        public string DisplayName => displayName;
        public int MaxHp => maxHp;
        public int BaseArmor => baseArmor;
        public int BaseAttack => baseAttack;
        public float BaseAttackInterval => baseAttackInterval;
        public IReadOnlyList<BossPhaseDefinitionSO> Phases => phases;
        public int PhaseCount => phases != null ? phases.Count : 0;

        public void Initialize(
            string id,
            string name,
            int hp,
            int armor,
            int attack,
            float interval,
            List<BossPhaseDefinitionSO> phaseList)
        {
            bossId = id;
            displayName = name;
            maxHp = Mathf.Max(1, hp);
            baseArmor = Mathf.Max(0, armor);
            baseAttack = Mathf.Max(1, attack);
            baseAttackInterval = Mathf.Max(0.2f, interval);
            phases = phaseList ?? new List<BossPhaseDefinitionSO>();
        }

        public BossPhaseDefinitionSO GetPhase(int index)
        {
            if (phases == null || index < 0 || index >= phases.Count) return null;
            return phases[index];
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(bossId))
            {
                error = "Boss ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (maxHp <= 0)
            {
                error = "Max HP must be greater than 0.";
                return false;
            }
            if (phases == null || phases.Count == 0)
            {
                error = "Boss must define at least one phase.";
                return false;
            }

            for (int i = 0; i < phases.Count; i++)
            {
                if (phases[i] == null)
                {
                    error = $"Null phase definition at index {i}.";
                    return false;
                }
                if (!phases[i].IsValid(out string pErr))
                {
                    error = $"Phase at index {i} is invalid: {pErr}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Factory creating canonical The Lich Lord 3-phase configuration from PLAN.md Section 4.2 / 10.
        /// </summary>
        public static BossDefinitionSO CreateLichLordDefinition()
        {
            BossDefinitionSO boss = ScriptableObject.CreateInstance<BossDefinitionSO>();
            List<BossPhaseDefinitionSO> phaseList = new List<BossPhaseDefinitionSO>();

            // Phase 1: Frost Warden (100% -> 66%)
            BossPhaseDefinitionSO p1 = ScriptableObject.CreateInstance<BossPhaseDefinitionSO>();
            p1.Initialize("phase_1_frost_warden", "Phase 1: Frost Warden", threshold: 1.0f, extraArmor: 0, extraAttack: 0, speedMult: 1.0f);
            phaseList.Add(p1);

            // Phase 2: Soul Harvest (66% -> 33%)
            BossPhaseDefinitionSO p2 = ScriptableObject.CreateInstance<BossPhaseDefinitionSO>();
            p2.Initialize("phase_2_soul_harvest", "Phase 2: Soul Harvest", threshold: 0.66f, extraArmor: 5, extraAttack: 4, speedMult: 0.8f);
            phaseList.Add(p2);

            // Phase 3: Necrotic Inversion (33% -> 0%)
            BossPhaseDefinitionSO p3 = ScriptableObject.CreateInstance<BossPhaseDefinitionSO>();
            p3.Initialize("phase_3_necrotic_inversion", "Phase 3: Necrotic Inversion", threshold: 0.33f, extraArmor: 10, extraAttack: 8, speedMult: 0.64f);
            phaseList.Add(p3);

            boss.Initialize("boss_lich_lord", "The Lich Lord", hp: 750, armor: 10, attack: 8, interval: 2.5f, phaseList);
            return boss;
        }
    }
}
