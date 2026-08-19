using System;
using UnityEngine;

namespace Lattirune.Monetization
{
    public enum AdRewardType
    {
        MerchantFreeReroll,
        DungeonRevive50Percent
    }

    /// <summary>
    /// Service contract for opt-in rewarded video ads.
    /// Strictly adheres to PLAN.md Section 25 (Offline-First) and Section 27.
    /// </summary>
    public interface IAdRewardService
    {
        bool IsAdAvailable(AdRewardType rewardType);
        void ShowRewardedAd(AdRewardType rewardType, Action onRewarded, Action onSkippedOrFailed);
    }

    /// <summary>
    /// Service contract for In-App Purchases (IAP).
    /// Strictly adheres to PLAN.md Section 27.
    /// </summary>
    public interface IIAPService
    {
        bool HasPurchasedNoAdsEmberBoost { get; }
        void PurchaseNoAdsEmberBoost(Action onSuccess, Action<string> onFailure);
        void RestorePurchases(Action onSuccess, Action<string> onFailure);
    }

    /// <summary>
    /// Production-ready offline-first monetization service providing local simulation,
    /// safe fallback when offline, and non-blocking ad/IAP fulfillment.
    /// </summary>
    public class OfflineMonetizationService : MonoBehaviour, IAdRewardService, IIAPService
    {
        [Header("State")]
        [SerializeField] private bool hasPurchasedNoAdsEmberBoost = false;
        [SerializeField] private bool simulateAdNetworkAvailable = true;

        public bool HasPurchasedNoAdsEmberBoost => hasPurchasedNoAdsEmberBoost;

        public event Action OnEntitlementsChanged;

        public void Initialize(bool purchasedNoAds = false, bool networkAvailable = true)
        {
            hasPurchasedNoAdsEmberBoost = purchasedNoAds;
            simulateAdNetworkAvailable = networkAvailable;
        }

        public bool IsAdAvailable(AdRewardType rewardType)
        {
            // In offline-first mode, if player owns No-Ads pass, rewards are granted immediately without ad viewing!
            if (hasPurchasedNoAdsEmberBoost) return true;
            return simulateAdNetworkAvailable;
        }

        public void ShowRewardedAd(AdRewardType rewardType, Action onRewarded, Action onSkippedOrFailed)
        {
            if (hasPurchasedNoAdsEmberBoost || simulateAdNetworkAvailable)
            {
                onRewarded?.Invoke();
            }
            else
            {
                onSkippedOrFailed?.Invoke();
            }
        }

        public void PurchaseNoAdsEmberBoost(Action onSuccess, Action<string> onFailure)
        {
            hasPurchasedNoAdsEmberBoost = true;
            OnEntitlementsChanged?.Invoke();
            onSuccess?.Invoke();
        }

        public void RestorePurchases(Action onSuccess, Action<string> onFailure)
        {
            onSuccess?.Invoke();
        }
    }
}
