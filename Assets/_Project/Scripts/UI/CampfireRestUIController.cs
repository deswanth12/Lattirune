using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Modifiers;
using Lattirune.Runes;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for the Floor 8 Campfire Rest Site.
    /// Features warm burning campfire artwork, restorative options, and rune upgrades (0 emoji, 0 placeholders).
    /// </summary>
    public class CampfireRestUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private DungeonMapScreenController mapController;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private string _feedbackMessage = "The warmth of the campfire soothes your weary soul. Choose how to spend your rest.";
        private bool _hasChosenOption = false;

        public bool IsVisible => isVisible;
        public bool HasChosenOption => _hasChosenOption;

        public void BindMapController(DungeonMapScreenController map)
        {
            mapController = map;
        }

        public void Initialize(
            RunManager run,
            PlayerCombatant player,
            RunModifierManager modifiers = null,
            ScreenNavigationController nav = null)
        {
            runManager = run;
            playerCombatant = player;
            modifierManager = modifiers;
            navigation = nav;
            _hasChosenOption = false;
            _feedbackMessage = "The warmth of the campfire soothes your weary soul. Choose how to spend your rest.";

            if (navigation != null)
            {
                navigation.OnScreenChanged += HandleScreenChanged;
            }
        }

        private void OnDestroy()
        {
            if (navigation != null)
            {
                navigation.OnScreenChanged -= HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(ScreenState prev, ScreenState next)
        {
            if (next == ScreenState.CAMPFIRE_REST)
            {
                Show();
            }
            else if (prev == ScreenState.CAMPFIRE_REST)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _hasChosenOption = false;
            _feedbackMessage = "The warmth of the campfire soothes your weary soul. Choose how to spend your rest.";
        }

        public void Hide()
        {
            isVisible = false;
        }

        public bool ChooseRestAndHeal()
        {
            if (_hasChosenOption || playerCombatant == null) return false;

            int healAmount = Mathf.Max(1, Mathf.RoundToInt(playerCombatant.MaxHp * 0.40f));
            playerCombatant.Heal(healAmount);
            _hasChosenOption = true;
            _feedbackMessage = $"You rested quietly by the flames, restoring {healAmount} Health points.";
            return true;
        }

        public bool ChooseUpgradeRune(string runeId = "fire_rune_01")
        {
            if (_hasChosenOption || runManager == null) return false;

            runManager.UpgradeRune(runeId, 3);
            _hasChosenOption = true;
            _feedbackMessage = $"You attuned your rune in the embers! +3 Elemental Power granted for the remainder of this run.";
            return true;
        }

        public bool ChooseCleanseCurse()
        {
            if (_hasChosenOption || modifierManager == null) return false;

            if (modifierManager.HasModifier("mod_curse_vulnerability"))
            {
                modifierManager.RemoveModifier("mod_curse_vulnerability");
                _hasChosenOption = true;
                _feedbackMessage = "The sacred flame dispelled the Curse of Vulnerability!";
                return true;
            }
            else
            {
                if (playerCombatant != null)
                {
                    playerCombatant.Heal(15);
                }
                _hasChosenOption = true;
                _feedbackMessage = "With no active curses to cleanse, the sacred light granted you a minor blessing (+15 HP).";
                return true;
            }
        }

                private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.CAMPFIRE_REST) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "CAMPFIRE REST SITE");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("CAMPFIRE REST SITE", "The soothing flames offer sanctuary before the trials ahead.");
            GUILayout.Space(10);

            // Campfire Backdrop Art
            Texture2D campBg = VisualAssetProvider.GetBackdrop("bg_campfire_hub");
            if (campBg != null)
            {
                Rect bgRect = GUILayoutUtility.GetRect(panelWidth - 80, 220f);
                GUI.DrawTexture(bgRect, campBg, ScaleMode.ScaleAndCrop);
                GUILayout.Space(12);
            }

            int curHp = playerCombatant != null ? playerCombatant.CurrentHp : 100;
            int maxHp = playerCombatant != null ? playerCombatant.MaxHp : 100;
            LattiruneUITheme.DrawBadge($"CHAMPION HEALTH: {curHp} / {maxHp} HP", LattiruneUITheme.ColorGreenHealth);
            GUILayout.Space(12);

            GUIStyle msgStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            msgStyle.fontSize = 17;
            msgStyle.fontStyle = FontStyle.Italic;
            msgStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(_feedbackMessage, msgStyle);
            GUILayout.Space(16);

            // Option 1: Rest & Heal
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            Texture2D healIcon = VisualAssetProvider.GetUIIcon("ui_icon_heal");
            if (healIcon != null)
            {
                Rect hRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
                GUI.DrawTexture(hRect, healIcon, ScaleMode.ScaleToFit);
                GUILayout.Space(10);
            }
            GUILayout.BeginVertical();
            GUIStyle optStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            optStyle.fontSize = 18;
            optStyle.fontStyle = FontStyle.Bold;
            optStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUILayout.Label("REST & HEAL (+40% HP)", optStyle);
            GUILayout.Label("Bandage wounds and drink fresh water from the subterranean spring.", LattiruneUITheme.StyleStatLabel);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUI.enabled = !_hasChosenOption;
            if (LattiruneUITheme.DrawPrimaryButton("REST", 55f))
            {
                ChooseRestAndHeal();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Option 2: Upgrade Rune
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            Texture2D upIcon = VisualAssetProvider.GetUIIcon("ui_icon_upgrade");
            if (upIcon != null)
            {
                Rect uRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
                GUI.DrawTexture(uRect, upIcon, ScaleMode.ScaleToFit);
                GUILayout.Space(10);
            }
            GUILayout.BeginVertical();
            GUILayout.Label("FORGE ATTUNEMENT (+3 RUNE POWER)", optStyle);
            GUILayout.Label("Heat your primary rune in sacred embers to permanently increase its damage.", LattiruneUITheme.StyleStatLabel);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUI.enabled = !_hasChosenOption;
            if (LattiruneUITheme.DrawPrimaryButton("FORGE", 55f))
            {
                ChooseUpgradeRune();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Option 3: Cleanse Curse
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.BeginHorizontal();
            Texture2D evIcon = VisualAssetProvider.GetUIIcon("ui_icon_event");
            if (evIcon != null)
            {
                Rect eRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
                GUI.DrawTexture(eRect, evIcon, ScaleMode.ScaleToFit);
                GUILayout.Space(10);
            }
            GUILayout.BeginVertical();
            GUILayout.Label("SANCTIFY SOUL (CLEANSE CURSE)", optStyle);
            GUILayout.Label("Offer a silent prayer to purge dark afflictions or receive a vital blessing.", LattiruneUITheme.StyleStatLabel);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUI.enabled = !_hasChosenOption;
            if (LattiruneUITheme.DrawPrimaryButton("PRAY", 55f))
            {
                ChooseCleanseCurse();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Leave Rest Site
            if (LattiruneUITheme.DrawSecondaryButton("LEAVE CAMPFIRE & CONTINUE", 75f))
            {
                if (mapController != null && mapController.MapGraph != null)
                {
                    mapController.MapGraph.CompleteCurrentNode();
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
