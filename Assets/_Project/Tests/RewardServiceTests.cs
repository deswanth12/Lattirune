using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RewardServiceTests
    {
        private GameObject _serviceHolder;
        private RewardService _rewardService;
        private ItemDataSO _swordData;
        private ItemDataSO _plateData;

        [SetUp]
        public void Setup()
        {
            _serviceHolder = new GameObject("RewardServiceHolder");
            _rewardService = _serviceHolder.AddComponent<RewardService>();

            _swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _swordData.Initialize("item_sword", "Training Sword", "Basic Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            _plateData = ScriptableObject.CreateInstance<ItemDataSO>();
            _plateData.Initialize("item_plate", "Guard Plate", "Shield", ItemCategory.Shield, new Vector2Int(2, 2), true, Color.blue);
        }

        [TearDown]
        public void Teardown()
        {
            if (_serviceHolder != null)
            {
                Object.DestroyImmediate(_serviceHolder);
            }
        }

        [Test]
        public void RewardService_ApplyReward_CreatesValidItemInstance()
        {
            RewardOption option = RewardOption.FromItemData(_swordData);
            ItemInstance instance = _rewardService.ApplyReward(option, Vector3.zero);

            Assert.IsNotNull(instance);
            Assert.AreEqual("item_sword", instance.Data.ItemId);
            Assert.IsTrue(_rewardService.IsSelectionLocked);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void RewardService_DoubleSelection_IsPreventedByLock()
        {
            RewardOption option1 = RewardOption.FromItemData(_swordData);
            RewardOption option2 = RewardOption.FromItemData(_plateData);

            // First selection succeeds
            ItemInstance first = _rewardService.ApplyReward(option1, Vector3.zero);
            Assert.IsNotNull(first);
            Assert.IsTrue(_rewardService.IsSelectionLocked);

            // Second selection in same round is rejected
            ItemInstance second = _rewardService.ApplyReward(option2, Vector3.zero);
            Assert.IsNull(second);

            Object.DestroyImmediate(first.gameObject);
        }

        [Test]
        public void RewardService_ItemDataSO_RemainsImmutableAfterRewardApplication()
        {
            RewardOption option = RewardOption.FromItemData(_swordData);
            ItemInstance instance = _rewardService.ApplyReward(option, Vector3.zero);

            Assert.AreEqual("item_sword", _swordData.ItemId);
            Assert.AreEqual(ItemCategory.Weapon, _swordData.Category);
            Assert.AreEqual(new Vector2Int(1, 2), _swordData.BaseDimensions);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void RewardService_ResetSelectionLock_EnablesNewSelectionRound()
        {
            RewardOption option1 = RewardOption.FromItemData(_swordData);
            ItemInstance first = _rewardService.ApplyReward(option1, Vector3.zero);
            Assert.IsTrue(_rewardService.IsSelectionLocked);

            _rewardService.ResetSelectionLock();
            Assert.IsFalse(_rewardService.IsSelectionLocked);

            RewardOption option2 = RewardOption.FromItemData(_plateData);
            ItemInstance second = _rewardService.ApplyReward(option2, Vector3.zero);
            Assert.IsNotNull(second);
            Assert.IsTrue(_rewardService.IsSelectionLocked);

            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }
    }
}
