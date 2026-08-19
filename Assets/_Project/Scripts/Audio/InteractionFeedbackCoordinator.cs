using UnityEngine;
using Lattirune.Combat;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Audio
{
    /// <summary>
    /// Observes existing gameplay system events and dispatches coordinated audio and haptic feedback.
    /// Strictly adheres to the architectural rule: Observes gameplay; does not control or mutate gameplay.
    /// </summary>
    public class InteractionFeedbackCoordinator : MonoBehaviour
    {
        [Header("Feedback Dispatchers")]
        [SerializeField] private AudioController audioController;
        [SerializeField] private HapticFeedback hapticFeedback;

        private LatticeGrid _grid;
        private SynergySystem _synergySystem;
        private CombatSystem _combatSystem;
        private RewardService _rewardService;
        private Reactions.ElementalReactionSystem _reactionSystem;
        private Economy.MerchantSystem _merchantSystem;

        public AudioController Audio => audioController;
        public HapticFeedback Haptics => hapticFeedback;

        public void Initialize(
            AudioController audio,
            HapticFeedback haptics,
            LatticeGrid grid,
            SynergySystem synergy,
            CombatSystem combat,
            RewardService rewards,
            Reactions.ElementalReactionSystem reactions = null,
            Economy.MerchantSystem merchant = null)
        {
            UnsubscribeAll();

            audioController = audio;
            hapticFeedback = haptics;
            _grid = grid;
            _synergySystem = synergy;
            _combatSystem = combat;
            _rewardService = rewards;
            _reactionSystem = reactions;
            _merchantSystem = merchant;

            SubscribeAll();
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
        }

        private void SubscribeAll()
        {
            if (_grid != null)
            {
                _grid.OnItemPlaced += HandleItemPlaced;
                _grid.OnItemRemoved += HandleItemRemoved;
            }

            if (_synergySystem != null)
            {
                _synergySystem.OnSynergyActivated += HandleSynergyActivated;
                _synergySystem.OnSynergyDeactivated += HandleSynergyDeactivated;
            }

            if (_reactionSystem != null)
            {
                _reactionSystem.OnReactionActivated += HandleReactionActivated;
            }

            if (_merchantSystem != null)
            {
                _merchantSystem.OnOfferPurchased += HandleOfferPurchased;
            }

            if (_combatSystem != null)
            {
                _combatSystem.OnAttackExecuted += HandleAttackExecuted;
                _combatSystem.OnEmergencyPotionUsed += HandleEmergencyPotionUsed;
                _combatSystem.OnVictory += HandleVictory;
                _combatSystem.OnDefeat += HandleDefeat;
            }

            if (_rewardService != null)
            {
                _rewardService.OnRewardApplied += HandleRewardApplied;
            }
        }

        public void UnsubscribeAll()
        {
            if (_grid != null)
            {
                _grid.OnItemPlaced -= HandleItemPlaced;
                _grid.OnItemRemoved -= HandleItemRemoved;
            }

            if (_synergySystem != null)
            {
                _synergySystem.OnSynergyActivated -= HandleSynergyActivated;
                _synergySystem.OnSynergyDeactivated -= HandleSynergyDeactivated;
            }

            if (_reactionSystem != null)
            {
                _reactionSystem.OnReactionActivated -= HandleReactionActivated;
            }

            if (_merchantSystem != null)
            {
                _merchantSystem.OnOfferPurchased -= HandleOfferPurchased;
            }

            if (_combatSystem != null)
            {
                _combatSystem.OnAttackExecuted -= HandleAttackExecuted;
                _combatSystem.OnEmergencyPotionUsed -= HandleEmergencyPotionUsed;
                _combatSystem.OnVictory -= HandleVictory;
                _combatSystem.OnDefeat -= HandleDefeat;
            }

            if (_rewardService != null)
            {
                _rewardService.OnRewardApplied -= HandleRewardApplied;
            }
        }

        private void HandleItemPlaced(string itemId, Vector2Int origin, Vector2Int size)
        {
            audioController?.PlaySfx(AudioCueType.ItemValidPlacement);
            hapticFeedback?.TriggerHaptic(HapticType.Light);
        }

        private void HandleItemRemoved(string itemId, Vector2Int origin, Vector2Int size)
        {
            audioController?.PlaySfx(AudioCueType.ItemDragStart);
            hapticFeedback?.TriggerHaptic(HapticType.Light);
        }

        private void HandleSynergyActivated(SynergyResult result)
        {
            audioController?.PlaySfx(AudioCueType.SynergyActivated);
            hapticFeedback?.TriggerHaptic(HapticType.Medium);
        }

        private void HandleSynergyDeactivated(SynergyResult result)
        {
            audioController?.PlaySfx(AudioCueType.SynergyDeactivated);
            hapticFeedback?.TriggerHaptic(HapticType.Light);
        }

        private void HandleReactionActivated(Reactions.ElementalReactionResult result)
        {
            audioController?.PlaySfx(AudioCueType.RuneConduitIgnite);
            hapticFeedback?.TriggerHaptic(HapticType.Heavy);
        }

        private void HandleOfferPurchased(Economy.MerchantOffer offer)
        {
            audioController?.PlaySfx(AudioCueType.RewardApplied);
            hapticFeedback?.TriggerHaptic(HapticType.Success);
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            audioController?.PlaySfx(AudioCueType.Attack);
            hapticFeedback?.TriggerHaptic(HapticType.Light);
        }

        private void HandleEmergencyPotionUsed(int healAmount)
        {
            audioController?.PlaySfx(AudioCueType.RewardApplied);
            hapticFeedback?.TriggerHaptic(HapticType.Medium);
        }

        private void HandleVictory()
        {
            audioController?.PlaySfx(AudioCueType.Victory);
            hapticFeedback?.TriggerHaptic(HapticType.Success);
        }

        private void HandleDefeat()
        {
            audioController?.PlaySfx(AudioCueType.Defeat);
            hapticFeedback?.TriggerHaptic(HapticType.Failure);
        }

        private void HandleRewardApplied(RewardOption option, ItemInstance instance)
        {
            audioController?.PlaySfx(AudioCueType.RewardApplied);
            hapticFeedback?.TriggerHaptic(HapticType.Success);
        }
    }
}
