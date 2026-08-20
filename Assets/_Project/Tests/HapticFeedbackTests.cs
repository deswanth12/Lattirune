using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Combat;
using Lattirune.Grid;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class HapticFeedbackTests
    {
        private GameObject _hapticObj;
        private HapticFeedback _haptic;
        private AudioController _audio;
        private InteractionFeedbackCoordinator _coordinator;

        [SetUp]
        public void Setup()
        {
            _hapticObj = new GameObject("TestHapticsHolder");
            _haptic = _hapticObj.AddComponent<HapticFeedback>();
            _audio = _hapticObj.AddComponent<AudioController>();
            _coordinator = _hapticObj.AddComponent<InteractionFeedbackCoordinator>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_hapticObj != null)
            {
                Object.DestroyImmediate(_hapticObj);
            }
        }

        [Test]
        public void HapticFeedback_TriggersAndIncrementsCount()
        {
            Assert.AreEqual(0, _haptic.TriggerCount);

            _haptic.TriggerHaptic(HapticType.Light);
            Assert.AreEqual(1, _haptic.TriggerCount);
            Assert.AreEqual(HapticType.Light, _haptic.LastTriggered);

            _haptic.TriggerHaptic(HapticType.Success);
            Assert.AreEqual(2, _haptic.TriggerCount);
            Assert.AreEqual(HapticType.Success, _haptic.LastTriggered);
        }

        [Test]
        public void HapticFeedback_Disabled_PreventsTriggers()
        {
            _haptic.HapticsEnabled = false;

            _haptic.TriggerHaptic(HapticType.Heavy);
            Assert.AreEqual(0, _haptic.TriggerCount);

            _haptic.HapticsEnabled = true;
            _haptic.TriggerHaptic(HapticType.Heavy);
            Assert.AreEqual(1, _haptic.TriggerCount);
        }

        [Test]
        public void HapticFeedback_AllSixTypes_ExecuteSafelyOnNonMobile()
        {
            HapticType[] types = new HapticType[]
            {
                HapticType.Light,
                HapticType.Medium,
                HapticType.Heavy,
                HapticType.Success,
                HapticType.Warning,
                HapticType.Failure
            };

            foreach (var t in types)
            {
                Assert.DoesNotThrow(() => _haptic.TriggerHaptic(t));
            }

            Assert.AreEqual(6, _haptic.TriggerCount);
        }

        [Test]
        public void InteractionFeedbackCoordinator_Unsubscribe_PreventsDuplicateListeners()
        {
            LatticeGrid grid = new LatticeGrid(initializeDefaultLayout: true);
            GameObject synergyObj = new GameObject("SynergySys");
            SynergySystem synergy = synergyObj.AddComponent<SynergySystem>();

            _coordinator.Initialize(_audio, _haptic, grid, synergy, null, null);

            // Trigger item placed event once
            grid.PlaceItem("item_01", new Vector2Int(2, 2), new Vector2Int(1, 1));
            int countAfterFirst = _haptic.TriggerCount;
            Assert.IsTrue(countAfterFirst >= 1, "First placement must trigger at least 1 haptic.");

            // Re-initialize (unsubscribes previous listeners to prevent duplicate callbacks)
            _coordinator.Initialize(_audio, _haptic, grid, synergy, null, null);
            int countBeforeSecond = _haptic.TriggerCount;
            grid.PlaceItem("item_02", new Vector2Int(0, 0), new Vector2Int(1, 1));
            int countAfterSecond = _haptic.TriggerCount;

            // The 2nd placement must fire AT MOST as many haptics as the 1st
            // (i.e., no duplicate subscription doubling the callback count)
            int firstPlacementFired = countAfterFirst;
            int secondPlacementFired = countAfterSecond - countBeforeSecond;
            Assert.LessOrEqual(secondPlacementFired, firstPlacementFired,
                "Re-init should not cause more haptics than a single-subscribe placement.");

            Object.DestroyImmediate(synergyObj);
        }

        [Test]
        public void InteractionFeedbackCoordinator_ReactionAndMerchantEvents_TriggerFeedback()
        {
            var rxnObj = new GameObject("RxnSys");
            var rxnSys = rxnObj.AddComponent<Lattirune.Reactions.ElementalReactionSystem>();
            rxnSys.EnsureDefaultDefinitions();

            var merchObj = new GameObject("MerchSys");
            var merchSys = merchObj.AddComponent<Lattirune.Economy.MerchantSystem>();
            merchSys.Initialize();

            _coordinator.Initialize(_audio, _haptic, null, null, null, null, rxnSys, merchSys);

            // Test merchant purchase
            merchSys.GenerateOffers(1);
            var economy = merchObj.AddComponent<Lattirune.Economy.SimpleEconomyService>();
            economy.Initialize(999);
            merchSys.BuyOffer(0, economy);

            Assert.AreEqual(1, _haptic.TriggerCount);
            Assert.AreEqual(HapticType.Success, _haptic.LastTriggered);
            Assert.AreEqual(AudioCueType.RewardApplied, _audio.LastCuePlayed);

            Object.DestroyImmediate(rxnObj);
            Object.DestroyImmediate(merchObj);
        }
    }
}
