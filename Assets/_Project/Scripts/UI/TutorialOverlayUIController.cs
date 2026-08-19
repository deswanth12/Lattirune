using System;
using UnityEngine;
using Lattirune.Tutorial;

namespace Lattirune.UI
{
    /// <summary>
    /// Lightweight non-blocking floating banner overlay presenting contextual onboarding steps.
    /// Strictly adheres to PLAN.md Section 15 and Section 34.
    /// </summary>
    public class TutorialOverlayUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TutorialManager tutorialManager;

        public void Initialize(TutorialManager tutorial = null)
        {
            tutorialManager = tutorial;
        }

        private void OnGUI()
        {
            if (tutorialManager == null || tutorialManager.IsTutorialCompleted) return;

            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float bannerWidth = 1000f;
            float bannerHeight = 160f;
            float posX = (1080f - bannerWidth) * 0.5f;
            float posY = 1700f; // Floating at bottom thumb zone

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.1f, 0.12f, 0.18f, 0.95f);
            GUI.Box(new Rect(posX, posY, bannerWidth, bannerHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 20, posY + 15, bannerWidth - 40, bannerHeight - 30));

            GUILayout.BeginHorizontal();

            // Hint Text
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 20;
            hintStyle.fontStyle = FontStyle.Bold;
            hintStyle.wordWrap = true;
            hintStyle.normal.textColor = Color.yellow;

            GUILayout.Label(tutorialManager.GetCurrentStepHint(), hintStyle, GUILayout.Width(680), GUILayout.Height(120));

            GUILayout.Space(15);

            GUILayout.BeginVertical();

            // Advance Step Button
            if (GUILayout.Button("GOT IT! (NEXT)", GUILayout.Height(55), GUILayout.Width(220)))
            {
                tutorialManager.AdvanceStep(tutorialManager.CurrentStep);
            }

            GUILayout.Space(6);

            // Skip Tutorial Button
            GUIStyle skipStyle = new GUIStyle(GUI.skin.button);
            skipStyle.fontSize = 14;
            if (GUILayout.Button("Skip Tutorial", skipStyle, GUILayout.Height(40), GUILayout.Width(220)))
            {
                tutorialManager.SkipTutorial();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
