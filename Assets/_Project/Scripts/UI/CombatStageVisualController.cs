using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Boss;
using Lattirune.Audio;

namespace Lattirune.UI
{
    /// <summary>
    /// Interactive 2D Visual Combat Stage & Character Animation Controller.
    /// Manages Hero and Enemy visual sprites, stance breathing, attack lunges,
    /// weapon slash VFX, hit recoil, red flashes, and floating damage numbers.
    /// </summary>
    public class CombatStageVisualController : MonoBehaviour
    {
        private static CombatStageVisualController s_Instance;
        public static CombatStageVisualController Instance => s_Instance;

        private float _heroLunge = 0f;
        private float _heroHitFlash = 0f;
        private float _heroAnticipation = 0f;
        private float _enemyLunge = 0f;
        private float _enemyHitFlash = 0f;
        private float _enemyAnticipation = 0f;
        private float _bossPhaseFlash = 0f;

        private struct FloatingNumber
        {
            public string text;
            public Color color;
            public Vector2 pos;
            public float velocityY;
            public float lifetime;
            public float maxLifetime;
            public float scale;
        }

        private struct ActiveVFX
        {
            public Texture2D tex;
            public Vector2 pos;
            public float size;
            public float lifetime;
            public float maxLifetime;
            public Color tint;
        }

        private readonly List<FloatingNumber> _floatingNumbers = new List<FloatingNumber>();
        private readonly List<ActiveVFX> _activeVFX = new List<ActiveVFX>();

        private void Awake()
        {
            s_Instance = this;
        }

        private void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_heroLunge > 0f) _heroLunge = Mathf.Max(0f, _heroLunge - dt * 5f);
            if (_heroHitFlash > 0f) _heroHitFlash = Mathf.Max(0f, _heroHitFlash - dt * 4f);
            if (_heroAnticipation > 0f) _heroAnticipation = Mathf.Max(0f, _heroAnticipation - dt * 4f);

            if (_enemyLunge > 0f) _enemyLunge = Mathf.Max(0f, _enemyLunge - dt * 5f);
            if (_enemyHitFlash > 0f) _enemyHitFlash = Mathf.Max(0f, _enemyHitFlash - dt * 4f);
            if (_enemyAnticipation > 0f) _enemyAnticipation = Mathf.Max(0f, _enemyAnticipation - dt * 4f);

            if (_bossPhaseFlash > 0f) _bossPhaseFlash = Mathf.Max(0f, _bossPhaseFlash - dt * 2f);

            // Update floating numbers
            for (int i = _floatingNumbers.Count - 1; i >= 0; i--)
            {
                var fn = _floatingNumbers[i];
                fn.lifetime -= dt;
                fn.pos.y += fn.velocityY * dt;
                if (fn.lifetime <= 0f)
                {
                    _floatingNumbers.RemoveAt(i);
                }
                else
                {
                    _floatingNumbers[i] = fn;
                }
            }

            // Update VFX
            for (int i = _activeVFX.Count - 1; i >= 0; i--)
            {
                var vfx = _activeVFX[i];
                vfx.lifetime -= dt;
                if (vfx.lifetime <= 0f)
                {
                    _activeVFX.RemoveAt(i);
                }
                else
                {
                    _activeVFX[i] = vfx;
                }
            }
        }

        public void TriggerHeroAttack()
        {
            _heroLunge = 1f;
            SpawnVFX("VFX/vfx_slash_fire", new Vector2(680f, 220f), 180f, 0.3f, new Color(1f, 0.8f, 0.4f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerHeroHit(int damage)
        {
            _heroHitFlash = 1f;
            SpawnFloatingNumber($"-{damage}", new Color(1f, 0.35f, 0.35f), new Vector2(250f, 130f), 1.0f, 1.2f);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(250f, 220f), 120f, 0.25f, Color.white);
            JuiceController.Instance?.TriggerScreenShake(8f, 0.2f);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
        }

        public void TriggerEnemyAttack()
        {
            _enemyLunge = 1f;
            SpawnVFX("VFX/vfx_slash_shadow", new Vector2(350f, 220f), 180f, 0.3f, new Color(0.8f, 0.3f, 1f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerEnemyHit(int damage, bool isCrit = false)
        {
            _enemyHitFlash = 1f;
            Color col = isCrit ? new Color(1f, 0.9f, 0.2f) : new Color(0.95f, 0.95f, 0.95f);
            string prefix = isCrit ? "CRIT! -" : "-";
            float scale = isCrit ? 1.5f : 1.2f;
            SpawnFloatingNumber($"{prefix}{damage}", col, new Vector2(750f, 130f), 1.0f, scale);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(750f, 220f), isCrit ? 160f : 120f, 0.25f, col);

            if (isCrit)
            {
                JuiceController.Instance?.TriggerScreenShake(14f, 0.3f);
                JuiceController.Instance?.TriggerHitStop(0.05f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Heavy);
            }
            else
            {
                JuiceController.Instance?.TriggerScreenShake(6f, 0.15f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Light);
            }
        }

        public void TriggerBossPhaseTransition()
        {
            _bossPhaseFlash = 1f;
            SpawnFloatingNumber("BOSS PHASE TRANSITION!", new Color(1f, 0.85f, 0.2f), new Vector2(500f, 80f), 2.0f, 1.6f);
            SpawnVFX("VFX/vfx_slash_lightning", new Vector2(750f, 220f), 240f, 0.6f, new Color(0.9f, 0.4f, 1f, 1f));
            JuiceController.Instance?.TriggerScreenShake(20f, 0.5f);
            JuiceController.Instance?.TriggerScreenFlash(new Color(1f, 0.8f, 0.2f, 0.5f), 0.5f);
            JuiceController.Instance?.TriggerHaptic(HapticType.Heavy);
        }

        public void SpawnFloatingNumber(string text, Color color, Vector2 startPos, float duration = 0.9f, float scale = 1.2f)
        {
            _floatingNumbers.Add(new FloatingNumber
            {
                text = text,
                color = color,
                pos = startPos,
                velocityY = -55f,
                lifetime = duration,
                maxLifetime = duration,
                scale = scale
            });
        }

        private void SpawnVFX(string path, Vector2 pos, float size, float duration, Color tint)
        {
            Texture2D tex = Resources.Load<Texture2D>("Art/" + path);
            if (tex != null)
            {
                _activeVFX.Add(new ActiveVFX
                {
                    tex = tex,
                    pos = pos,
                    size = size,
                    lifetime = duration,
                    maxLifetime = duration,
                    tint = tint
                });
            }
        }

        /// <summary>
        /// Renders the 2D Visual Battle Arena above the rune grid inside GUI matrix coordinates.
        /// </summary>
        public void DrawBattleArenaStage(
            Rect stageRect,
            Texture2D heroTexture,
            string heroName,
            int heroHp,
            int heroMaxHp,
            int heroArmor,
            int heroAtk,
            Texture2D enemyTexture,
            string enemyName,
            int enemyHp,
            int enemyMaxHp,
            int enemyArmor,
            int enemyAtk,
            bool isBoss,
            int bossPhase = 1)
        {
            // Apply screen shake offset to arena
            Vector2 shake = JuiceController.Instance != null ? JuiceController.Instance.ShakeOffset : Vector2.zero;
            stageRect.x += shake.x;
            stageRect.y += shake.y;

            // Background Arena Backdrop
            Texture2D arenaBg = VisualAssetProvider.GetBackdrop("bg_combat_arena");
            if (arenaBg != null)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.75f);
                GUI.DrawTexture(stageRect, arenaBg, ScaleMode.ScaleAndCrop);
                GUI.color = oldC;
            }

            // Stance breathing calculations
            float heroBreathing = Mathf.Sin(Time.time * 3f) * 4f;
            float enemyBreathing = Mathf.Sin(Time.time * 2.5f + 1.2f) * 4f;

            // Attack lunges & hit shake offsets
            float heroOffsetX = _heroLunge * 70f + (_heroHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 10f) : 0f);
            float enemyOffsetX = -_enemyLunge * 70f + (_enemyHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 10f) : 0f);

            float cardW = (stageRect.width - 50f) * 0.5f;
            float cardH = stageRect.height - 20f;

            // -----------------------------------------------------------------
            // HERO VISUAL CARD (LEFT)
            // -----------------------------------------------------------------
            Rect heroCardRect = new Rect(stageRect.x + 15f + heroOffsetX, stageRect.y + 10f, cardW, cardH);
            LattiruneUITheme.DrawCard(heroCardRect);

            // Hero Avatar Box
            float avatarSize = Mathf.Min(170f, cardH - 130f);
            Rect heroAvatarRect = new Rect(heroCardRect.x + (heroCardRect.width - avatarSize) * 0.5f, heroCardRect.y + 12f + heroBreathing, avatarSize, avatarSize);
            
            Color oldGUIColor = GUI.color;
            if (_heroHitFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), _heroHitFlash);
            }
            if (heroTexture != null)
            {
                GUI.DrawTexture(heroAvatarRect, heroTexture, ScaleMode.ScaleToFit);
            }
            GUI.color = oldGUIColor;

            // Hero Labels
            GUIStyle heroNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            heroNameStyle.alignment = TextAnchor.MiddleCenter;
            heroNameStyle.fontSize = 20;
            heroNameStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(heroCardRect.x + 10f, heroCardRect.y + cardH - 100f, cardW - 20f, 26f), heroName, heroNameStyle);

            GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statStyle.alignment = TextAnchor.MiddleCenter;
            statStyle.fontSize = 15;
            statStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(heroCardRect.x + 10f, heroCardRect.y + cardH - 74f, cardW - 20f, 22f), $"ATK: {heroAtk} | ARMOR: {heroArmor}", statStyle);

            // -----------------------------------------------------------------
            // ENEMY / BOSS VISUAL CARD (RIGHT)
            // -----------------------------------------------------------------
            Rect enemyCardRect = new Rect(stageRect.x + cardW + 35f + enemyOffsetX, stageRect.y + 10f, cardW, cardH);
            LattiruneUITheme.DrawCard(enemyCardRect);

            if (isBoss)
            {
                // Boss Royal Frame
                Color bossBorder = _bossPhaseFlash > 0f ? Color.white : new Color(0.95f, 0.3f, 0.2f, 0.9f);
                LattiruneUITheme.DrawBorder(enemyCardRect, 3f, bossBorder);
            }

            // Enemy Avatar Box
            float enemyAvatarSize = isBoss ? Mathf.Min(200f, cardH - 110f) : avatarSize;
            Rect enemyAvatarRect = new Rect(enemyCardRect.x + (enemyCardRect.width - enemyAvatarSize) * 0.5f, enemyCardRect.y + (isBoss ? 5f : 12f) + enemyBreathing, enemyAvatarSize, enemyAvatarSize);

            if (_enemyHitFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), _enemyHitFlash);
            }
            if (_bossPhaseFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.3f), _bossPhaseFlash);
            }
            if (enemyTexture != null)
            {
                GUI.DrawTexture(enemyAvatarRect, enemyTexture, ScaleMode.ScaleToFit);
            }
            GUI.color = oldGUIColor;

            // Enemy Labels
            GUIStyle enemyNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            enemyNameStyle.alignment = TextAnchor.MiddleCenter;
            enemyNameStyle.fontSize = isBoss ? 21 : 19;
            enemyNameStyle.fontStyle = FontStyle.Bold;
            enemyNameStyle.normal.textColor = isBoss ? new Color(1f, 0.45f, 0.45f) : new Color(0.95f, 0.4f, 0.4f);
            
            string enemyTitle = isBoss ? $"[BOSS P{bossPhase}] {enemyName}" : enemyName;
            GUI.Label(new Rect(enemyCardRect.x + 10f, enemyCardRect.y + cardH - 100f, cardW - 20f, 26f), enemyTitle, enemyNameStyle);

            GUI.Label(new Rect(enemyCardRect.x + 10f, enemyCardRect.y + cardH - 74f, cardW - 20f, 22f), $"ATK: {enemyAtk} DMG | ARMOR: {enemyArmor}", statStyle);

            // -----------------------------------------------------------------
            // RENDER COMBAT VFX OVERLAYS
            // -----------------------------------------------------------------
            for (int i = 0; i < _activeVFX.Count; i++)
            {
                var vfx = _activeVFX[i];
                float alpha = Mathf.Clamp01(vfx.lifetime / (vfx.maxLifetime * 0.5f));
                Color oldC = GUI.color;
                GUI.color = new Color(vfx.tint.r, vfx.tint.g, vfx.tint.b, alpha);
                GUI.DrawTexture(new Rect(vfx.pos.x - vfx.size * 0.5f, vfx.pos.y - vfx.size * 0.5f, vfx.size, vfx.size), vfx.tex, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }

            // -----------------------------------------------------------------
            // RENDER FLOATING DAMAGE NUMBERS
            // -----------------------------------------------------------------
            for (int i = 0; i < _floatingNumbers.Count; i++)
            {
                var fn = _floatingNumbers[i];
                float alpha = Mathf.Clamp01(fn.lifetime / (fn.maxLifetime * 0.4f));

                GUIStyle numStyle = new GUIStyle(LattiruneUITheme.StyleHeroTitle);
                numStyle.alignment = TextAnchor.MiddleCenter;
                numStyle.fontSize = Mathf.RoundToInt(26 * fn.scale);
                numStyle.fontStyle = FontStyle.Bold;
                numStyle.normal.textColor = new Color(fn.color.r, fn.color.g, fn.color.b, alpha);

                GUI.Label(new Rect(fn.pos.x - 120f, fn.pos.y, 240f, 50f), fn.text, numStyle);
            }
        }
    }
}
