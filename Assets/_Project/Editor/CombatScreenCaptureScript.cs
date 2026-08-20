using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Lattirune.UI;
using Lattirune.Core;
using Lattirune.Combat;
using Lattirune.Dungeon;

namespace Lattirune.Editor
{
    public static class CombatScreenCaptureScript
    {
        [MenuItem("Lattirune/Capture Combat Screen")]
        public static void CaptureCombatScreenVisuals()
        {
            Debug.Log("[CaptureScript] Starting High-Res Combat Screen Capture in Batchmode...");

            // 1. Open Master Scene
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Bootstrap.unity");

            // 2. Setup Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.4f;
            cam.transform.position = new Vector3(0f, 0.5f, -10f);
            cam.backgroundColor = new Color(0.031f, 0.043f, 0.071f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            // 3. Initialize Bootstrap
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<GridInteractionBootstrap>();
            if (bootstrap == null)
            {
                GameObject bootObj = new GameObject("GridInteractionBootstrap");
                bootstrap = bootObj.AddComponent<GridInteractionBootstrap>();
            }

            bootstrap.InitializePrototype();

            // Set screen state to COMBAT
            var nav = bootstrap.Navigation;
            if (nav != null)
            {
                nav.NavigateTo(ScreenState.COMBAT);
            }

            var combatUI = UnityEngine.Object.FindFirstObjectByType<CombatEncounterUI>();
            if (combatUI != null)
            {
                combatUI.SetupEncounter(1, "Sewer Rat", 35, 3, 0, false, 1);
            }

            // 4. Render 1080x2412 Frame
            int width = 1080;
            int height = 2412;

            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            Texture2D screenTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            cam.Render();
            RenderTexture.active = rt;
            screenTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenTex.Apply();

            byte[] pngData = screenTex.EncodeToPNG();
            string outDir = @"C:\Users\k deswanth\.gemini\antigravity\brain\f8083bc6-9542-40ea-b403-f9c5e8c5bd23";
            string combatPath = Path.Combine(outDir, "redesigned_combat_stage_hires.png");
            File.WriteAllBytes(combatPath, pngData);
            Debug.Log($"[CaptureScript] Saved Redesigned Combat Stage to: {combatPath} ({pngData.Length} bytes)");

            // Cleanup
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(screenTex);

            Debug.Log("[CaptureScript] Capture Complete!");
        }
    }
}
