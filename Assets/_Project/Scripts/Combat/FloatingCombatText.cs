using System;
using UnityEngine;

namespace Lattirune.Combat
{
    public enum FloatingTextType
    {
        NormalDamage,
        CriticalDamage,
        ElementalDamage,
        Heal,
        ShieldBlock,
        StatusEffect
    }

    /// <summary>
    /// Represents an active floating damage/heal text instance drifting upwards and fading out.
    /// Strictly pooled per PLAN.md Section 23.
    /// </summary>
    [Serializable]
    public class FloatingCombatText
    {
        public bool IsActive;
        public string Text;
        public Color TextColor;
        public Vector2 ScreenPosition;
        public float Lifetime;
        public float MaxLifetime;
        public float VelocityY;
        public float Scale;

        public void Spawn(string text, Vector2 pos, FloatingTextType type, float duration = 0.85f, float scale = 1.0f)
        {
            this.IsActive = true;
            this.Text = text;
            this.ScreenPosition = pos;
            this.Lifetime = duration;
            this.MaxLifetime = duration;
            this.VelocityY = -60f; // Drift upwards on screen GUI (Y decreases)
            this.Scale = scale;

            switch (type)
            {
                case FloatingTextType.CriticalDamage:
                    this.TextColor = new Color(1f, 0.2f, 0.2f, 1f); // Bright Red
                    this.Scale = scale * 1.35f;
                    break;
                case FloatingTextType.ElementalDamage:
                    this.TextColor = new Color(1f, 0.55f, 0.1f, 1f); // Solar Orange
                    break;
                case FloatingTextType.Heal:
                    this.TextColor = new Color(0.2f, 1f, 0.3f, 1f); // Emerald Green
                    break;
                case FloatingTextType.ShieldBlock:
                    this.TextColor = new Color(0.3f, 0.8f, 1f, 1f); // Glacial Cyan
                    break;
                case FloatingTextType.StatusEffect:
                    this.TextColor = new Color(0.8f, 0.4f, 1f, 1f); // Arc Purple
                    break;
                default:
                    this.TextColor = Color.white;
                    break;
            }
        }

        public void Tick(float dt)
        {
            if (!IsActive) return;

            Lifetime -= dt;
            ScreenPosition.y += VelocityY * dt;

            if (Lifetime <= 0f)
            {
                IsActive = false;
            }
            else
            {
                // Fade alpha in the last 40% of lifetime
                float alpha = Mathf.Clamp01(Lifetime / (MaxLifetime * 0.4f));
                TextColor = new Color(TextColor.r, TextColor.g, TextColor.b, alpha);
            }
        }
    }
}
