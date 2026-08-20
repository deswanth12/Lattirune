using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Boss;
using Lattirune.Audio;

namespace Lattirune.UI
{
    /// <summary>
    /// Dedicated High-Fidelity 2D Combat Stage & Battle Arena Visual Controller.
    /// Manages full-size character artwork, ground shadows, dynamic idle breathing,
    /// attack anticipation & lunges, impact recoil, red hit flashes, elemental slash VFX,
    /// and bouncing critical floating damage numbers.
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

        private Texture2D _texShadowDisc;
        private Texture2D _texHeroAura;
        private Texture2D _texArenaFloor;

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
            InitializeProceduralTextures();
        }

        private void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        private void InitializeProceduralTextures()
        {
            if (_texShadowDisc == null)
            {
                _texShadowDisc = new Texture2D(64, 32, TextureFormat.RGBA32, false);
                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        float nx = (x - 32f) / 30f;
                        float ny = (y - 16f) / 14f;
                        float dist = (nx * nx) + (ny * ny);
                        float alpha = Mathf.Clamp01(1f - dist) * 0.55f;
                        _texShadowDisc.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                    }
                }
                _texShadowDisc.Apply();
            }

            if (_texHeroAura == null)
            {
                _texHeroAura = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        float nx = (x - 32f) / 30f;
                        float ny = (y - 32f) / 30f;
                        float dist = Mathf.Sqrt(nx * nx + ny * ny);
                        float alpha = Mathf.Clamp01(1f - dist) * 0.35f;
                        _texHeroAura.SetPixel(x, y, new Color(0.84f, 0.65f, 0.16f, alpha));
                    }
                }
                _texHeroAura.Apply();
            }

            if (_texArenaFloor == null)
            {
                _texArenaFloor = new Texture2D(128, 64, TextureFormat.RGBA32, false);
                Color darkStone = new Color(0.06f, 0.08f, 0.12f, 0.95f);
                Color borderGold = new Color(0.84f, 0.65f, 0.16f, 0.40f);
                for (int x = 0; x < 128; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        bool isBorder = y == 0 || y == 63 || x == 0 || x == 127;
                        _texArenaFloor.SetPixel(x, y, isBorder ? borderGold : darkStone);
                    }
                }
                _texArenaFloor.Apply();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_heroLunge > 0f) _heroLunge = Mathf.Max(0f, _heroLunge - dt * 4.5f);
            if (_heroHitFlash > 0f) _heroHitFlash = Mathf.Max(0f, _heroHitFlash - dt * 4.0f);
            if (_heroAnticipation > 0f) _heroAnticipation = Mathf.Max(0f, _heroAnticipation - dt * 4.0f);

            if (_enemyLunge > 0f) _enemyLunge = Mathf.Max(0f, _enemyLunge - dt * 4.5f);
            if (_enemyHitFlash > 0f) _enemyHitFlash = Mathf.Max(0f, _enemyHitFlash - dt * 4.0f);
            if (_enemyAnticipation > 0f) _enemyAnticipation = Mathf.Max(0f, _enemyAnticipation - dt * 4.0f);

            if (_bossPhaseFlash > 0f) _bossPhaseFlash = Mathf.Max(0f, _bossPhaseFlash - dt * 2.0f);

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
            SpawnVFX("VFX/vfx_slash_fire", new Vector2(620f, 380f), 220f, 0.35f, new Color(1f, 0.75f, 0.2f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerHeroHit(int damage)
        {
            _heroHitFlash = 1f;
            SpawnFloatingNumber($"-{damage}", new Color(1f, 0.35f, 0.35f), new Vector2(240f, 260f), 1.0f, 1.3f);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(240f, 380f), 140f, 0.25f, Color.white);
            JuiceController.Instance?.TriggerScreenShake(8f, 0.2f);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
        }

        public void TriggerEnemyAttack()
        {
            _enemyLunge = 1f;
            SpawnVFX("VFX/vfx_slash_shadow", new Vector2(420f, 380f), 220f, 0.35f, new Color(0.85f, 0.3f, 1f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerEnemyHit(int damage, bool isCrit = false)
        {
            _enemyHitFlash = 1f;
            Color col = isCrit ? new Color(1f, 0.9f, 0.2f) : new Color(0.95f, 0.95f, 0.95f);
            string prefix = isCrit ? "CRIT! -" : "-";
            float scale = isCrit ? 1.6f : 1.25f;
            SpawnFloatingNumber($"{prefix}{damage}", col, new Vector2(800f, 260f), 1.0f, scale);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(800f, 380f), isCrit ? 180f : 130f, 0.25f, col);

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
            SpawnFloatingNumber("BOSS PHASE TRANSITION!", new Color(1f, 0.85f, 0.2f), new Vector2(540f, 220f), 2.0f, 1.7f);
            SpawnVFX("VFX/vfx_slash_lightning", new Vector2(800f, 380f), 280f, 0.6f, new Color(0.9f, 0.4f, 1f, 1f));
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
                velocityY = -60f,
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
        /// Renders the Dedicated Open Combat Stage (Hero vs Enemy) in the upper half of the screen.
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
            InitializeProceduralTextures();

            // Apply screen shake offset to arena
            Vector2 shake = JuiceController.Instance != null ? JuiceController.Instance.ShakeOffset : Vector2.zero;
            stageRect.x += shake.x;
            stageRect.y += shake.y;

            // 1. Arena Floor / Atmospheric Backdrop
            if (_texArenaFloor != null)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                GUI.DrawTexture(stageRect, _texArenaFloor, ScaleMode.StretchToFill);
                LattiruneUITheme.DrawBorder(stageRect, 2f, new Color(0.84f, 0.65f, 0.16f, 0.50f));
                GUI.color = oldC;
            }

            // Stance breathing calculations
            float heroBreathingY = Mathf.Sin(Time.time * 2.8f) * 6f;
            float enemyBreathingY = Mathf.Sin(Time.time * 2.5f + 1.2f) * 6f;
            float heroScalePulse = 1f + Mathf.Sin(Time.time * 2.8f) * 0.02f;
            float enemyScalePulse = 1f + Mathf.Sin(Time.time * 2.5f + 1.2f) * 0.02f;

            // Attack lunges & hit shake offsets
            float heroLungeOffset = _heroLunge * 140f - _heroAnticipation * 30f + (_heroHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 12f) : 0f);
            float enemyLungeOffset = -_enemyLunge * 140f + _enemyAnticipation * 30f + (_enemyHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 12f) : 0f);

            // Ground base Y position
            float groundBaseY = stageRect.y + stageRect.height - 50f;

            // =================================================================
            // 2. HERO CHARACTER SILHOUETTE (LEFT)
            // =================================================================
            float heroWidth = 260f * heroScalePulse;
            float heroHeight = 320f * heroScalePulse;
            float heroCenterX = stageRect.x + 220f + heroLungeOffset;

            // Hero Shadow Disc
            if (_texShadowDisc != null)
            {
                Rect heroShadowRect = new Rect(heroCenterX - 100f, groundBaseY - 15f, 200f, 35f);
                GUI.DrawTexture(heroShadowRect, _texShadowDisc);
            }

            // Hero Golden Aura Disc
            if (_texHeroAura != null)
            {
                Color oldAura = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.40f + Mathf.Sin(Time.time * 3f) * 0.15f);
                Rect heroAuraRect = new Rect(heroCenterX - 110f, groundBaseY - 25f, 220f, 50f);
                GUI.DrawTexture(heroAuraRect, _texHeroAura);
                GUI.color = oldAura;
            }

            // Hero Character Artwork
            Rect heroCharRect = new Rect(heroCenterX - heroWidth * 0.5f, groundBaseY - heroHeight + heroBreathingY, heroWidth, heroHeight);
            Color oldHeroColor = GUI.color;
            if (_heroHitFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), _heroHitFlash);
            }
            if (heroTexture != null)
            {
                GUI.DrawTexture(heroCharRect, heroTexture, ScaleMode.ScaleToFit);
            }
            GUI.color = oldHeroColor;

            // Hero Floating Name Badge
            GUIStyle heroBadgeStyle = new GUIStyle(LattiruneUITheme.StyleBadge);
            heroBadgeStyle.alignment = TextAnchor.MiddleCenter;
            heroBadgeStyle.fontSize = 16;
            heroBadgeStyle.fontStyle = FontStyle.Bold;
            heroBadgeStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            Rect heroBadgeRect = new Rect(heroCenterX - 90f, groundBaseY + 6f, 180f, 28f);
            GUI.Label(heroBadgeRect, $"🛡️ {heroName.ToUpper()}", heroBadgeStyle);

            // =================================================================
            // 3. ENEMY / BOSS CHARACTER SILHOUETTE (RIGHT)
            // =================================================================
            float enemyWidth = (isBoss ? 340f : 260f) * enemyScalePulse;
            float enemyHeight = (isBoss ? 380f : 300f) * enemyScalePulse;
            float enemyCenterX = stageRect.x + stageRect.width - 220f + enemyLungeOffset;

            // Enemy Shadow Disc
            if (_texShadowDisc != null)
            {
                Rect enemyShadowRect = new Rect(enemyCenterX - (isBoss ? 130f : 100f), groundBaseY - 15f, isBoss ? 260f : 200f, isBoss ? 45f : 35f);
                GUI.DrawTexture(enemyShadowRect, _texShadowDisc);
            }

            // Enemy Character Artwork
            Rect enemyCharRect = new Rect(enemyCenterX - enemyWidth * 0.5f, groundBaseY - enemyHeight + enemyBreathingY, enemyWidth, enemyHeight);
            Color oldEnemyColor = GUI.color;
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
                GUI.DrawTexture(enemyCharRect, enemyTexture, ScaleMode.ScaleToFit);
            }
            GUI.color = oldEnemyColor;

            // Enemy Floating Name Badge
            GUIStyle enemyBadgeStyle = new GUIStyle(LattiruneUITheme.StyleBadge);
            enemyBadgeStyle.alignment = TextAnchor.MiddleCenter;
            enemyBadgeStyle.fontSize = 16;
            enemyBadgeStyle.fontStyle = FontStyle.Bold;
            enemyBadgeStyle.normal.textColor = isBoss ? new Color(1f, 0.35f, 0.35f) : new Color(0.95f, 0.45f, 0.45f);
            Rect enemyBadgeRect = new Rect(enemyCenterX - 100f, groundBaseY + 6f, 200f, 28f);
            string enemyLabel = isBoss ? $"👑 [BOSS P{bossPhase}] {enemyName.ToUpper()}" : $"💀 {enemyName.ToUpper()}";
            GUI.Label(enemyBadgeRect, enemyLabel, enemyBadgeStyle);

            // =================================================================
            // 4. ACTIVE VFX OVERLAYS (Slashes, Sparks, Spell Bursts)
            // =================================================================
            foreach (var vfx in _activeVFX)
            {
                if (vfx.tex != null)
                {
                    float alpha = Mathf.Clamp01(vfx.lifetime / vfx.maxLifetime);
                    Color c = vfx.tint;
                    c.a *= alpha;
                    GUI.color = c;
                    Rect vfxRect = new Rect(vfx.pos.x - vfx.size * 0.5f, vfx.pos.y - vfx.size * 0.5f, vfx.size, vfx.size);
                    GUI.DrawTexture(vfxRect, vfx.tex, ScaleMode.ScaleToFit);
                }
            }
            GUI.color = Color.white;

            // =================================================================
            // 5. FLOATING BOUNCY DAMAGE NUMBERS
            // =================================================================
            foreach (var fn in _floatingNumbers)
            {
                float alpha = Mathf.Clamp01(fn.lifetime / (fn.maxLifetime * 0.5f));
                Color c = fn.color;
                c.a = alpha;

                GUIStyle numStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                numStyle.alignment = TextAnchor.MiddleCenter;
                numStyle.fontSize = Mathf.RoundToInt(28f * fn.scale);
                numStyle.fontStyle = FontStyle.Bold;
                numStyle.normal.textColor = c;

                Rect numRect = new Rect(fn.pos.x - 120f, fn.pos.y - 20f, 240f, 40f);
                GUI.Label(numRect, fn.text, numStyle);
            }
        }
    }
}
