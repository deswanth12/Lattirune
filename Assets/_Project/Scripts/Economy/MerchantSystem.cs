using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Economy
{
    /// <summary>
    /// Coordinates in-run Merchant Stall generation, pricing curves, purchases,
    /// and inventory refresh according to PLAN.md Section 11 and Section 13.1.
    /// </summary>
    public class MerchantSystem : MonoBehaviour
    {
        [Header("Databases")]
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private RuneDatabaseSO runeDatabase;

        [Header("Offers")]
        [SerializeField] private List<MerchantOffer> currentOffers = new List<MerchantOffer>();

        private IRandomSource _random;

        public IReadOnlyList<MerchantOffer> CurrentOffers => currentOffers;
        public int OfferCount => currentOffers != null ? currentOffers.Count : 0;

        public event Action<MerchantOffer> OnOfferPurchased;
        public event Action OnOffersRefreshed;

        public void Initialize(
            ItemDatabaseSO itemDb = null, 
            RuneDatabaseSO runeDb = null, 
            IRandomSource random = null)
        {
            itemDatabase = itemDb != null ? itemDb : ItemDatabaseSO.CreateCanonicalDatabase();
            runeDatabase = runeDb != null ? runeDb : RuneDatabaseSO.CreateCanonicalDatabase();
            _random = random ?? new SystemRandomSource();
        }

        /// <summary>
        /// Generates a balanced set of merchant stall offers based on PLAN.md Section 13.1 balance sheet:
        /// - 2 Equipment Items (15-50 Gold)
        /// - 1 Directional Rune (30-45 Gold)
        /// - 1 Lattice Grid Slot Expansion (40 Gold)
        /// - 1 Emergency Health Potion (15 Gold)
        /// </summary>
        public void GenerateOffers(int floorNumber = 1)
        {
            currentOffers.Clear();
            if (_random == null) _random = new SystemRandomSource();

            // 1. Generate 2 Equipment Items
            if (itemDatabase != null && itemDatabase.TotalItemCount > 0)
            {
                var availableItems = new List<ItemDataSO>(itemDatabase.AllItems);
                // Filter out non-equipment or internal items if needed
                for (int i = 0; i < 2 && availableItems.Count > 0; i++)
                {
                    int pickIdx = _random.Next(0, availableItems.Count);
                    ItemDataSO item = availableItems[pickIdx];
                    availableItems.RemoveAt(pickIdx);

                    int price = item.Category == ItemCategory.Relic ? _random.Next(35, 51) : _random.Next(15, 31);
                    currentOffers.Add(new MerchantOffer(
                        id: $"offer_item_{item.ItemId}_{i}",
                        title: item.DisplayName,
                        desc: $"Equipment [{item.Category}] | Size: {item.Dimensions.x}x{item.Dimensions.y}",
                        type: MerchantOfferType.Item,
                        price: price,
                        item: item
                    ));
                }
            }

            // 2. Generate 1 Directional Rune
            if (runeDatabase != null && runeDatabase.TotalRuneCount > 0)
            {
                int runeIdx = _random.Next(0, runeDatabase.TotalRuneCount);
                RuneData rune = runeDatabase.AllRunes[runeIdx];
                int runePrice = _random.Next(30, 46); // 30-45g
                currentOffers.Add(new MerchantOffer(
                    id: $"offer_rune_{rune.RuneId}",
                    title: rune.RuneName,
                    desc: $"Elemental Rune [{rune.Element}] | Direction: {rune.Direction}",
                    type: MerchantOfferType.Rune,
                    price: runePrice,
                    rune: rune
                ));
            }

            // 3. Grid Slot Expansion (40 Gold fixed)
            currentOffers.Add(new MerchantOffer(
                id: "offer_grid_expansion",
                title: "Lattice Slot Expansion",
                desc: "Permanently unlocks 1 adjacent locked grid tile for this run.",
                type: MerchantOfferType.GridSlotExpansion,
                price: 40
            ));

            // 4. Emergency Health Potion (15 Gold)
            currentOffers.Add(new MerchantOffer(
                id: "offer_health_potion",
                title: "Rejuvenation Draught",
                desc: "Restores 35 Health points immediately.",
                type: MerchantOfferType.HealthPotion,
                price: 15
            ));

            OnOffersRefreshed?.Invoke();
        }

        /// <summary>
        /// Attempts to purchase an offer by index.
        /// </summary>
        public bool BuyOffer(
            int index,
            IEconomyService economy,
            InventorySystem inventory = null,
            LatticeGrid grid = null,
            PlayerCombatant player = null)
        {
            if (index < 0 || index >= currentOffers.Count) return false;
            MerchantOffer offer = currentOffers[index];
            if (offer == null || offer.IsSold) return false;

            if (economy == null || !economy.CanAfford(offer.CurrentPrice))
            {
                return false;
            }

            // Deduct Gold
            if (!economy.SpendGold(offer.CurrentPrice))
            {
                return false;
            }

            // Deliver Offer Payload
            switch (offer.OfferType)
            {
                case MerchantOfferType.GridSlotExpansion:
                    if (grid != null)
                    {
                        grid.UnlockFirstAvailableLockedSlot();
                    }
                    break;

                case MerchantOfferType.HealthPotion:
                    if (player != null)
                    {
                        player.Heal(35);
                    }
                    break;

                case MerchantOfferType.Item:
                    if (offer.ItemData != null && inventory != null)
                    {
                        ItemInstance instance = ItemFactory.CreateInstance(offer.ItemData);
                        inventory.AddItemToStaging(instance);
                    }
                    break;

                case MerchantOfferType.Rune:
                    // Rune is safely treated as an active catalog purchase
                    break;
            }

            offer.MarkAsSold();
            OnOfferPurchased?.Invoke(offer);
            return true;
        }

        /// <summary>
        /// Rerolls all unsold offers for a nominal gold fee.
        /// </summary>
        public bool RerollOffers(IEconomyService economy, int cost = 10, int floorNumber = 1)
        {
            if (economy == null || !economy.CanAfford(cost)) return false;
            if (!economy.SpendGold(cost)) return false;

            GenerateOffers(floorNumber);
            return true;
        }
    }
}
