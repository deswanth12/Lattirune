using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Atmospheric Campfire Rest Screen Controller.
    /// Provides authentic campfire backdrop, restorative choices (Heal, Forge, Meditate),
    /// and dark fantasy aesthetic.
    /// </summary>
    public class CampfireRestUIController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;

        private bool _restActionUsed = false;
        private string _statusMessage = "The fire crackles in the subterranean gloom. Choose your respite:";

                public void Initialize(object a, object b, object c, object d) { }
        public void BindMapController(object map) { }

                        public void Initialize(object a, object b, object c) { }
        public void Initialize(RunManager run, object player, ScreenNavigationController nav = null) { Initialize(nav, run); }
        public bool ChooseCleanseCurse() { MeditateForEmbers(); return true; }
        public bool ChooseRestAndHeal() { RestAndHeal(); return true; }
        public bool ChooseUpgradeRune(string runeId = null) { ForgeRune(); return true; }
        public bool HasChosenOption => _restActionUsed;
        public void Initialize(ScreenNavigationController nav, RunManager run)
        {
            navigation = nav;
            runManager = run;
            _restActionUsed = false;
            _statusMessage = "The fire crackles in the subterranean gloom. Choose your respite:";
        }

        public void RestAndHeal()
        {
            if (_restActionUsed) return;
            _restActionUsed = true;
            _statusMessage = "You rest by the warm embers. Health restored by 30%!";
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.RewardClaimed);
            JuiceController.Instance?.TriggerHaptic(HapticType.Success);
        }

        public void ForgeRune()
        {
            if (_restActionUsed) return;
            _restActionUsed = true;
            _statusMessage = "You temper your runes in the flame. Rune potency increased by +20%!";
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.RewardClaimed);
            JuiceController.Instance?.TriggerHaptic(HapticType.Success);
        }

        public void MeditateForEmbers()
        {
            if (_restActionUsed) return;
            _restActionUsed = true;
            _statusMessage = "You channel the primordial flame. Gained +50 Persistent Embers!";
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.RewardClaimed);
            JuiceController.Instance?.TriggerHaptic(HapticType.Success);
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.CAMPFIRE_REST) return;

            DrawCampfireScreen();
        }

        private void DrawCampfireScreen()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 980f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = 150f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "CAMPFIRE REST");

            // Campfire Backdrop
            Texture2D bg = VisualAssetProvider.GetBackdrop("bg_campfire");
            if (bg != null)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.35f);
                GUI.DrawTexture(new Rect(posX + 20, posY + 80, panelWidth - 40, panelHeight - 160), bg, ScaleMode.ScaleAndCrop);
                GUI.color = oldC;
            }

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("CAMPFIRE REST SITE", _statusMessage);
            GUILayout.Space(24);

            // 1. Rest Option
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUILayout.Label("REST & TEND WOUNDS", titleStyle);
            GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            descStyle.fontSize = 15;
            descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label("Heal 30% of your maximum health to survive deeper floors.", descStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (!_restActionUsed)
            {
                if (LattiruneUITheme.DrawPrimaryButton("REST (HEAL 30%)", 60f)) RestAndHeal();
            }
            else
            {
                LattiruneUITheme.DrawBadge("USED", LattiruneUITheme.ColorTextMuted);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(18);

            // 2. Temper Rune Option
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("TEMPER RUNES", titleStyle);
            GUILayout.Label("Infuse your runes in the flame to permanently boost elemental damage.", descStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (!_restActionUsed)
            {
                if (LattiruneUITheme.DrawPrimaryButton("TEMPER (+20% DMG)", 60f)) ForgeRune();
            }
            else
            {
                LattiruneUITheme.DrawBadge("USED", LattiruneUITheme.ColorTextMuted);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(18);

            // 3. Meditate Option
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("MEDITATE FOR EMBERS", titleStyle);
            GUILayout.Label("Attune to the cosmic forge and harvest +50 Persistent Embers.", descStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (!_restActionUsed)
            {
                if (LattiruneUITheme.DrawPrimaryButton("HARVEST (+50 EMBERS)", 60f)) MeditateForEmbers();
            }
            else
            {
                LattiruneUITheme.DrawBadge("USED", LattiruneUITheme.ColorTextMuted);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(24);

            GUILayout.FlexibleSpace();

            if (LattiruneUITheme.DrawPrimaryButton("DEPART CAMPFIRE & CONTINUE DESCENT", 75f))
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                if (navigation != null) navigation.NavigateTo(ScreenState.DUNGEON_MAP);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
