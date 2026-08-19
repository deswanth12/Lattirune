using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Core;
using Lattirune.Reactions;

namespace Lattirune.Tests
{
    [TestFixture]
    public class ReactionCombatIntegrationTests
    {
        private GameObject _holderObj;
        private CombatSystem _combatSystem;
        private CombatEffectSystem _effectSystem;
        private CombatEffectDatabaseSO _effectDatabase;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ReactionCombatIntegrationHolder");
            _effectDatabase = CombatEffectDatabaseSO.CreateDefaultDatabase();

            _effectSystem = _holderObj.AddComponent<CombatEffectSystem>();
            _effectSystem.Initialize(_effectDatabase);

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(initialHp: 100);
            _player.SetExplicitStats(baseDamage: 10, runeBonus: 5, armorValue: 0, interval: 1.0f);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupTrainingDummy(hp: 100, baseArmor: 10, attack: 10, interval: 1.0f);

            _combatSystem = _holderObj.AddComponent<CombatSystem>();
            _combatSystem.Initialize(_player, _enemy, _effectSystem);
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        [Test]
        public void ReactionCombat_Steam_InflictsBlindAttackReduction()
        {
            // Steam reaction applied to enemy
            ElementalReactionDefinitionSO steamDef = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            steamDef.Initialize("reaction_steam", "Steam", "Desc", ElementType.Fire, ElementType.Ice, Color.white);
            BeamIntersection inter = new BeamIntersection(new Vector2Int(2, 2), "fire_1", "ice_1", ElementType.Fire, ElementType.Ice, ConduitDirection.North, ConduitDirection.East);
            ElementalReactionResult steamResult = ElementalReactionResult.CreateActive(steamDef, inter);

            CombatEffectInstance effect = ReactionEffectResolver.ResolveEffect(steamResult, _effectDatabase, _enemy);
            _effectSystem.ApplyEffect(effect);

            _combatSystem.StartCombat();

            // Enemy attack base 10 * 0.75 (from Steam 25% blind) = 8 damage dealt to player
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(92, _player.CurrentHp);
        }

        [Test]
        public void ReactionCombat_Superconductor_ShredsArmorInDamagePipeline()
        {
            // Superconductor reaction applied to enemy (reduces 10 armor to 6 armor)
            ElementalReactionDefinitionSO superDef = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            superDef.Initialize("reaction_superconductor", "Superconductor", "Desc", ElementType.Lightning, ElementType.Ice, Color.white);
            BeamIntersection inter = new BeamIntersection(new Vector2Int(2, 2), "lightning_1", "ice_1", ElementType.Lightning, ElementType.Ice, ConduitDirection.North, ConduitDirection.East);
            ElementalReactionResult superResult = ElementalReactionResult.CreateActive(superDef, inter);

            CombatEffectInstance effect = ReactionEffectResolver.ResolveEffect(superResult, _effectDatabase, _enemy);
            _effectSystem.ApplyEffect(effect);

            _combatSystem.StartCombat();

            // Player attacks: (10 base + 5 fire rune) - 6 shredded armor = 9 damage dealt
            // Without shred, damage would be (15 - 10 = 5).
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(91, _enemy.CurrentHp);
        }

        [Test]
        public void ReactionCombat_Frostbite_IncreasesDamageVulnerability()
        {
            // Frostbite reaction applied to enemy (+50% damage intake)
            ElementalReactionDefinitionSO frostDef = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            frostDef.Initialize("reaction_frostbite", "Frostbite", "Desc", ElementType.Ice, ElementType.Poison, Color.white);
            BeamIntersection inter = new BeamIntersection(new Vector2Int(2, 2), "ice_1", "poison_1", ElementType.Ice, ElementType.Poison, ConduitDirection.North, ConduitDirection.East);
            ElementalReactionResult frostResult = ElementalReactionResult.CreateActive(frostDef, inter);

            CombatEffectInstance effect = ReactionEffectResolver.ResolveEffect(frostResult, _effectDatabase, _enemy);
            _effectSystem.ApplyEffect(effect);

            _combatSystem.StartCombat();

            // Player base 10 + 5 rune = 15 raw * 1.50 vulnerability = 22.5 (23) - 10 armor = 13 damage
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(88, _enemy.CurrentHp);
        }

        [Test]
        public void ReactionCombat_MultipleEffects_CoexistOnSameTarget()
        {
            // Apply Superconductor (-40% armor) AND Frostbite (+50% damage intake)
            CombatEffectDefinitionSO superDef = _effectDatabase.GetByEffectId("effect_superconductor_shred");
            CombatEffectDefinitionSO frostDef = _effectDatabase.GetByEffectId("effect_frostbite_vulnerability");

            _effectSystem.ApplyEffect(new CombatEffectInstance(superDef, "rune_1", "rune_2", _enemy));
            _effectSystem.ApplyEffect(new CombatEffectInstance(frostDef, "rune_3", "rune_4", _enemy));

            Assert.AreEqual(2, _effectSystem.GetActiveEffectCount(_enemy));
            Assert.AreEqual(0.60f, _effectSystem.GetArmorMultiplier(_enemy), 0.001f);
            Assert.AreEqual(1.50f, _effectSystem.GetDamageIntakeMultiplier(_enemy), 0.001f);
        }
    }
}
