using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Combat;
using Lattirune.Modifiers;
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

                                private PlayerCombatant _playerCombatant;
        private RunModifierManager _modifierManager;

        public void Initialize(object a, object b, object c) { }
        public void Initialize(RunManager run, PlayerCombatant player, RunModifierManager mod, ScreenNavigationController nav)
        {
            runManager = run;
            _playerCombatant = player;
            _modifierManager = mod;
            Initialize(nav, run);
        }

        public void Initialize(RunManager run, PlayerCombatant player, RunModifierManager mod = null)
        {
            runManager = run;
            _playerCombatant = player;
            _modifierManager = mod;
            Initialize(FindFirstObjectByType<ScreenNavigationController>(), run);
        }

        public void Initialize(RunManager run, object player, ScreenNavigationController nav = null)
        {
            if (player is PlayerCombatant pc) _playerCombatant = pc;
            Initialize(nav, run);
        }

        public bool ChooseCleanseCurse()
        {
            if (_restActionUsed) return false;
            _restActionUsed = true;
            if (_modifierManager != null)
            {
                _modifierManager.RemoveModifier("mod_curse_vulnerability");
            }
            MeditateForEmbers();
            return true;
        }

        public bool ChooseRestAndHeal()
        {
            if (_restActionUsed) return false;
            _restActionUsed = true;
            if (_playerCombatant != null)
            {
                _playerCombatant.Heal(Mathf.RoundToInt(_playerCombatant.MaxHp * 0.4f));
            }
            RestAndHeal();
            return true;
        }

        public bool ChooseUpgradeRune(string runeId = null)
        {
            if (_restActionUsed) return false;
            _restActionUsed = true;
            ForgeRune();
            return true;
        }

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

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 35f;
            float contentW = screenW - (padX * 2f);

            // =================================================================
            // 1. TOP HEADER & HUD BAR
            // =================================================================
            float topY = 45f;
            float topH = 120f;
            Rect topBarRect = new Rect(padX, topY, contentW, topH);
            LattiruneUITheme.DrawCard(topBarRect);

            Texture2D campIcon = VisualAssetProvider.GetUIIcon("ui_icon_campfire");
            if (campIcon != null)
            {
                GUI.DrawTexture(new Rect(padX + 18f, topY + 18f, 84f, 84f), campIcon, ScaleMode.ScaleToFit);
            }

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 116f, topY + 18f, 400f, 26f), "CAMPFIRE REST SITE", titleStyle);

            GUIStyle statusStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statusStyle.fontSize = 13;
            statusStyle.fontStyle = FontStyle.Italic;
            statusStyle.alignment = TextAnchor.MiddleLeft;
            statusStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(padX + 116f, topY + 48f, contentW - 130f, 50f), _statusMessage, statusStyle);

            // =================================================================
            // 2. REST CHOICES (3 CARDS)
            // =================================================================
            float choicesY = topY + topH + 16f;
            float botBtnH = 85f;
            float botMargin = 25f;
            float actY = virtualH - botBtnH - botMargin;
            float listH = actY - choicesY - 16f;

            Rect listRect = new Rect(padX, choicesY, contentW, listH);
            LattiruneUITheme.DrawCard(listRect);

            float cardY = choicesY + 16f;
            float cardH = (listH - 48f) / 3f;

            // Choice 1: Rest & Heal
            DrawChoiceCard(
                new Rect(padX + 16f, cardY, contentW - 32f, cardH),
                "ui_icon_heal",
                "REST & TEND WOUNDS",
                "Heal 40% of your maximum health to survive deeper floors.",
                "REST (+40% HP)",
                new Color(0.3f, 0.9f, 0.4f),
                RestAndHeal
            );

            cardY += cardH + 8f;

            // Choice 2: Temper Runes
            DrawChoiceCard(
                new Rect(padX + 16f, cardY, contentW - 32f, cardH),
                "ui_icon_upgrade",
                "TEMPER RUNES",
                "Infuse your runes in the flame to permanently boost elemental damage.",
                "TEMPER (+20% DMG)",
                LattiruneUITheme.ColorCyanArcane,
                ForgeRune
            );

            cardY += cardH + 8f;

            // Choice 3: Meditate for Embers
            DrawChoiceCard(
                new Rect(padX + 16f, cardY, contentW - 32f, cardH),
                "ui_icon_embers",
                "MEDITATE FOR EMBERS",
                "Attune to the primordial flame and harvest +50 Persistent Embers.",
                "HARVEST (+50 EMBERS)",
                new Color(1f, 0.6f, 0.2f),
                MeditateForEmbers
            );

            // =================================================================
            // 3. BOTTOM ACTION BUTTON
            // =================================================================
            Rect actRect = new Rect(padX, actY, contentW, botBtnH);
            if (GUI.Button(actRect, "LEAVE REST SITE & RESUME DESCENT", LattiruneUITheme.StylePrimaryBtn))
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                navigation?.NavigateTo(ScreenState.DUNGEON_MAP);
            }

            GUI.matrix = oldMatrix;
        }

        private void DrawChoiceCard(Rect rect, string iconId, string title, string desc, string btnText, Color accentColor, Action onChoose)
        {
            Color cardBg = _restActionUsed ? new Color(0.06f, 0.08f, 0.10f, 0.70f) : new Color(0.12f, 0.16f, 0.24f, 0.90f);
            GUI.color = cardBg;
            LattiruneUITheme.DrawCard(rect);
            GUI.color = Color.white;

            if (!_restActionUsed)
            {
                LattiruneUITheme.DrawBorder(rect, 1.5f, accentColor);
            }

            Texture2D icon = VisualAssetProvider.GetUIIcon(iconId);
            if (icon != null)
            {
                Rect iconRect = new Rect(rect.x + 16f, rect.y + (rect.height - 72f) * 0.5f, 72f, 72f);
                Color oldC = GUI.color;
                if (_restActionUsed) GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }

            float textX = rect.x + 104f;
            float textW = rect.width - 320f;

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = _restActionUsed ? LattiruneUITheme.ColorTextMuted : Color.white;
            GUI.Label(new Rect(textX, rect.y + 18f, textW, 24f), title, titleStyle);

            GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            descStyle.fontSize = 13;
            descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(textX, rect.y + 46f, textW, 40f), desc, descStyle);

            float btnW = 190f;
            float btnX = rect.x + rect.width - btnW - 16f;
            float btnY = rect.y + (rect.height - 55f) * 0.5f;
            Rect btnRect = new Rect(btnX, btnY, btnW, 55f);

            if (_restActionUsed)
            {
                GUI.DrawTexture(btnRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                LattiruneUITheme.DrawBorder(btnRect, 1f, LattiruneUITheme.ColorTextMuted);
                GUIStyle pStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                pStyle.alignment = TextAnchor.MiddleCenter;
                pStyle.fontSize = 14;
                pStyle.fontStyle = FontStyle.Bold;
                pStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUI.Label(btnRect, "USED", pStyle);
            }
            else
            {
                if (GUI.Button(btnRect, btnText, LattiruneUITheme.StylePrimaryBtn))
                {
                    onChoose?.Invoke();
                }
            }
        }
    }
}
