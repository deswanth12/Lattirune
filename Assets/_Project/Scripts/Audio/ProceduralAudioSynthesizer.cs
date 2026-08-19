using System;
using UnityEngine;

namespace Lattirune.Audio
{
    /// <summary>
    /// Procedural real-time waveform audio synthesizer generating 16-bit sound effects in-memory.
    /// Strictly adheres to PLAN.md Section 18 audio design matrix.
    /// </summary>
    public static class ProceduralAudioSynthesizer
    {
        public const int SAMPLE_RATE = 44100;

        /// <summary>
        /// Generates a synthesized audio clip corresponding to the specified AudioCueType.
        /// </summary>
        public static AudioClip CreateClipForCue(AudioCueType cue)
        {
            switch (cue)
            {
                case AudioCueType.ItemDragStart:
                    return GenerateClick(0.06f, 800f, 400f);
                case AudioCueType.ItemValidPlacement:
                    return GenerateThud(0.12f, 150f);
                case AudioCueType.ItemInvalidPlacement:
                    return GenerateBuzz(0.15f, 110f);
                case AudioCueType.RuneConduit:
                case AudioCueType.RuneConduitIgnite:
                    return GenerateLaserHum(0.25f, 220f, 660f);
                case AudioCueType.CombatBladeSlash:
                case AudioCueType.Attack:
                    return GenerateSlash(0.14f);
                case AudioCueType.CombatBurnTick:
                    return GenerateCrackle(0.10f);
                case AudioCueType.CombatFreezeShatter:
                    return GenerateCrystalChime(0.25f, 1200f);
                case AudioCueType.CombatBossRoar:
                    return GenerateGrowl(0.50f, 75f);
                case AudioCueType.BgmDungeonLoop:
                    return GenerateAmbientDrone(1.0f, 110f);
                case AudioCueType.Victory:
                    return GenerateFanfare(0.40f);
                case AudioCueType.Defeat:
                    return GenerateSadMinor(0.40f);
                default:
                    return GenerateClick(0.05f, 600f, 300f);
            }
        }

        private static AudioClip GenerateClick(float duration, float startFreq, float endFreq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = Mathf.Exp(-t * 8f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("sfx_click", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateThud(float duration, float freq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Exp(-t * 6f);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) + (UnityEngine.Random.value * 0.2f - 0.1f)) * envelope * 0.6f;
            }

            AudioClip clip = AudioClip.Create("sfx_thud", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateBuzz(float duration, float freq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = 1f - t;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) > 0 ? 0.3f : -0.3f;
                samples[i] = wave * envelope;
            }

            AudioClip clip = AudioClip.Create("sfx_buzz", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateLaserHum(float duration, float startFreq, float endFreq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, Mathf.Sqrt(t));
                float envelope = Mathf.Sin(t * Mathf.PI);
                samples[i] = Mathf.Sin(2f * Mathf.PI * currentFreq * (i / (float)SAMPLE_RATE)) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("sfx_laser_hum", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateSlash(float duration)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * 3f);
                float noise = (UnityEngine.Random.value * 2f - 1f);
                samples[i] = noise * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("sfx_slash", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCrackle(float duration)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float noise = (UnityEngine.Random.value > 0.85f) ? (UnityEngine.Random.value * 2f - 1f) : 0f;
                samples[i] = noise * (1f - t) * 0.4f;
            }

            AudioClip clip = AudioClip.Create("sfx_crackle", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCrystalChime(float duration, float fundamental)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Exp(-t * 5f);
                float wave1 = Mathf.Sin(2f * Mathf.PI * fundamental * (i / (float)SAMPLE_RATE));
                float wave2 = Mathf.Sin(2f * Mathf.PI * (fundamental * 1.5f) * (i / (float)SAMPLE_RATE));
                samples[i] = (wave1 + wave2 * 0.5f) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create("sfx_crystal", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateGrowl(float duration, float baseFreq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Sin(t * Mathf.PI);
                float sub = Mathf.Sin(2f * Mathf.PI * baseFreq * (i / (float)SAMPLE_RATE));
                float grit = (UnityEngine.Random.value * 0.4f - 0.2f);
                samples[i] = (sub + grit) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("sfx_growl", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateAmbientDrone(float duration, float freq)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Sin(t * Mathf.PI);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) +
                             Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * (i / (float)SAMPLE_RATE)) * 0.5f;
                samples[i] = wave * envelope * 0.25f;
            }

            AudioClip clip = AudioClip.Create("sfx_ambient_drone", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateFanfare(float duration)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = t < 0.5f ? 440f : 554.37f; // A4 -> C#5 major third
                float envelope = Mathf.Exp(-(t % 0.5f) * 4f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("sfx_fanfare", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateSadMinor(float duration)
        {
            int sampleCount = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = t < 0.5f ? 330f : 293.66f; // E4 -> D4 falling cadence
                float envelope = Mathf.Exp(-(t % 0.5f) * 4f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SAMPLE_RATE)) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("sfx_minor", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
