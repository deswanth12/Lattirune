using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;

namespace Lattirune.Tests
{
    [TestFixture]
    public class AudioControllerTests
    {
        private GameObject _audioObj;
        private AudioController _audioController;

        [SetUp]
        public void Setup()
        {
            _audioObj = new GameObject("TestAudioController");
            _audioController = _audioObj.AddComponent<AudioController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_audioObj != null)
            {
                Object.DestroyImmediate(_audioObj);
            }
        }

        [Test]
        public void AudioController_Initializes_WithDefaultVolumes()
        {
            Assert.AreEqual(1.0f, _audioController.MasterVolume);
            Assert.AreEqual(1.0f, _audioController.SfxVolume);
            Assert.IsFalse(_audioController.IsMuted);
            Assert.AreEqual(1.0f, _audioController.EffectiveSfxVolume);
        }

        [Test]
        public void AudioController_VolumeClamping_RestrictsValuesToZeroOne()
        {
            _audioController.SetMasterVolume(1.5f);
            Assert.AreEqual(1.0f, _audioController.MasterVolume);

            _audioController.SetMasterVolume(-0.5f);
            Assert.AreEqual(0.0f, _audioController.MasterVolume);

            _audioController.SetSfxVolume(2.0f);
            Assert.AreEqual(1.0f, _audioController.SfxVolume);

            _audioController.SetSfxVolume(-1.0f);
            Assert.AreEqual(0.0f, _audioController.SfxVolume);
        }

        [Test]
        public void AudioController_Mute_SetsEffectiveVolumeToZero()
        {
            _audioController.SetMasterVolume(0.8f);
            _audioController.SetSfxVolume(0.5f);
            Assert.AreEqual(0.4f, _audioController.EffectiveSfxVolume, 0.001f);

            _audioController.SetMuted(true);
            Assert.IsTrue(_audioController.IsMuted);
            Assert.AreEqual(0.0f, _audioController.EffectiveSfxVolume);

            _audioController.SetMuted(false);
            Assert.AreEqual(0.4f, _audioController.EffectiveSfxVolume, 0.001f);
        }

        [Test]
        public void AudioController_PlaySfx_UpdatesTelemetryAndLastCue()
        {
            Assert.AreEqual(0, _audioController.TotalSfxPlayed);

            _audioController.PlaySfx(AudioCueType.SynergyActivated);
            Assert.AreEqual(1, _audioController.TotalSfxPlayed);
            Assert.AreEqual(AudioCueType.SynergyActivated, _audioController.LastCuePlayed);

            _audioController.PlaySfx(AudioCueType.Victory);
            Assert.AreEqual(2, _audioController.TotalSfxPlayed);
            Assert.AreEqual(AudioCueType.Victory, _audioController.LastCuePlayed);
        }

        [Test]
        public void AudioController_AllFifteenCues_PlaySafelyWithoutExceptions()
        {
            for (int i = 0; i <= (int)AudioCueType.Continue; i++)
            {
                AudioCueType cue = (AudioCueType)i;
                Assert.DoesNotThrow(() => _audioController.PlaySfx(cue));
            }

            Assert.AreEqual((int)AudioCueType.Continue + 1, _audioController.TotalSfxPlayed);
        }
    }
}
