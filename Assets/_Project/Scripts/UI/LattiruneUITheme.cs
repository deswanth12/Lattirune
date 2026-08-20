using UnityEngine;

namespace Lattirune.UI
{
    /// <summary>
    /// Centralized Dark Fantasy + Mobile Roguelite UI Design System for Lattirune.
    /// Provides consistent colors, procedural textures, GUIStyles, touch-target sizing,
    /// and responsive 1080x2412 layout calculations across all UI controllers.
    /// </summary>
    public static class LattiruneUITheme
    {
        // ----------------------------------------------------------------------
        // Color Palette (Strictly adheres to Design System Specification)
        // ----------------------------------------------------------------------
        public static readonly Color ColorObsidianBg     = new Color(0.031f, 0.043f, 0.071f, 0.98f); // #080B12
        public static readonly Color ColorSurfaceDark     = new Color(0.067f, 0.090f, 0.133f, 0.98f); // #111722
        public static readonly Color ColorSurfaceCard     = new Color(0.094f, 0.129f, 0.192f, 0.96f); // #182131
        public static readonly Color ColorSurfaceHeader   = new Color(0.050f, 0.065f, 0.100f, 1.00f);

        public static readonly Color ColorGoldPrimary    = new Color(0.839f, 0.651f, 0.165f, 1.00f); // #D6A62A
        public static readonly Color ColorGoldBright     = new Color(0.957f, 0.788f, 0.290f, 1.00f); // #F4C94A
        public static readonly Color ColorBorderGold     = new Color(0.839f, 0.651f, 0.165f, 0.85f);
        public static readonly Color ColorBorderMuted    = new Color(0.200f, 0.260f, 0.360f, 0.70f);

        public static readonly Color ColorTextPrimary    = new Color(0.949f, 0.949f, 0.910f, 1.00f); // #F2F2E8
        public static readonly Color ColorTextMuted      = new Color(0.604f, 0.643f, 0.710f, 1.00f); // #9AA4B5
        public static readonly Color ColorRedDanger      = new Color(0.784f, 0.231f, 0.271f, 1.00f); // #C83B45

        public static readonly Color ColorCyanArcane     = new Color(0.125f, 0.839f, 0.910f, 1.00f); // #20D6E8
        public static readonly Color ColorFireElemental  = new Color(1.000f, 0.478f, 0.188f, 1.00f); // #FF7A30
        public static readonly Color ColorShadowRune     = new Color(0.608f, 0.361f, 1.000f, 1.00f); // #9B5CFF
        public static readonly Color ColorGreenHealth    = new Color(0.220f, 0.820f, 0.450f, 1.00f);

        // ----------------------------------------------------------------------
        // Procedural Textures
        // ----------------------------------------------------------------------
        private static Texture2D _texObsidian;
        private static Texture2D _texSurface;
        private static Texture2D _texCard;
        private static Texture2D _texHeader;
        private static Texture2D _texBtnPrimary;
        private static Texture2D _texBtnPrimaryHover;
        private static Texture2D _texBtnSecondary;
        private static Texture2D _texBtnSecondaryHover;
        private static Texture2D _texBtnDanger;
        private static Texture2D _texBtnDisabled;
        private static Texture2D _texProgressBarBg;
        private static Texture2D _texProgressBarFillHp;
        private static Texture2D _texProgressBarFillShield;
        private static Texture2D _texBadgeBg;

        // ----------------------------------------------------------------------
        // Cached GUIStyles
        // ----------------------------------------------------------------------
        private static GUIStyle _styleWindow;
        private static GUIStyle _styleHeaderTitle;
        private static GUIStyle _styleHeaderSubtitle;
        private static GUIStyle _styleSectionTitle;
        private static GUIStyle _stylePrimaryBtn;
        private static GUIStyle _styleSecondaryBtn;
        private static GUIStyle _styleDangerBtn;
        private static GUIStyle _styleCard;
        private static GUIStyle _styleBadge;
        private static GUIStyle _styleResourcePill;
        private static GUIStyle _styleStatLabel;

        private static bool _initialized = false;

        public static void EnsureInitialized()
        {
            if (_initialized && _texObsidian != null && _stylePrimaryBtn != null && _styleCard != null) return;
            _initialized = false;

            _texObsidian = CreateSolidTexture(ColorObsidianBg);
            _texSurface = CreateSolidTexture(ColorSurfaceDark);
            _texCard = CreateSolidTexture(ColorSurfaceCard);
            _texHeader = CreateSolidTexture(ColorSurfaceHeader);

            _texBtnPrimary = CreateGradientTexture(ColorGoldBright, ColorGoldPrimary, 64);
            _texBtnPrimaryHover = CreateGradientTexture(new Color(1f, 0.88f, 0.45f), ColorGoldBright, 64);
            _texBtnSecondary = CreateGradientTexture(new Color(0.12f, 0.16f, 0.24f), new Color(0.08f, 0.11f, 0.17f), 64);
            _texBtnSecondaryHover = CreateGradientTexture(new Color(0.18f, 0.24f, 0.35f), new Color(0.12f, 0.16f, 0.24f), 64);
            _texBtnDanger = CreateGradientTexture(new Color(0.85f, 0.25f, 0.30f), ColorRedDanger, 64);
            _texBtnDisabled = CreateSolidTexture(new Color(0.12f, 0.14f, 0.18f, 0.60f));

            _texProgressBarBg = CreateSolidTexture(new Color(0.05f, 0.07f, 0.11f, 0.95f));
            _texProgressBarFillHp = CreateGradientTexture(new Color(0.30f, 0.88f, 0.50f), ColorGreenHealth, 32);
            _texProgressBarFillShield = CreateGradientTexture(new Color(0.30f, 0.85f, 0.98f), ColorCyanArcane, 32);
            _texBadgeBg = CreateSolidTexture(new Color(0.10f, 0.14f, 0.22f, 0.92f));

            // Setup GUIStyles safely
            try
            {
                if (GUI.skin == null) return;

                _styleWindow = new GUIStyle(GUI.skin.box);
                _styleWindow.normal.background = _texObsidian;

                _styleHeaderTitle = new GUIStyle(GUI.skin.label);
                _styleHeaderTitle.fontSize = 38;
                _styleHeaderTitle.fontStyle = FontStyle.Bold;
                _styleHeaderTitle.alignment = TextAnchor.MiddleCenter;
                _styleHeaderTitle.normal.textColor = ColorGoldPrimary;

                _styleHeaderSubtitle = new GUIStyle(GUI.skin.label);
                _styleHeaderSubtitle.fontSize = 18;
                _styleHeaderSubtitle.fontStyle = FontStyle.Italic;
                _styleHeaderSubtitle.alignment = TextAnchor.MiddleCenter;
                _styleHeaderSubtitle.normal.textColor = ColorTextMuted;

                _styleSectionTitle = new GUIStyle(GUI.skin.label);
                _styleSectionTitle.fontSize = 24;
                _styleSectionTitle.fontStyle = FontStyle.Bold;
                _styleSectionTitle.normal.textColor = ColorGoldBright;

                _stylePrimaryBtn = new GUIStyle(GUI.skin.button);
                _stylePrimaryBtn.fontSize = 24;
                _stylePrimaryBtn.fontStyle = FontStyle.Bold;
                _stylePrimaryBtn.alignment = TextAnchor.MiddleCenter;
                _stylePrimaryBtn.normal.background = _texBtnPrimary;
                _stylePrimaryBtn.normal.textColor = ColorObsidianBg;
                _stylePrimaryBtn.hover.background = _texBtnPrimaryHover;
                _stylePrimaryBtn.active.background = _texBtnPrimaryHover;

                _styleSecondaryBtn = new GUIStyle(GUI.skin.button);
                _styleSecondaryBtn.fontSize = 22;
                _styleSecondaryBtn.fontStyle = FontStyle.Bold;
                _styleSecondaryBtn.alignment = TextAnchor.MiddleCenter;
                _styleSecondaryBtn.normal.background = _texBtnSecondary;
                _styleSecondaryBtn.normal.textColor = ColorTextPrimary;
                _styleSecondaryBtn.hover.background = _texBtnSecondaryHover;
                _styleSecondaryBtn.active.background = _texBtnSecondaryHover;

                _styleDangerBtn = new GUIStyle(GUI.skin.button);
                _styleDangerBtn.fontSize = 22;
                _styleDangerBtn.fontStyle = FontStyle.Bold;
                _styleDangerBtn.alignment = TextAnchor.MiddleCenter;
                _styleDangerBtn.normal.background = _texBtnDanger;
                _styleDangerBtn.normal.textColor = Color.white;

                _styleCard = new GUIStyle(GUI.skin.box);
                _styleCard.normal.background = _texCard;

                _styleBadge = new GUIStyle(GUI.skin.box);
                _styleBadge.normal.background = _texBadgeBg;
                _styleBadge.fontSize = 16;
                _styleBadge.fontStyle = FontStyle.Bold;
                _styleBadge.alignment = TextAnchor.MiddleCenter;
                _styleBadge.normal.textColor = ColorCyanArcane;

                _styleResourcePill = new GUIStyle(GUI.skin.box);
                _styleResourcePill.normal.background = _texHeader;
                _styleResourcePill.fontSize = 20;
                _styleResourcePill.fontStyle = FontStyle.Bold;
                _styleResourcePill.alignment = TextAnchor.MiddleCenter;
                _styleResourcePill.normal.textColor = ColorGoldPrimary;

                _styleStatLabel = new GUIStyle(GUI.skin.label);
                _styleStatLabel.fontSize = 18;
                _styleStatLabel.normal.textColor = ColorTextPrimary;

                _initialized = true;
            }
            catch (System.ArgumentException)
            {
                // In non-OnGUI unit test contexts, GUI.skin is inaccessible.
                _initialized = false;
            }
        }

        // ----------------------------------------------------------------------
        // Matrix & Layout Calculations
        // ----------------------------------------------------------------------
        public static Matrix4x4 PrepareGUIMatrix(out float scale, out float offsetY)
        {
            EnsureInitialized();
            scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;
            offsetY = (Screen.height / scale - 1920f) * 0.5f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));
            return oldMatrix;
        }

        public static void DrawModalWindow(Rect windowRect, string title, string subtitle = null)
        {
            EnsureInitialized();

            // Background panel
            Color oldColor = GUI.color;
            GUI.color = ColorObsidianBg;
            GUI.Box(windowRect, GUIContent.none, _styleWindow);

            // Subtle inner surface
            GUI.color = ColorSurfaceDark;
            GUI.Box(new Rect(windowRect.x + 4, windowRect.y + 4, windowRect.width - 8, windowRect.height - 8), GUIContent.none, _styleWindow);

            // Top & Bottom Gold Accent Lines
            GUI.color = ColorBorderGold;
            GUI.Box(new Rect(windowRect.x, windowRect.y, windowRect.width, 3), GUIContent.none, _styleWindow);
            GUI.Box(new Rect(windowRect.x, windowRect.y + windowRect.height - 3, windowRect.width, 3), GUIContent.none, _styleWindow);

            // 4 Corner Rune Brackets
            float cornerSize = 18f;
            GUI.color = ColorGoldBright;
            // Top Left
            GUI.Box(new Rect(windowRect.x, windowRect.y, cornerSize, 4), GUIContent.none, _styleWindow);
            GUI.Box(new Rect(windowRect.x, windowRect.y, 4, cornerSize), GUIContent.none, _styleWindow);
            // Top Right
            GUI.Box(new Rect(windowRect.x + windowRect.width - cornerSize, windowRect.y, cornerSize, 4), GUIContent.none, _styleWindow);
            GUI.Box(new Rect(windowRect.x + windowRect.width - 4, windowRect.y, 4, cornerSize), GUIContent.none, _styleWindow);
            // Bottom Left
            GUI.Box(new Rect(windowRect.x, windowRect.y + windowRect.height - 4, cornerSize, 4), GUIContent.none, _styleWindow);
            GUI.Box(new Rect(windowRect.x, windowRect.y + windowRect.height - cornerSize, 4, cornerSize), GUIContent.none, _styleWindow);
            // Bottom Right
            GUI.Box(new Rect(windowRect.x + windowRect.width - cornerSize, windowRect.y + windowRect.height - 4, cornerSize, 4), GUIContent.none, _styleWindow);
            GUI.Box(new Rect(windowRect.x + windowRect.width - 4, windowRect.y + windowRect.height - cornerSize, 4, cornerSize), GUIContent.none, _styleWindow);

            GUI.color = oldColor;
        }

        public static void DrawHeader(string title, string subtitle = null)
        {
            EnsureInitialized();
            GUILayout.Label(title, _styleHeaderTitle);
            if (!string.IsNullOrEmpty(subtitle))
            {
                GUILayout.Space(4);
                GUILayout.Label(subtitle, _styleHeaderSubtitle);
            }
        }

        public static bool DrawPrimaryButton(string text, float height = 75f)
        {
            EnsureInitialized();
            return GUILayout.Button(text, _stylePrimaryBtn, GUILayout.Height(height));
        }

        public static bool DrawSecondaryButton(string text, float height = 65f)
        {
            EnsureInitialized();
            return GUILayout.Button(text, _styleSecondaryBtn, GUILayout.Height(height));
        }

        public static bool DrawDangerButton(string text, float height = 65f)
        {
            EnsureInitialized();
            return GUILayout.Button(text, _styleDangerBtn, GUILayout.Height(height));
        }

        public static bool DrawTabButton(string text, bool isSelected, float height = 55f)
        {
            EnsureInitialized();
            if (isSelected)
            {
                return GUILayout.Button(text, _stylePrimaryBtn, GUILayout.Height(height));
            }
            else
            {
                return GUILayout.Button(text, _styleSecondaryBtn, GUILayout.Height(height));
            }
        }

        public static void DrawProgressBar(float current, float max, string label, Color barColor, float height = 28f)
        {
            EnsureInitialized();
            float fillPct = Mathf.Clamp01(max > 0 ? current / max : 0);

            Rect totalRect = GUILayoutUtility.GetRect(100f, 1000f, height, height);
            
            Color oldColor = GUI.color;
            // BG
            GUI.color = new Color(0.04f, 0.06f, 0.09f, 0.95f);
            GUI.DrawTexture(totalRect, _texProgressBarBg);

            // Fill
            if (fillPct > 0)
            {
                Rect fillRect = new Rect(totalRect.x, totalRect.y, totalRect.width * fillPct, totalRect.height);
                GUI.color = barColor;
                GUI.DrawTexture(fillRect, _texProgressBarFillHp);
            }
            GUI.color = oldColor;

            // Overlay Text
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 16;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = ColorTextPrimary;

            GUI.Label(totalRect, label, labelStyle);
        }

        public static void DrawBadge(string text, Color badgeColor)
        {
            EnsureInitialized();
            Color oldColor = GUI.color;
            GUI.color = badgeColor;
            GUILayout.Box(text, _styleBadge, GUILayout.Height(32));
            GUI.color = oldColor;
        }

        public static float GetPulseAlpha(float speed = 4f, float min = 0.70f, float max = 1.00f)
        {
            return Mathf.Lerp(min, max, (Mathf.Sin(Time.time * speed) + 1f) * 0.5f);
        }

        // ----------------------------------------------------------------------
        // Helper Texture Generators
        // ----------------------------------------------------------------------
        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateGradientTexture(Color top, Color bottom, int height)
        {
            Texture2D tex = new Texture2D(1, height);
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color c = Color.Lerp(bottom, top, t);
                tex.SetPixel(0, y, c);
            }
            tex.Apply();
            return tex;
        }

        public static GUIStyle StyleHeaderTitle => _styleHeaderTitle;
        public static GUIStyle StyleHeaderSubtitle => _styleHeaderSubtitle;
        public static GUIStyle StyleSectionTitle => _styleSectionTitle;
        public static GUIStyle StylePrimaryBtn => _stylePrimaryBtn;
        public static GUIStyle StyleSecondaryBtn => _styleSecondaryBtn;
        public static GUIStyle StyleDangerBtn => _styleDangerBtn;
        public static GUIStyle StyleCard => _styleCard;
        public static GUIStyle StyleBadge => _styleBadge;
        public static GUIStyle StyleResourcePill => _styleResourcePill;
        public static GUIStyle StyleStatLabel => _styleStatLabel;
    }
}
