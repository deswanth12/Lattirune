using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Monetization;

namespace Lattirune.Tests
{
    [TestFixture]
    public class MonetizationServiceTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""MonetizationTestHolder"");
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        [Test]
        public void RewardedAd_InvokesRewardCallbackSuccessfully()
        {
            var service = _holder.AddComponent<OfflineMonetizationService>();
            service.Initialize(purchasedNoAds: false, networkAvailable: true);

            bool rewarded = false;
            bool failed = false;

            service.ShowRewardedAd(AdRewardType.MerchantFreeReroll, () => rewarded = true, () => failed = true);

            Assert.IsTrue(rewarded);
            Assert.IsFalse(failed);
        }

        [Test]
        public void PurchaseNoAds_GrantsInstantEntitlement()
        {
            var service = _holder.AddComponent<OfflineMonetizationService>();
            service.Initialize(purchasedNoAds: false, networkAvailable: false);

            Assert.IsFalse(service.HasPurchasedNoAdsEmberBoost);

            bool purchaseSuccess = false;
            service.PurchaseNoAdsEmberBoost(() => purchaseSuccess = true, null);

            Assert.IsTrue(purchaseSuccess);
            Assert.IsTrue(service.HasPurchasedNoAdsEmberBoost);
            // Ads are always treated as available when owning No-Ads pass
            Assert.IsTrue(service.IsAdAvailable(AdRewardType.DungeonRevive50Percent));
        }

        [Test]
        public void RunManager_RevivePlayer_RestoresHpAndEnforcesSingleUsePerRun()
        {
            var runManager = _holder.AddComponent<RunManager>();
            var combat = _holder.AddComponent<CombatSystem>();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject(""Enemy"");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(100, 0, 10, 1.0f);

            combat.Initialize(player, enemy);
            runManager.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), combat, null, player, enemy);
            runManager.StartRun();

            // Simulate Defeat
            runManager.StartEncounterCombat();
            player.TakeDirectDamage(100); // Player dies
            combat.Tick(0.1f); // Triggers defeat

            Assert.AreEqual(RunState.Defeated, runManager.CurrentState);
            Assert.IsFalse(runManager.HasUsedReviveThisRun);

            // Execute 1st Revive (50% HP = 50 HP)
            bool revived = runManager.RevivePlayer(0.5f);
            Assert.IsTrue(revived);
            Assert.AreEqual(RunState.EncounterActive, runManager.CurrentState);
            Assert.AreEqual(50, player.CurrentHp);
            Assert.IsTrue(runManager.HasUsedReviveThisRun);

            // Simulate 2nd Death
            player.TakeDirectDamage(50);
            combat.Tick(0.1f);
            Assert.AreEqual(RunState.Defeated, runManager.CurrentState);

            // Attempt 2nd Revive -> Must Fail!
            bool secondRevive = runManager.RevivePlayer(0.5f);
            Assert.IsFalse(secondRevive);
            Assert.AreEqual(RunState.Defeated, runManager.CurrentState);
        }
    }
}
