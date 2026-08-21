using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Boss;
using Lattirune.Audio;

namespace Lattirune.UI
{
    /// <summary>
    /// Production-quality dark fantasy combat stage renderer.
    /// Hero/enemy large sprite presentation, animated HP bars, idle breathing,
    /// attack lunges, hit flash, floating damage numbers, VFX overlays,
    /// boss phase banners, combo indicators, elite aura rings.
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
        private float _bossPhaseTimer = 0f;
        private string _bossPhaseTitle = "";
        private float _comboFlash = 0f;
        private int _currentCombo = 0;
        private float _screenFlashAlpha = 0f;
        private Color _screenFlashColor = Color.white;

        private Texture2D _texShadowDisc;
        private Texture2D _texGoldAura;
        private Texture2D _texBossAura;
        private Texture2D _texVignetteOverlay;
        private Texture2D _texEliteAuraRing;

        private float _enemyHPBarAnim = 1f;
        private float _heroHPBarAnim = 1f;
        private float _targetEnemyHP = 1f;
        private float _targetHeroHP = 1f;

        private struct FloatingNumber
        {
            public string text;
            public Color color;
            public Vector2 pos;
            public float velocityY;
            public float lifetime;
            public float maxLifetime;
            public float scale;
            public bool isCrit;
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
            if (_texShadowDisc != null) return;

            _texShadowDisc = new Texture2D(128, 48, TextureFormat.RGBA32, false);
            for (int x = 0; x < 128; x++) for (int y = 0; y < 48; y++)
            {
                float nx = (x - 64f) / 58f; float ny = (y - 24f) / 20f;
                float d = nx * nx + ny * ny;
                _texShadowDisc.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Clamp01(1f - d) * 0.65f));
            }
            _texShadowDisc.Apply();

            _texGoldAura = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int x = 0; x < 128; x++) for (int y = 0; y < 128; y++)
            {
                float nx = (x - 64f) / 60f; float ny = (y - 64f) / 60f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01(1.2f - d) * 0.35f;
                _texGoldAura.SetPixel(x, y, new Color(0.9f, 0.72f, 0.2f, a));
            }
            _texGoldAura.Apply();

            _texBossAura = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int x = 0; x < 128; x++) for (int y = 0; y < 128; y++)
            {
                float nx = (x - 64f) / 60f; float ny = (y - 64f) / 60f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01(1.3f - d) * 0.38f;
                _texBossAura.SetPixel(x, y, new Color(1f, 0.22f, 0.18f, a));
            }
            _texBossAura.Apply();

            _texVignetteOverlay = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int x = 0; x < 64; x++) for (int y = 0; y < 64; y++)
            {
                float nx = (x - 32f) / 30f; float ny = (y - 32f) / 30f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01((d - 0.6f) * 2f) * 0.7f;
                _texVignetteOverlay.SetPixel(x, y, new Color(0f, 0f, 0.02f, a));
            }
            _texVignetteOverlay.Apply();

            _texEliteAuraRing = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int x = 0; x < 64; x++) for (int y = 0; y < 64; y++)
            {
                float nx = (x - 32f) / 30f; float ny = (y - 32f) / 30f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float ring = Mathf.Clamp01(1f - Mathf.Abs(d - 0.85f) * 8f) * 0.8f;
                _texEliteAuraRing.SetPixel(x, y, new Color(0.7f, 0.2f, 1f, ring));
            }
            _texEliteAuraRing.Apply();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _heroLunge = Mathf.Max(0f, _heroLunge - dt * 4.5f);
            _heroHitFlash = Mathf.Max(0f, _heroHitFlash - dt * 4.0f);
            _heroAnticipation = Mathf.Max(0f, _heroAnticipation - dt * 4.0f);
            _enemyLunge = Mathf.Max(0f, _enemyLunge - dt * 4.5f);
            _enemyHitFlash = Mathf.Max(0f, _enemyHitFlash - dt * 4.0f);
            _enemyAnticipation = Mathf.Max(0f, _enemyAnticipation - dt * 4.0f);
            _bossPhaseFlash = Mathf.Max(0f, _bossPhaseFlash - dt * 2.0f);
            _comboFlash = Mathf.Max(0f, _comboFlash - dt * 2.5f);
            _screenFlashAlpha = Mathf.Max(0f, _screenFlashAlpha - dt * 3.5f);
            if (_bossPhaseTimer > 0f) _bossPhaseTimer -= dt;
            _enemyHPBarAnim = Mathf.Lerp(_enemyHPBarAnim, _targetEnemyHP, dt * 6f);
            _heroHPBarAnim = Mathf.Lerp(_heroHPBarAnim, _targetHeroHP, dt * 6f);

            for (int i = _floatingNumbers.Count - 1; i >= 0; i--)
            {
                var fn = _floatingNumbers[i];
                fn.lifetime -= dt;
                fn.pos.y += fn.velocityY * dt;
                if (fn.lifetime <= 0f) _floatingNumbers.RemoveAt(i);
                else _floatingNumbers[i] = fn;
            }
            for (int i = _activeVFX.Count - 1; i >= 0; i--)
            {
                var vfx = _activeVFX[i];
                vfx.lifetime -= dt;
                if (vfx.lifetime <= 0f) _activeVFX.RemoveAt(i);
                else _activeVFX[i] = vfx;
            }
        }

        public void SetHPTargets(float heroRatio, float enemyRatio)
        {
            _targetHeroHP = Mathf.Clamp01(heroRatio);
            _targetEnemyHP = Mathf.Clamp01(enemyRatio);
        }

        public void TriggerHeroAttack()
        {
            _heroLunge = 1f;
            _currentCombo++;
            _comboFlash = 1f;
            SpawnVFX("VFX/vfx_slash_fire", new Vector2(660f, 380f), 240f, 0.3f, new Color(1f, 0.78f, 0.22f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerHeroHit(int damage)
        {
            _heroHitFlash = 1f;
            _currentCombo = 0;
            SpawnFloatingNumber($"-{damage}", new Color(1f, 0.28f, 0.28f), new Vector2(200f, 320f), 1.1f, 1.4f);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(200f, 420f), 160f, 0.28f, Color.white);
            JuiceController.Instance?.TriggerScreenShake(9f, 0.22f);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
        }

        public void TriggerEnemyAttack()
        {
            _enemyLunge = 1f;
            SpawnVFX("VFX/vfx_slash_shadow", new Vector2(380f, 380f), 240f, 0.32f, new Color(0.8f, 0.3f, 1f, 1f));
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void TriggerEnemyHit(int damage, bool isCrit = false)
        {
            _enemyHitFlash = 1f;
            Color col = isCrit ? new Color(1f, 0.92f, 0.2f) : new Color(0.96f, 0.96f, 0.96f);
            string prefix = isCrit ? "CRIT! -" : "-";
            float scale = isCrit ? 1.8f : 1.35f;
            SpawnFloatingNumber($"{prefix}{damage}", col, new Vector2(840f, 290f), 1.1f, scale, isCrit);
            SpawnVFX("VFX/vfx_impact_spark", new Vector2(840f, 400f), isCrit ? 200f : 140f, 0.28f, col);
            if (isCrit)
            {
                JuiceController.Instance?.TriggerScreenShake(16f, 0.32f);
                JuiceController.Instance?.TriggerHitStop(0.06f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Heavy);
                _screenFlashColor = new Color(1f, 0.9f, 0.1f, 0.3f);
                _screenFlashAlpha = 0.3f;
            }
            else
            {
                JuiceController.Instance?.TriggerScreenShake(6f, 0.16f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Light);
            }
        }

        public void TriggerBossPhaseTransition(string phaseTitle)
        {
            _bossPhaseFlash = 1f;
            _bossPhaseTimer = 2.5f;
            _bossPhaseTitle = phaseTitle;
            _currentCombo = 0;
            SpawnFloatingNumber(phaseTitle, new Color(1f, 0.85f, 0.2f), new Vector2(540f, 200f), 2.2f, 1.8f);
            SpawnVFX("VFX/vfx_slash_lightning", new Vector2(840f, 380f), 300f, 0.65f, new Color(0.9f, 0.35f, 1f, 1f));
            JuiceController.Instance?.TriggerScreenShake(22f, 0.55f);
            _screenFlashColor = new Color(1f, 0.8f, 0.2f, 0.6f);
            _screenFlashAlpha = 0.6f;
            JuiceController.Instance?.TriggerHaptic(HapticType.Heavy);
        }

        public void ResetCombo() => _currentCombo = 0;
        public int CurrentCombo => _currentCombo;

        public void SpawnFloatingNumber(string text, Color color, Vector2 startPos, float duration = 0.95f, float scale = 1.25f, bool isCrit = false)
        {
            _floatingNumbers.Add(new FloatingNumber
            {
                text = text, color = color, pos = startPos,
                velocityY = -70f, lifetime = duration, maxLifetime = duration,
                scale = scale, isCrit = isCrit
            });
        }

        private void SpawnVFX(string path, Vector2 pos, float size, float duration, Color tint)
        {
            Texture2D tex = Resources.Load<Texture2D>("Art/" + path);
            if (tex != null)
                _activeVFX.Add(new ActiveVFX { tex = tex, pos = pos, size = size, lifetime = duration, maxLifetime = duration, tint = tint });
        }

        public void DrawBattleArenaStage(
            Rect stageRect, Texture2D heroTexture, string heroName,
            int heroHp, int heroMaxHp, int heroArmor, int heroAtk,
            Texture2D enemyTexture, string enemyName,
            int enemyHp, int enemyMaxHp, int enemyArmor, int enemyAtk,
            bool isBoss, int bossPhase = 1,
            bool isElite = false, string eliteAffix = "")
        {
            InitializeProceduralTextures();
            SetHPTargets((float)heroHp / Mathf.Max(1, heroMaxHp), (float)enemyHp / Mathf.Max(1, enemyMaxHp));

            Vector2 shake = JuiceController.Instance != null ? JuiceController.Instance.ShakeOffset : Vector2.zero;
            stageRect.x += shake.x;
            stageRect.y += shake.y;

            float t = Time.time;
            float heroBreathY = Mathf.Sin(t * 2.8f) * 7f;
            float enemyBreathY = Mathf.Sin(t * 2.5f + 1.2f) * 7f;
            float heroScalePulse = 1f + Mathf.Sin(t * 2.8f) * 0.018f;
            float enemyScalePulse = 1f + Mathf.Sin(t * 2.5f + 1.2f) * 0.02f;
            float heroLungeOff = _heroLunge * 150f - _heroAnticipation * 28f + (_heroHitFlash > 0f ? Mathf.Sin(t * 42f) * 11f : 0f);
            float enemyLungeOff = -_enemyLunge * 150f + _enemyAnticipation * 28f + (_enemyHitFlash > 0f ? Mathf.Sin(t * 42f) * 11f : 0f);
            float groundBaseY = stageRect.y + stageRect.height - 22f;

            // Obsidian stage background
            Color oldC = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.97f);
            GUI.DrawTexture(stageRect, Texture2D.whiteTexture);
            GUI.color = oldC;
            LattiruneUITheme.DrawBorder(stageRect, 2f, new Color(0.5f, 0.42f, 0.1f, 0.8f));

            if (_texVignetteOverlay != null)
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
                GUI.DrawTexture(stageRect, _texVignetteOverlay, ScaleMode.StretchToFill);
                GUI.color = oldC;
            }

            // HERO LEFT
            float heroW = (isBoss ? 290f : 310f) * heroScalePulse;
            float heroH = (isBoss ? 360f : 400f) * heroScalePulse;
            float heroCX = stageRect.x + 210f + heroLungeOff;

            if (_texShadowDisc != null)
                GUI.DrawTexture(new Rect(heroCX - 120f, groundBaseY - 12f, 240f, 40f), _texShadowDisc);

            if (_texGoldAura != null)
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.36f + Mathf.Sin(t * 3.2f) * 0.12f);
                GUI.DrawTexture(new Rect(heroCX - 150f, groundBaseY - heroH * 0.4f, 300f, 300f), _texGoldAura);
                GUI.color = oldC;
            }

            Rect heroRect = new Rect(heroCX - heroW * 0.5f, groundBaseY - heroH + heroBreathY, heroW, heroH);
            oldC = GUI.color;
            if (_heroHitFlash > 0f) GUI.color = Color.Lerp(Color.white, new Color(1f, 0.18f, 0.18f, 1f), _heroHitFlash);
            if (heroTexture != null) GUI.DrawTexture(heroRect, heroTexture, ScaleMode.ScaleToFit);
            GUI.color = oldC;
            DrawFloatingNameBadge(new Rect(heroCX - 100f, groundBaseY + 6f, 200f, 28f), heroName.ToUpper(), LattiruneUITheme.ColorGoldBright, 15);

            // ENEMY RIGHT
            float enemyW = (isBoss ? 380f : 300f) * enemyScalePulse;
            float enemyH = (isBoss ? 450f : 360f) * enemyScalePulse;
            float enemyCX = stageRect.x + stageRect.width - 215f + enemyLungeOff;

            if (_texShadowDisc != null)
                GUI.DrawTexture(new Rect(enemyCX - (isBoss ? 150f : 120f), groundBaseY - 12f, isBoss ? 300f : 240f, isBoss ? 50f : 40f), _texShadowDisc);

            if (isBoss && _texBossAura != null)
            {
                oldC = GUI.color;
                float bossAuraPulse = 0.42f + Mathf.Sin(t * 2.2f) * 0.16f + _bossPhaseFlash * 0.3f;
                GUI.color = new Color(1f, 1f, 1f, bossAuraPulse);
                GUI.DrawTexture(new Rect(enemyCX - 200f, groundBaseY - enemyH * 0.5f, 400f, 380f), _texBossAura);
                GUI.color = oldC;
            }
            else if (isElite && _texEliteAuraRing != null)
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.38f + Mathf.Sin(t * 4f) * 0.14f);
                GUI.DrawTexture(new Rect(enemyCX - 160f, groundBaseY - enemyH * 0.5f, 320f, 320f), _texEliteAuraRing);
                GUI.color = oldC;
            }

            Rect enemyRect = new Rect(enemyCX - enemyW * 0.5f, groundBaseY - enemyH + enemyBreathY, enemyW, enemyH);
            oldC = GUI.color;
            if (_enemyHitFlash > 0f) GUI.color = Color.Lerp(Color.white, new Color(1f, 0.18f, 0.18f, 1f), _enemyHitFlash);
            if (_bossPhaseFlash > 0f) GUI.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.28f, 1f), _bossPhaseFlash * 0.7f);
            if (enemyTexture != null) GUI.DrawTexture(enemyRect, enemyTexture, ScaleMode.ScaleToFit);
            GUI.color = oldC;

            string displayName = isBoss ? $"[BOSS P{bossPhase}] {enemyName.ToUpper()}"
                               : isElite ? $"[ELITE: {eliteAffix.ToUpper()}] {enemyName.ToUpper()}"
                               : enemyName.ToUpper();
            Color nameCol = isBoss ? new Color(1f, 0.32f, 0.28f)
                          : isElite ? new Color(0.85f, 0.32f, 1f)
                          : new Color(0.96f, 0.44f, 0.44f);
            DrawFloatingNameBadge(new Rect(enemyCX - 130f, groundBaseY + 6f, 260f, 28f), displayName, nameCol, isBoss ? 13 : 15);

            // HP BARS
            float hpBarY = stageRect.y + stageRect.height - 95f;
            float hpBarW = stageRect.width * 0.44f;
            float hpBarH = 28f;

            DrawPolishedHPBar(
                new Rect(stageRect.x + 16f, hpBarY, hpBarW, hpBarH),
                _heroHPBarAnim, new Color(0.22f, 0.85f, 0.38f, 1f),
                $"HP {heroHp}/{heroMaxHp}", VisualAssetProvider.GetUIIcon("ui_icon_hp"));

            if (heroArmor > 0)
            {
                LattiruneUITheme.DrawIconValue(
                    new Rect(stageRect.x + 16f, hpBarY + hpBarH + 6f, 110f, 22f),
                    VisualAssetProvider.GetUIIcon("ui_icon_armor"), $"ARMOR: {heroArmor}", LattiruneUITheme.ColorCyanArcane, 13);
            }

            DrawPolishedHPBar(
                new Rect(stageRect.x + stageRect.width - hpBarW - 16f, hpBarY, hpBarW, hpBarH),
                _enemyHPBarAnim,
                isBoss ? new Color(1f, 0.2f, 0.2f, 1f) : isElite ? new Color(0.8f, 0.2f, 1f, 1f) : new Color(0.9f, 0.22f, 0.26f, 1f),
                $"HP {enemyHp}/{enemyMaxHp}", VisualAssetProvider.GetUIIcon("ui_icon_hp"));

            if (enemyArmor > 0)
            {
                LattiruneUITheme.DrawIconValue(
                    new Rect(stageRect.x + stageRect.width - hpBarW - 16f, hpBarY + hpBarH + 6f, 130f, 22f),
                    VisualAssetProvider.GetUIIcon("ui_icon_armor"), $"ARMOR: {enemyArmor}", LattiruneUITheme.ColorCyanArcane, 13);
            }

            // COMBO INDICATOR
            if (_currentCombo >= 5 && _comboFlash > 0f)
            {
                string comboText = _currentCombo >= 15 ? "ARCANE FRENZY!"
                                 : _currentCombo >= 10 ? "COMBO SURGE!"
                                 : "COMBO!";
                Color comboColor = _currentCombo >= 15 ? new Color(0.85f, 0.35f, 1f)
                                 : _currentCombo >= 10 ? new Color(1f, 0.75f, 0.2f)
                                 : new Color(0.35f, 0.9f, 1f);
                float comboScale = 1f + Mathf.Sin(t * 8f) * 0.06f * _comboFlash;
                GUIStyle comboStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                comboStyle.fontSize = Mathf.RoundToInt(28f * comboScale);
                comboStyle.fontStyle = FontStyle.Bold;
                comboStyle.alignment = TextAnchor.MiddleCenter;
                comboColor.a = Mathf.Clamp01(_comboFlash);
                comboStyle.normal.textColor = comboColor;
                GUI.Label(new Rect(stageRect.x + stageRect.width * 0.5f - 180f, stageRect.y + 20f, 360f, 46f), $"x{_currentCombo} {comboText}", comboStyle);
            }

            // BOSS PHASE BANNER
            if (_bossPhaseTimer > 0f)
            {
                float bannerAlpha = Mathf.Min(1f, _bossPhaseTimer * 2f, (2.5f - _bossPhaseTimer) * 2f);
                float bannerCY = stageRect.y + stageRect.height * 0.45f;
                oldC = GUI.color;
                GUI.color = new Color(0.05f, 0.02f, 0.08f, bannerAlpha * 0.88f);
                GUI.DrawTexture(new Rect(stageRect.x, bannerCY - 48f, stageRect.width, 96f), Texture2D.whiteTexture);
                GUI.color = oldC;
                LattiruneUITheme.DrawBorder(new Rect(stageRect.x + 4f, bannerCY - 44f, stageRect.width - 8f, 88f), 2f, new Color(1f, 0.8f, 0.2f, bannerAlpha));
                GUIStyle phaseStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                phaseStyle.fontSize = 26;
                phaseStyle.fontStyle = FontStyle.Bold;
                phaseStyle.alignment = TextAnchor.MiddleCenter;
                phaseStyle.normal.textColor = new Color(1f, 0.88f, 0.2f, bannerAlpha);
                GUI.Label(new Rect(stageRect.x + 20f, bannerCY - 20f, stageRect.width - 40f, 40f), $"- {_bossPhaseTitle} -", phaseStyle);
            }

            // VFX
            foreach (var vfx in _activeVFX)
            {
                if (vfx.tex == null) continue;
                float alpha = Mathf.Clamp01(vfx.lifetime / vfx.maxLifetime);
                Color vc = vfx.tint; vc.a *= alpha;
                oldC = GUI.color;
                GUI.color = vc;
                GUI.DrawTexture(new Rect(vfx.pos.x - vfx.size * 0.5f, vfx.pos.y - vfx.size * 0.5f, vfx.size, vfx.size), vfx.tex, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }
            GUI.color = Color.white;

            // FLOATING NUMBERS
            foreach (var fn in _floatingNumbers)
            {
                float alpha = Mathf.Clamp01(fn.lifetime / (fn.maxLifetime * 0.55f));
                Color fc = fn.color; fc.a = alpha;
                GUIStyle numStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                numStyle.alignment = TextAnchor.MiddleCenter;
                numStyle.fontSize = Mathf.RoundToInt((fn.isCrit ? 38f : 28f) * fn.scale);
                numStyle.fontStyle = fn.isCrit ? FontStyle.BoldAndItalic : FontStyle.Bold;
                numStyle.normal.textColor = fc;
                GUI.Label(new Rect(fn.pos.x - 140f, fn.pos.y - 26f, 280f, 52f), fn.text, numStyle);
            }

            // SCREEN FLASH
            if (_screenFlashAlpha > 0f)
            {
                oldC = GUI.color;
                Color flashC = _screenFlashColor;
                flashC.a = _screenFlashAlpha;
                GUI.color = flashC;
                GUI.DrawTexture(stageRect, Texture2D.whiteTexture);
                GUI.color = oldC;
            }
        }

        private void DrawPolishedHPBar(Rect rect, float ratio, Color fillColor, string label, Texture2D icon)
        {
            Color oldC = GUI.color;
            GUI.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldC;
            LattiruneUITheme.DrawBorder(rect, 1.5f, new Color(0.5f, 0.42f, 0.1f, 0.7f));
            float fillW = Mathf.Max(0f, (rect.width - 4f) * Mathf.Clamp01(ratio));
            if (fillW > 0f)
            {
                oldC = GUI.color;
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, fillW, rect.height - 4f), Texture2D.whiteTexture);
                GUI.color = oldC;
            }
            GUIStyle lblStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            lblStyle.alignment = TextAnchor.MiddleCenter;
            lblStyle.fontSize = 15;
            lblStyle.fontStyle = FontStyle.Bold;
            lblStyle.normal.textColor = Color.white;
            GUI.Label(rect, label, lblStyle);
            if (icon != null)
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                GUI.DrawTexture(new Rect(rect.x + 4f, rect.y + 4f, rect.height - 8f, rect.height - 8f), icon, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }
        }

        private void DrawFloatingNameBadge(Rect rect, string text, Color col, int fontSize)
        {
            Color oldC = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldC;
            LattiruneUITheme.DrawBorder(rect, 1f, col);
            GUIStyle badgeStyle = new GUIStyle(LattiruneUITheme.StyleBadge);
            badgeStyle.alignment = TextAnchor.MiddleCenter;
            badgeStyle.fontSize = fontSize;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = col;
            GUI.Label(rect, text, badgeStyle);
        }
    }
}
