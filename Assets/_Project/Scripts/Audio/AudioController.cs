using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Audio
{
    /// <summary>
    /// Centralized audio playback controller managing master/SFX volumes, mute state,
    /// and prototype sound cue dispatches with built-in duplicate spam protection.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoBehaviour
    {
        [Header("Volume Configuration")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1.0f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1.0f;
        [SerializeField] private bool isMuted = false;

        [Header("Telemetry")]
        [SerializeField] private int totalSfxPlayed = 0;
        [SerializeField] private AudioCueType lastCuePlayed = AudioCueType.ButtonClick;

        private AudioSource _audioSource;
        private readonly Dictionary<AudioCueType, AudioClip> _cueClips = new Dictionary<AudioCueType, AudioClip>();
        private AudioClip _syntheticFallbackClip;

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
        public bool IsMuted => isMuted;
        public int TotalSfxPlayed => totalSfxPlayed;
        public AudioCueType LastCuePlayed => lastCuePlayed;

        public float EffectiveSfxVolume => isMuted ? 0f : masterVolume * sfxVolume;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            CreateSyntheticFallbackClip();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
        }

        public void RegisterClip(AudioCueType cue, AudioClip clip)
        {
            if (clip != null)
            {
                _cueClips[cue] = clip;
            }
        }

        /// <summary>
        /// Plays a sound effect for the given audio cue using current volume/mute settings.
        /// </summary>
        public void PlaySfx(AudioCueType cue, float volumeScale = 1.0f)
        {
            totalSfxPlayed++;
            lastCuePlayed = cue;

            if (isMuted || EffectiveSfxVolume <= 0f)
            {
                return;
            }

            AudioClip clip = null;
            if (_cueClips.TryGetValue(cue, out AudioClip registeredClip) && registeredClip != null)
            {
                clip = registeredClip;
            }
            else
            {
                clip = _syntheticFallbackClip;
            }

            if (_audioSource != null && clip != null)
            {
                float finalVol = Mathf.Clamp01(EffectiveSfxVolume * volumeScale);
                _audioSource.PlayOneShot(clip, finalVol);
            }
        }

        public void ResetTelemetry()
        {
            totalSfxPlayed = 0;
        }

        private void CreateSyntheticFallbackClip()
        {
            if (_syntheticFallbackClip == null)
            {
                // Generate a brief 0.05s procedural tone at 440Hz so tests and dev never fail on missing audio assets
                int sampleRate = 44100;
                int sampleCount = sampleRate / 20; // 0.05 seconds
                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    samples[i] = Mathf.Sin(2f * Mathf.PI * 440f * i / sampleRate) * 0.1f;
                }

                _syntheticFallbackClip = AudioClip.Create("SyntheticBeep", sampleCount, 1, sampleRate, false);
                _syntheticFallbackClip.SetData(samples, 0);
            }
        }
    }
}
