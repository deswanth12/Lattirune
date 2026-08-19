using System;

namespace Lattirune.Boss
{
    /// <summary>
    /// Read-only snapshot of runtime Boss status, active phase, and scaled attributes.
    /// </summary>
    [Serializable]
    public struct BossTelemetry
    {
        public string BossId;
        public string BossName;
        public int CurrentPhaseIndex;
        public string PhaseName;
        public int CurrentHp;
        public int MaxHp;
        public float HpPercentage;
        public int EffectiveAttack;
        public int EffectiveArmor;
        public float EffectiveAttackInterval;
        public int PhaseTransitionCount;

        public BossTelemetry(
            string id,
            string name,
            int phaseIdx,
            string phaseName,
            int hp,
            int maxHp,
            float hpPct,
            int atk,
            int arm,
            float interval,
            int transitions)
        {
            BossId = id;
            BossName = name;
            CurrentPhaseIndex = phaseIdx;
            PhaseName = phaseName;
            CurrentHp = hp;
            MaxHp = maxHp;
            HpPercentage = hpPct;
            EffectiveAttack = atk;
            EffectiveArmor = arm;
            EffectiveAttackInterval = interval;
            PhaseTransitionCount = transitions;
        }
    }
}
