using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Data-driven enemy combatant supporting unique PLAN.md traits
    /// (Gold steal, Damage reflection, Poison on hit, Bag disable, Summoning).
    /// </summary>
    public class EnemyCombatant : Combatant
    {
        [Header("Enemy Attack Stats")]
        [SerializeField] private int baseAttackDamage = 4;

        private readonly List<EnemyTraitDefinitionSO> _activeTraits = new List<EnemyTraitDefinitionSO>();

        public event Action<int> OnGoldStolen;
        public event Action<int> OnDamageReflected;
        public event Action<int> OnPoisonInflicted;
        public event Action OnBagSlotDisabled;
        public event Action<int> OnMinionsSummoned;

        public int BaseAttackDamage => baseAttackDamage;
        public int BaseDamage => baseAttackDamage;
        public IReadOnlyList<EnemyTraitDefinitionSO> ActiveTraits => _activeTraits;

        public void SetupTrainingDummy(int hp = 50, int baseArmor = 2, int attack = 4, float interval = 1.5f)
        {
            baseAttackDamage = attack;
            _activeTraits.Clear();
            Initialize("Training Dummy", hp, baseArmor, interval);
        }

        public void SetupSewerRat() => SetupCustom("Sewer Rat", 35, 0, 3, 1.2f);
        public void SetupGoblinThief() => SetupCustom("Goblin Thief", 45, 0, 4, 1.0f);
        public void SetupArmoredSkeleton() => SetupCustom("Armored Skeleton", 75, 15, 5, 2.0f);
        public void SetupVenomousSpider() => SetupCustom("Venomous Spider", 50, 0, 4, 1.4f);
        public void SetupAcidSlime() => SetupCustom("Acid Slime", 160, 2, 6, 2.0f);
        public void SetupNecromancer() => SetupCustom("Necromancer", 140, 0, 5, 3.0f);
        public void SetupSlime() => SetupCustom("Acid Slime", 160, 2, 6, 2.0f);
        public void SetupSkeleton() => SetupCustom("Armored Skeleton", 75, 15, 5, 2.0f);

        public void SetupCustom(
            string name,
            int hp,
            int baseArmor,
            int attack,
            float interval,
            IEnumerable<EnemyTraitDefinitionSO> traits = null)
        {
            baseAttackDamage = attack;
            _activeTraits.Clear();
            if (traits != null)
            {
                _activeTraits.AddRange(traits);
            }
            Initialize(name, hp, baseArmor, interval);
        }

        public void SetEffectiveStats(int newArmor, int newAttack, float newInterval)
        {
            baseAttackDamage = newAttack;
            SetStats(MaxHp, newArmor, newInterval);
        }

        public void AddTrait(EnemyTraitDefinitionSO trait)
        {
            if (trait != null && !_activeTraits.Contains(trait))
            {
                _activeTraits.Add(trait);
            }
        }

        public void TriggerAttackTraits(Combatant target, DamageResult damage)
        {
            if (!IsAlive || target == null) return;

            for (int i = 0; i < _activeTraits.Count; i++)
            {
                var trait = _activeTraits[i];
                if (trait == null) continue;

                if (trait.TraitType == EnemyTraitType.GoldSteal)
                {
                    int stolenGold = Mathf.Max(1, Mathf.RoundToInt(trait.TraitValue));
                    OnGoldStolen?.Invoke(stolenGold);
                }
                else if (trait.TraitType == EnemyTraitType.ApplyPoisonOnHit)
                {
                    int poisonStacks = Mathf.Max(1, Mathf.RoundToInt(trait.TraitValue));
                    OnPoisonInflicted?.Invoke(poisonStacks);
                }
            }
        }

        public void TriggerEncounterStartTraits()
        {
            if (!IsAlive) return;

            for (int i = 0; i < _activeTraits.Count; i++)
            {
                var trait = _activeTraits[i];
                if (trait != null && trait.TraitType == EnemyTraitType.DisableBagSlot)
                {
                    OnBagSlotDisabled?.Invoke();
                }
            }
        }

        public int CalculateDamageReflect(DamageResult incomingDamage)
        {
            if (!IsAlive || incomingDamage == null || incomingDamage.IsReflected) return 0;

            for (int i = 0; i < _activeTraits.Count; i++)
            {
                var trait = _activeTraits[i];
                if (trait != null && trait.TraitType == EnemyTraitType.DamageReflect)
                {
                    int reflectAmount = Mathf.Max(1, Mathf.RoundToInt(incomingDamage.FinalDamage * trait.TraitValue));
                    OnDamageReflected?.Invoke(reflectAmount);
                    return reflectAmount;
                }
            }
            return 0;
        }
    }
}
