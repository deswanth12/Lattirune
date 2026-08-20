using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;

namespace Lattirune.UI
{


    /// <summary>
    /// Centralized Juice, Game Feel, Screen Shake, and Haptic Feedback Controller.
    /// Provides AAA-style polish across combat, grid interactions, and UI navigation.
    /// </summary>
    public class JuiceController : MonoBehaviour
    {
        private static JuiceController s_Instance;
        public static JuiceController Instance => s_Instance;

        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;
        private Vector2 _shakeOffset = Vector2.zero;

        private float _screenFlashAlpha = 0f;
        private Color _screenFlashColor = Color.white;

        public struct ElementalReactionBanner
        {
            public string title;
            public string subtitle;
            public Color color;
            public float lifetime;
            public float maxLifetime;
            public float scale;
        }

        private readonly List<ElementalReactionBanner> _reactionBanners = new List<ElementalReactionBanner>();

        public Vector2 ShakeOffset => _shakeOffset;
        public float ShakeIntensity => _shakeIntensity;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // Screen Shake Decay
            if (_shakeDuration > 0f)
            {
                _shakeDuration -= dt;
                float currentMag = _shakeIntensity * (_shakeDuration > 0f ? 1f : 0f);
                _shakeOffset = new Vector2(
                    UnityEngine.Random.Range(-currentMag, currentMag),
                    UnityEngine.Random.Range(-currentMag, currentMag)
                );
            }
            else
            {
                _shakeOffset = Vector2.zero;
                _shakeIntensity = 0f;
            }

            // Screen Flash Decay
            if (_screenFlashAlpha > 0f)
            {
                _screenFlashAlpha = Mathf.Max(0f, _screenFlashAlpha - dt * 3.5f);
            }

            // Update Reaction Banners
            for (int i = _reactionBanners.Count - 1; i >= 0; i--)
            {
                var b = _reactionBanners[i];
                b.lifetime -= dt;
                float progress = 1f - (b.lifetime / b.maxLifetime);
                b.scale = Mathf.Lerp(1.3f, 1.0f, Mathf.Min(1f, progress * 4f));

                if (b.lifetime <= 0f)
                {
                    _reactionBanners.RemoveAt(i);
                }
                else
                {
                    _reactionBanners[i] = b;
                }
            }
        }

        public void TriggerScreenShake(float intensity = 12f, float duration = 0.25f)
        {
            _shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
            _shakeDuration = Mathf.Max(_shakeDuration, duration);
        }

        public void TriggerHaptic(HapticType type = HapticType.Medium)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Handheld.Vibrate();
            }
            catch (Exception) { }
#endif
        }

        public void TriggerScreenFlash(Color color, float initialAlpha = 0.5f)
        {
            _screenFlashColor = color;
            _screenFlashAlpha = initialAlpha;
        }

        public void TriggerHitStop(float duration = 0.04f)
        {
            StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float oldScale = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = oldScale;
        }

        public void TriggerElementalReaction(string reactionName, string bonusText, Color primaryColor)
        {
            _reactionBanners.Add(new ElementalReactionBanner
            {
                title = reactionName,
                subtitle = bonusText,
                color = primaryColor,
                lifetime = 1.4f,
                maxLifetime = 1.4f,
                scale = 1.4f
            });

            TriggerScreenShake(14f, 0.3f);
            TriggerHaptic(HapticType.Heavy);
            TriggerScreenFlash(new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.35f), 0.35f);
        }

        public void DrawScreenEffects()
        {
            // 1. Draw Screen Flash
            if (_screenFlashAlpha > 0.01f)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(_screenFlashColor.r, _screenFlashColor.g, _screenFlashColor.b, _screenFlashAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = oldC;
            }

            // 2. Draw Elemental Reaction Banners in virtual matrix coordinates
            if (_reactionBanners.Count > 0)
            {
                Matrix4x4 oldM = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

                for (int i = 0; i < _reactionBanners.Count; i++)
                {
                    var b = _reactionBanners[i];
                    float alpha = Mathf.Clamp01(b.lifetime / 0.4f);

                    float bannerW = 680f * b.scale;
                    float bannerH = 110f * b.scale;
                    float posX = (1080f - bannerW) * 0.5f;
                    float posY = 520f + offsetY + (i * 120f);

                    Rect bannerRect = new Rect(posX, posY, bannerW, bannerH);

                    // Glow background
                    Color oldColor = GUI.color;
                    GUI.color = new Color(b.color.r * 0.25f, b.color.g * 0.25f, b.color.b * 0.25f, 0.95f * alpha);
                    LattiruneUITheme.DrawCard(bannerRect);

                    // Border
                    GUI.color = new Color(b.color.r, b.color.g, b.color.b, alpha);
                    LattiruneUITheme.DrawBorder(bannerRect, 3f, b.color);

                    // Text
                    GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleHeroTitle);
                    titleStyle.alignment = TextAnchor.MiddleCenter;
                    titleStyle.fontSize = Mathf.RoundToInt(28 * b.scale);
                    titleStyle.normal.textColor = Color.white;

                    GUIStyle subStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    subStyle.alignment = TextAnchor.MiddleCenter;
                    subStyle.fontSize = Mathf.RoundToInt(18 * b.scale);
                    subStyle.normal.textColor = b.color;

                    GUI.Label(new Rect(posX, posY + 12f, bannerW, 42f), b.title, titleStyle);
                    GUI.Label(new Rect(posX, posY + 55f, bannerW, 35f), b.subtitle, subStyle);

                    GUI.color = oldColor;
                }

                GUI.matrix = oldM;
            }
        }
    }
}
