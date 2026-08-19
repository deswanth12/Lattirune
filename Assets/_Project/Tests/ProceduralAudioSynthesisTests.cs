using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;

namespace Lattirune.Tests
{
    [TestFixture]
    public class ProceduralAudioSynthesisTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""AudioTestHolder"");
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
        public void ProceduralSynthesizer_GeneratesValidAudioClipsForAllCanonicalCues()
        {
            AudioCueType[] cues = new[]
            {
                AudioCueType.ItemDragStart,
                AudioCueType.ItemValidPlacement,
                AudioCueType.ItemInvalidPlacement,
                AudioCueType.RuneConduit,
                AudioCueType.CombatBladeSlash,
                AudioCueType.CombatBurnTick,
                AudioCueType.CombatFreezeShatter,
                AudioCueType.CombatBossRoar,
                AudioCueType.BgmDungeonLoop,
                AudioCueType.Victory,
                AudioCueType.Defeat
            };

            foreach (var cue in cues)
            {
                AudioClip clip = ProceduralAudioSynthesizer.CreateClipForCue(cue);
                Assert.IsNotNull(clip, $""Failed to synthesize clip for cue {cue}"");
                Assert.AreEqual(1, clip.channels);
                Assert.AreEqual(44100, clip.frequency);
                Assert.Greater(clip.samples, 0);
                Assert.Greater(clip.length, 0f);

                // Sample sanity check: amplitude within [-1.0, 1.0]
                float[] data = new float[clip.samples];
                clip.GetData(data, 0);

                bool hasNonZeroSample = false;
                for (int i = 0; i < data.Length; i++)
                {
                    Assert.GreaterOrEqual(data[i], -1.0f);
                    Assert.LessOrEqual(data[i], 1.0f);
                    if (Mathf.Abs(data[i]) > 0.001f)
                    {
                        hasNonZeroSample = true;
                    }
                }
                Assert.IsTrue(hasNonZeroSample, $""Synthesized clip for {cue} was completely silent!"");
            }
        }

        [Test]
        public void AudioController_PlaysProceduralSfxSeamlessly()
        {
            var controller = _holder.AddComponent<AudioController>();
            controller.SetMasterVolume(1.0f);
            controller.SetSfxVolume(0.8f);

            Assert.AreEqual(0, controller.TotalSfxPlayed);

            controller.PlaySfx(AudioCueType.CombatBladeSlash);
            Assert.AreEqual(1, controller.TotalSfxPlayed);
            Assert.AreEqual(AudioCueType.CombatBladeSlash, controller.LastCuePlayed);

            controller.PlaySfx(AudioCueType.CombatBossRoar);
            Assert.AreEqual(2, controller.TotalSfxPlayed);
            Assert.AreEqual(AudioCueType.CombatBossRoar, controller.LastCuePlayed);
        }
    }
}
