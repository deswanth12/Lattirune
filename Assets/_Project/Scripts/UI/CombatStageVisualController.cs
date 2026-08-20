using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Boss;

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
        private float _enemyLunge = 0f;
        private float _enemyHitFlash = 0f;
        private float _bossPhaseFlash = 0f;

        private struct FloatingNumber
        {
            public string text;
            public Color color;
            public Vector2 pos;
            public float velocityY;
            public float lifetime;
            public float maxLifetime;
        }

        private struct ActiveVFX
        {
            public Texture2D tex;
            public Vector2 pos;
            public float size;
            public float lifetime;
            public float maxLifetime;
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

            if (_heroLunge > 0f) _heroLunge = Mathf.Max(0f, _heroLunge - dt * 4f);
            if (_heroHitFlash > 0f) _heroHitFlash = Mathf.Max(0f, _heroHitFlash - dt * 3f);
            if (_enemyLunge > 0f) _enemyLunge = Mathf.Max(0f, _enemyLunge - dt * 4f);
            if (_enemyHitFlash > 0f) _enemyHitFlash = Mathf.Max(0f, _enemyHitFlash - dt * 3f);
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
            SpawnVFX("VFX/vfx_slash_fire", new Vector2(680f, 220f), 140f, 0.25f);
        }

        public void TriggerHeroHit(int damage)
        {
            _heroHitFlash = 1f;
            SpawnFloatingNumber($"-{damage}", new Color(1f, 0.3f, 0.3f), new Vector2(250f, 150f));
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(250f, 220f), 100f, 0.2f);
        }

        public void TriggerEnemyAttack()
        {
            _enemyLunge = 1f;
            SpawnVFX("VFX/vfx_slash_shadow", new Vector2(350f, 220f), 140f, 0.25f);
        }

        public void TriggerEnemyHit(int damage, bool isCrit = false)
        {
            _enemyHitFlash = 1f;
            Color col = isCrit ? new Color(1f, 0.9f, 0.2f) : new Color(0.95f, 0.95f, 0.95f);
            string prefix = isCrit ? "CRIT! -" : "-";
            SpawnFloatingNumber($"{prefix}{damage}", col, new Vector2(750f, 150f));
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(750f, 220f), 110f, 0.2f);
        }

        public void TriggerBossPhaseTransition()
        {
            _bossPhaseFlash = 1f;
            SpawnFloatingNumber("PHASE TRANSITION!", new Color(1f, 0.85f, 0.2f), new Vector2(500f, 100f), 1.5f);
            SpawnVFX("VFX/vfx_slash_lightning", new Vector2(750f, 220f), 200f, 0.5f);
        }

        public void SpawnFloatingNumber(string text, Color color, Vector2 startPos, float duration = 0.8f)
        {
            _floatingNumbers.Add(new FloatingNumber
            {
                text = text,
                color = color,
                pos = startPos,
                velocityY = -60f,
                lifetime = duration,
                maxLifetime = duration
            });
        }

        private void SpawnVFX(string path, Vector2 pos, float size, float duration)
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
                    maxLifetime = duration
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
            // Background Arena Backdrop
            Texture2D arenaBg = VisualAssetProvider.GetBackdrop("bg_combat_arena");
            if (arenaBg != null)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.65f);
                GUI.DrawTexture(stageRect, arenaBg, ScaleMode.ScaleAndCrop);
                GUI.color = oldC;
            }

            // Stance breathing calculations
            float heroBreathing = Mathf.Sin(Time.time * 3f) * 4f;
            float enemyBreathing = Mathf.Sin(Time.time * 2.5f + 1.2f) * 4f;

            // Attack lunges & hit shake offsets
            float heroOffsetX = _heroLunge * 60f + (_heroHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 8f) : 0f);
            float enemyOffsetX = -_enemyLunge * 60f + (_enemyHitFlash > 0 ? (Mathf.Sin(Time.time * 40f) * 8f) : 0f);

            float cardW = (stageRect.width - 60f) * 0.5f;
            float cardH = stageRect.height - 20f;

            // -----------------------------------------------------------------
            // HERO VISUAL CARD (LEFT)
            // -----------------------------------------------------------------
            Rect heroCardRect = new Rect(stageRect.x + 15f + heroOffsetX, stageRect.y + 10f, cardW, cardH);
            LattiruneUITheme.DrawCard(heroCardRect);

            // Hero Avatar Box
            float avatarSize = Mathf.Min(170f, cardH - 140f);
            Rect heroAvatarRect = new Rect(heroCardRect.x + (heroCardRect.width - avatarSize) * 0.5f, heroCardRect.y + 15f + heroBreathing, avatarSize, avatarSize);
            
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

            // Hero Info & Stats
            GUIStyle nameStyle = new GUIStyle(LattiruneUITheme.StyleHeaderTitle);
            nameStyle.fontSize = 20;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUI.Label(new Rect(heroCardRect.x + 10f, heroCardRect.y + avatarSize + 22f, heroCardRect.width - 20f, 26f), heroName, nameStyle);

            // Hero HP Bar
            Rect heroHpRect = new Rect(heroCardRect.x + 15f, heroCardRect.y + avatarSize + 52f, heroCardRect.width - 30f, 22f);
            LattiruneUITheme.DrawProgressBar(heroHp, heroMaxHp, $"HP {heroHp}/{heroMaxHp}", LattiruneUITheme.ColorGreenHealth, 22f);

            // Hero Stats
            GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statStyle.fontSize = 15;
            statStyle.alignment = TextAnchor.MiddleCenter;
            statStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
            string heroStats = $"ATK: {heroAtk}  |  ARMOR: {heroArmor}";
            GUI.Label(new Rect(heroCardRect.x + 10f, heroCardRect.y + avatarSize + 78f, heroCardRect.width - 20f, 22f), heroStats, statStyle);

            // -----------------------------------------------------------------
            // ENEMY / BOSS VISUAL CARD (RIGHT)
            // -----------------------------------------------------------------
            Rect enemyCardRect = new Rect(stageRect.x + stageRect.width - cardW - 15f + enemyOffsetX, stageRect.y + 10f, cardW, cardH);
            LattiruneUITheme.DrawCard(enemyCardRect);

            // Enemy Avatar Box
            Rect enemyAvatarRect = new Rect(enemyCardRect.x + (enemyCardRect.width - avatarSize) * 0.5f, enemyCardRect.y + 15f + enemyBreathing, avatarSize, avatarSize);
            
            if (_enemyHitFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), _enemyHitFlash);
            }
            else if (_bossPhaseFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.2f), _bossPhaseFlash);
            }
            if (enemyTexture != null)
            {
                GUI.DrawTexture(enemyAvatarRect, enemyTexture, ScaleMode.ScaleToFit);
            }
            GUI.color = oldGUIColor;

            // Enemy Info & Stats
            GUIStyle enemyNameStyle = new GUIStyle(LattiruneUITheme.StyleHeaderTitle);
            enemyNameStyle.fontSize = 20;
            enemyNameStyle.alignment = TextAnchor.MiddleCenter;
            enemyNameStyle.normal.textColor = isBoss ? new Color(1f, 0.85f, 0.2f) : LattiruneUITheme.ColorRedDanger;
            string bossPhaseTag = isBoss ? $" [PHASE {bossPhase}]" : "";
            GUI.Label(new Rect(enemyCardRect.x + 10f, enemyCardRect.y + avatarSize + 22f, enemyCardRect.width - 20f, 26f), enemyName + bossPhaseTag, enemyNameStyle);

            // Enemy HP Bar
            Rect enemyHpRect = new Rect(enemyCardRect.x + 15f, enemyCardRect.y + avatarSize + 52f, enemyCardRect.width - 30f, 22f);
            LattiruneUITheme.DrawProgressBar(enemyHp, enemyMaxHp, $"HP {enemyHp}/{enemyMaxHp}", LattiruneUITheme.ColorRedDanger, 22f);

            // Enemy Stats
            string enemyStats = $"ATK: {enemyAtk} DMG  |  ARMOR: {enemyArmor}";
            GUI.Label(new Rect(enemyCardRect.x + 10f, enemyCardRect.y + avatarSize + 78f, enemyCardRect.width - 20f, 22f), enemyStats, statStyle);

            // -----------------------------------------------------------------
            // DRAW ACTIVE VFX & FLOATING NUMBERS
            // -----------------------------------------------------------------
            foreach (var vfx in _activeVFX)
            {
                float alpha = vfx.lifetime / vfx.maxLifetime;
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                Rect vfxRect = new Rect(stageRect.x + vfx.pos.x - vfx.size * 0.5f, stageRect.y + vfx.pos.y - vfx.size * 0.5f, vfx.size, vfx.size);
                GUI.DrawTexture(vfxRect, vfx.tex, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }

            foreach (var fn in _floatingNumbers)
            {
                float alpha = Mathf.Clamp01(fn.lifetime / (fn.maxLifetime * 0.5f));
                GUIStyle fnStyle = new GUIStyle(LattiruneUITheme.StyleHeaderTitle);
                fnStyle.fontSize = 26;
                fnStyle.alignment = TextAnchor.MiddleCenter;
                fnStyle.normal.textColor = new Color(fn.color.r, fn.color.g, fn.color.b, alpha);
                GUI.Label(new Rect(stageRect.x + fn.pos.x - 120f, stageRect.y + fn.pos.y - 20f, 240f, 40f), fn.text, fnStyle);
            }
        }
    }
}
