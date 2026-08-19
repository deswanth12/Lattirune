using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RewardGeneratorTests
    {
        private List<ItemDataSO> _itemCatalogue;

        [SetUp]
        public void Setup()
        {
            _itemCatalogue = new List<ItemDataSO>();

            ItemDataSO sword = ScriptableObject.CreateInstance<ItemDataSO>();
            sword.Initialize("item_sword", "Training Sword", "Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);
            _itemCatalogue.Add(sword);

            ItemDataSO ember = ScriptableObject.CreateInstance<ItemDataSO>();
            ember.Initialize("item_ember", "Ember Blade", "Fire Sword", ItemCategory.Weapon, new Vector2Int(2, 1), true, Color.red);
            _itemCatalogue.Add(ember);

            ItemDataSO plate = ScriptableObject.CreateInstance<ItemDataSO>();
            plate.Initialize("item_plate", "Guard Plate", "Shield", ItemCategory.Shield, new Vector2Int(2, 2), true, Color.blue);
            _itemCatalogue.Add(plate);

            ItemDataSO relic = ScriptableObject.CreateInstance<ItemDataSO>();
            relic.Initialize("item_relic", "Arcane Relic", "Relic", ItemCategory.Relic, new Vector2Int(1, 1), false, Color.magenta);
            _itemCatalogue.Add(relic);

            ItemDataSO flask = ScriptableObject.CreateInstance<ItemDataSO>();
            flask.Initialize("item_flask", "Vital Flask", "Potion", ItemCategory.Consumable, new Vector2Int(1, 1), false, Color.green);
            _itemCatalogue.Add(flask);
        }

        [Test]
        public void RewardGenerator_GeneratesExactlyThreeOptions()
        {
            List<RewardOption> rewards = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3);

            Assert.AreEqual(3, rewards.Count);
        }

        [Test]
        public void RewardGenerator_RewardIds_AreUniqueWithinSelection()
        {
            List<RewardOption> rewards = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3);

            HashSet<string> seenIds = new HashSet<string>();
            foreach (var opt in rewards)
            {
                Assert.IsFalse(seenIds.Contains(opt.RewardId), $"Duplicate reward ID '{opt.RewardId}' found in selection.");
                seenIds.Add(opt.RewardId);
            }
        }

        [Test]
        public void RewardGenerator_Rewards_ReferenceValidItemDataAssets()
        {
            List<RewardOption> rewards = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3);

            foreach (var opt in rewards)
            {
                Assert.IsNotNull(opt.ItemData);
                Assert.IsTrue(opt.ItemData.IsValid(out string err));
                Assert.IsNull(err);
                Assert.IsFalse(string.IsNullOrEmpty(opt.DisplayName));
            }
        }

        [Test]
        public void RewardGenerator_SeededGeneration_IsDeterministic()
        {
            List<RewardOption> runA = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3, seed: 1337);
            List<RewardOption> runB = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3, seed: 1337);

            Assert.AreEqual(runA.Count, runB.Count);
            for (int i = 0; i < runA.Count; i++)
            {
                Assert.AreEqual(runA[i].RewardId, runB[i].RewardId);
                Assert.AreEqual(runA[i].DisplayName, runB[i].DisplayName);
            }
        }

        [Test]
        public void RewardGenerator_EmptyOrSmallPool_HandledGracefully()
        {
            // Empty catalog
            List<RewardOption> empty = RewardGenerator.GenerateRewardOptions(new List<ItemDataSO>(), count: 3);
            Assert.AreEqual(0, empty.Count);

            // Pool with only 2 items
            var smallPool = new List<ItemDataSO> { _itemCatalogue[0], _itemCatalogue[1] };
            List<RewardOption> smallResult = RewardGenerator.GenerateRewardOptions(smallPool, count: 3);
            Assert.AreEqual(2, smallResult.Count);
        }
    }
}
