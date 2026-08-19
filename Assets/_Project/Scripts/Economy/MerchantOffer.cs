using System;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Economy
{
    public enum MerchantOfferType
    {
        Item,
        Rune,
        GridSlotExpansion,
        HealthPotion
    }

    /// <summary>
    /// Represents an individual item, rune, or service offered in the in-run Merchant Stall.
    /// Strictly adheres to PLAN.md Section 11 and Section 13.1.
    /// </summary>
    [Serializable]
    public class MerchantOffer
    {
        [SerializeField] private string offerId;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private MerchantOfferType offerType;
        [SerializeField] private int basePrice;
        [SerializeField] private int currentPrice;
        [SerializeField] private bool isSold;
        [SerializeField] private ItemDataSO itemData;
        [SerializeField] private RuneData runeData;

        public string OfferId => offerId;
        public string Title => title;
        public string Description => description;
        public MerchantOfferType OfferType => offerType;
        public int BasePrice => basePrice;
        public int CurrentPrice => currentPrice;
        public bool IsSold => isSold;
        public ItemDataSO ItemData => itemData;
        public RuneData RuneData => runeData;

        public MerchantOffer(
            string id,
            string title,
            string desc,
            MerchantOfferType type,
            int price,
            ItemDataSO item = null,
            RuneData rune = null)
        {
            this.offerId = id;
            this.title = title;
            this.description = desc;
            this.offerType = type;
            this.basePrice = Mathf.Max(1, price);
            this.currentPrice = this.basePrice;
            this.isSold = false;
            this.itemData = item;
            this.runeData = rune;
        }

        public void ApplyDiscount(float discountFraction)
        {
            discountFraction = Mathf.Clamp(discountFraction, 0f, 0.9f);
            currentPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f - discountFraction)));
        }

        public bool MarkAsSold()
        {
            if (isSold) return false;
            isSold = true;
            return true;
        }
    }
}
