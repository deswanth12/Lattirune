using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// High-performance, zero-allocation object pool for floating combat damage, crits, and heals.
    /// Strictly adheres to PLAN.md Section 15 and Section 23.
    /// </summary>
    public class FloatingCombatTextPool : MonoBehaviour
    {
        public const int POOL_SIZE = 32;

        [Header("State")]
        [SerializeField] private CombatSystem combatSystem;

        [SerializeField] private Lattirune.Reactions.ElementalReactionSystem reactionSystem;

        private readonly FloatingCombatText[] _pool = new FloatingCombatText[POOL_SIZE];
        private int _nextSpawnIndex = 0;

        public IReadOnlyList<FloatingCombatText> ActivePool => _pool;

        public void Initialize(CombatSystem combat = null, Lattirune.Reactions.ElementalReactionSystem reactions = null)
        {
            combatSystem = combat;
            reactionSystem = reactions;

            for (int i = 0; i < POOL_SIZE; i++)
            {
                _pool[i] = new FloatingCombatText();
            }

            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted += HandleAttackExecuted;
                combatSystem.OnEmergencyPotionUsed += HandleEmergencyPotionUsed;

                if (combatSystem.Effects != null)
                {
                    combatSystem.Effects.OnEffectTicked += HandleEffectTicked;
                }
            }

            if (reactionSystem != null)
            {
                reactionSystem.OnReactionActivated += HandleReactionActivated;
            }
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted -= HandleAttackExecuted;
                combatSystem.OnEmergencyPotionUsed -= HandleEmergencyPotionUsed;

                if (combatSystem.Effects != null)
                {
                    combatSystem.Effects.OnEffectTicked -= HandleEffectTicked;
                }
            }

            if (reactionSystem != null)
            {
                reactionSystem.OnReactionActivated -= HandleReactionActivated;
            }
        }

        private void HandleReactionActivated(Lattirune.Reactions.ElementalReactionResult result)
        {
            if (result == null || !result.IsActive) return;

            Vector2 spawnPos = new Vector2(540f + UnityEngine.Random.Range(-50f, 50f), 520f + UnityEngine.Random.Range(-30f, 30f));
            SpawnText($"** {result.ReactionName.ToUpper()}! **", spawnPos, FloatingTextType.ElementalDamage, duration: 1.4f);
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            // Spawn floaty over target: target center approximate on 1080x1920 portrait
            Vector2 spawnPos = new Vector2(540f + UnityEngine.Random.Range(-40f, 40f), 450f + UnityEngine.Random.Range(-20f, 20f));
            
            FloatingTextType type;
            string prefix = "";

            if (damage.IsReflected)
            {
                type = FloatingTextType.StatusEffect;
                prefix = "THORNS! ";
            }
            else if (damage.IsCritical)
            {
                type = FloatingTextType.CriticalDamage;
                prefix = "CRIT! ";
            }
            else if (damage.RuneBonus > 0)
            {
                type = FloatingTextType.ElementalDamage;
                prefix = "+RUNE ";
            }
            else
            {
                type = FloatingTextType.NormalDamage;
            }

            string text = $"{prefix}-{damage.FinalDamage}";
            SpawnText(text, spawnPos, type);
        }

        private void HandleEmergencyPotionUsed(int healAmount)
        {
            Vector2 playerPos = new Vector2(540f, 320f);
            SpawnText($"+{healAmount} HP", playerPos, FloatingTextType.Heal, duration: 1.0f);
        }

        private void HandleEffectTicked(Lattirune.Combat.Effects.CombatEffectInstance instance, float tickDamage)
        {
            if (tickDamage <= 0) return;
            Vector2 pos = new Vector2(540f + UnityEngine.Random.Range(-30f, 30f), 480f + UnityEngine.Random.Range(-15f, 15f));
            SpawnText($"-{Mathf.RoundToInt(tickDamage)} ({instance.Definition.DisplayName})", pos, FloatingTextType.StatusEffect, duration: 0.85f);
        }

        public void BindComboTracker(Lattirune.Combo.ComboTracker combo)
        {
            if (combo != null)
            {
                combo.OnComboIncremented += HandleComboIncremented;
                combo.OnReactionChainIncremented += HandleReactionChainIncremented;
            }
        }

        private void HandleComboIncremented(int combo, float multiplier)
        {
            if (combo == 5 || combo == 10 || combo == 15 || combo == 20 || combo == 25 || combo == 30)
            {
                Vector2 spawnPos = new Vector2(540f, 380f);
                SpawnText($"COMBO x{combo}! ({multiplier:0.00}x DMG)", spawnPos, FloatingTextType.CriticalDamage, duration: 1.5f);
                Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.Victory, 0.6f);
                Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Medium);
            }
        }

        private void HandleReactionChainIncremented(int chainCount)
        {
            if (chainCount >= 2)
            {
                Vector2 spawnPos = new Vector2(540f, 440f);
                SpawnText($"CHAIN REACTION x{chainCount}!", spawnPos, FloatingTextType.ElementalDamage, duration: 1.6f);
                Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.RuneConduitIgnite, 0.8f);
                Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Heavy);
            }
        }

        public FloatingCombatText SpawnText(string text, Vector2 screenPos, FloatingTextType type, float duration = 0.85f)
        {
            // Find inactive or recycle round-robin
            FloatingCombatText candidate = null;
            for (int i = 0; i < POOL_SIZE; i++)
            {
                int idx = (_nextSpawnIndex + i) % POOL_SIZE;
                if (!_pool[idx].IsActive)
                {
                    candidate = _pool[idx];
                    _nextSpawnIndex = (idx + 1) % POOL_SIZE;
                    break;
                }
            }

            if (candidate == null)
            {
                candidate = _pool[_nextSpawnIndex];
                _nextSpawnIndex = (_nextSpawnIndex + 1) % POOL_SIZE;
            }

            candidate.Spawn(text, screenPos, type, duration);
            return candidate;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                if (_pool[i].IsActive)
                {
                    _pool[i].Tick(dt);
                }
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        [SerializeField] private UI.ScreenNavigationController navigation;

        public void BindNavigation(UI.ScreenNavigationController nav)
        {
            navigation = nav;
        }

        private void OnGUI()
        {
            if (navigation == null || (navigation.CurrentScreen != UI.ScreenState.GRID_BUILD && navigation.CurrentScreen != UI.ScreenState.COMBAT)) return;
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < POOL_SIZE; i++)
            {
                var entry = _pool[i];
                if (!entry.IsActive) continue;

                labelStyle.fontSize = Mathf.RoundToInt(26 * entry.Scale);
                labelStyle.fontStyle = FontStyle.Bold;

                // High-contrast drop shadow
                GUIStyle shadowStyle = new GUIStyle(labelStyle);
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, entry.TextColor.a * 0.85f);

                GUI.Label(new Rect(entry.ScreenPosition.x - 150 + 2, entry.ScreenPosition.y - 30 + 2, 300, 60), entry.Text, shadowStyle);
                GUI.Label(new Rect(entry.ScreenPosition.x - 150 - 2, entry.ScreenPosition.y - 30 - 2, 300, 60), entry.Text, shadowStyle);

                // Foreground text
                labelStyle.normal.textColor = entry.TextColor;
                GUI.Label(new Rect(entry.ScreenPosition.x - 150, entry.ScreenPosition.y - 30, 300, 60), entry.Text, labelStyle);
            }

            GUI.matrix = oldMatrix;
        }
    }
}
