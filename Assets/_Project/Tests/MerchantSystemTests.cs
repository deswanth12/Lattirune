using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Economy;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    [TestFixture]
    public class MerchantSystemTests
    {
        private GameObject _holder;
        private MerchantSystem _merchant;
        private SimpleEconomyService _economy;
        private LatticeGrid _grid;
        private InventorySystem _inventory;
        private PlayerCombatant _player;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("MerchantTestHolder");

            _merchant = _holder.AddComponent<MerchantSystem>();
            _merchant.Initialize(
                ItemDatabaseSO.CreateCanonicalDatabase(),
                RuneDatabaseSO.CreateCanonicalDatabase(),
                new SystemRandomSource(42)
            );

            _economy = _holder.AddComponent<SimpleEconomyService>();
            _economy.Initialize(startingGold: 100);

            _grid = new LatticeGrid();

            _inventory = _holder.AddComponent<InventorySystem>();
            _inventory.Initialize();

            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            _player = playerObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(100);
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
        public void GenerateOffers_ProducesBalancedCanonicalOfferSet()
        {
            _merchant.GenerateOffers(floorNumber: 4);

            Assert.AreEqual(4, _merchant.OfferCount);
            Assert.IsNotNull(_merchant.CurrentOffers[0]);
            Assert.IsNotNull(_merchant.CurrentOffers[1]);
            Assert.IsNotNull(_merchant.CurrentOffers[2]);
            Assert.IsNotNull(_merchant.CurrentOffers[3]);

            // Slot expansion is always priced at 40g (PLAN.md 13.1)
            var expansionOffer = System.Array.Find(_merchant.CurrentOffers as MerchantOffer[] ?? new List<MerchantOffer>(_merchant.CurrentOffers).ToArray(), o => o.OfferType == MerchantOfferType.GridSlotExpansion);
            Assert.IsNotNull(expansionOffer);
            Assert.AreEqual(40, expansionOffer.CurrentPrice);
        }

        [Test]
        public void BuyOffer_SlotExpansion_DeductsGoldAndUnlocksGridCell()
        {
            _merchant.GenerateOffers(floorNumber: 4);
            int initialLocked = _grid.GetLockedCount();
            Assert.Greater(initialLocked, 0);

            int expansionIdx = -1;
            for (int i = 0; i < _merchant.CurrentOffers.Count; i++)
            {
                if (_merchant.CurrentOffers[i].OfferType == MerchantOfferType.GridSlotExpansion)
                {
                    expansionIdx = i;
                    break;
                }
            }
            Assert.GreaterOrEqual(expansionIdx, 0);

            bool success = _merchant.BuyOffer(expansionIdx, _economy, _inventory, _grid, _player);
            Assert.IsTrue(success);
            Assert.AreEqual(60, _economy.CurrentGold); // 100 - 40 = 60
            Assert.AreEqual(initialLocked - 1, _grid.GetLockedCount());
            Assert.IsTrue(_merchant.CurrentOffers[expansionIdx].IsSold);

            // Second buy fails because already sold
            bool secondBuy = _merchant.BuyOffer(expansionIdx, _economy, _inventory, _grid, _player);
            Assert.IsFalse(secondBuy);
        }

        [Test]
        public void BuyOffer_HealthPotion_HealsWoundedPlayer()
        {
            _merchant.GenerateOffers(floorNumber: 4);
            _player.TakeDirectDamage(50);
            Assert.AreEqual(50, _player.CurrentHp);

            int potionIdx = -1;
            for (int i = 0; i < _merchant.CurrentOffers.Count; i++)
            {
                if (_merchant.CurrentOffers[i].OfferType == MerchantOfferType.HealthPotion)
                {
                    potionIdx = i;
                    break;
                }
            }
            Assert.GreaterOrEqual(potionIdx, 0);

            bool success = _merchant.BuyOffer(potionIdx, _economy, _inventory, _grid, _player);
            Assert.IsTrue(success);
            Assert.AreEqual(85, _economy.CurrentGold); // 100 - 15 = 85
            Assert.AreEqual(85, _player.CurrentHp); // 50 + 35 = 85
        }

        [Test]
        public void BuyOffer_FailsWhenInsufficientGold()
        {
            _economy.SpendGold(95); // 5 gold remaining
            Assert.AreEqual(5, _economy.CurrentGold);

            _merchant.GenerateOffers(floorNumber: 4);
            // Attempt to buy slot expansion (40g)
            int expansionIdx = -1;
            for (int i = 0; i < _merchant.CurrentOffers.Count; i++)
            {
                if (_merchant.CurrentOffers[i].OfferType == MerchantOfferType.GridSlotExpansion)
                {
                    expansionIdx = i;
                    break;
                }
            }

            bool success = _merchant.BuyOffer(expansionIdx, _economy, _inventory, _grid, _player);
            Assert.IsFalse(success);
            Assert.AreEqual(5, _economy.CurrentGold);
            Assert.IsFalse(_merchant.CurrentOffers[expansionIdx].IsSold);
        }

        [Test]
        public void RerollOffers_SpendsFeeAndRegeneratesNewOffers()
        {
            _merchant.GenerateOffers(floorNumber: 4);
            string firstOfferId = _merchant.CurrentOffers[0].OfferId;

            bool rerolled = _merchant.RerollOffers(_economy, cost: 10, floorNumber: 4);
            Assert.IsTrue(rerolled);
            Assert.AreEqual(90, _economy.CurrentGold); // 100 - 10 = 90
            Assert.AreEqual(4, _merchant.OfferCount);
        }
    }
}
