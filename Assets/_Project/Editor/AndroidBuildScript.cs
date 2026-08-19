#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lattirune.Editor
{
    /// <summary>
    /// Automated Android Build script for Phase 1, Phase 2, and Phase 3 MVP Release Candidate.
    /// Builds development and release candidate APKs targeting 1080x1920 portrait orientation.
    /// </summary>
    public static class AndroidBuildScript
    {
        public const string BUILD_OUTPUT_DIR = "Builds/Android";
        public const string APK_NAME_PHASE1 = "Lattirune-Phase1-Dev.apk";
        public const string APK_NAME_PHASE2 = "Lattirune-Phase2-Verification.apk";
        public const string APK_NAME_MVP1 = "Lattirune-MVP1-Verification.apk";
        public const string APK_NAME_RELEASE_CANDIDATE = "Lattirune-MVP1-ReleaseCandidate.apk";
        public const string PACKAGE_ID = "com.developer.lattirune";
        public const string BOOTSTRAP_SCENE_PATH = "Assets/_Project/Scenes/Bootstrap.unity";

        [MenuItem("Lattirune/Build/Build Android MVP 1.0 Release Candidate APK")]
        public static bool BuildAndroidMvp1ReleaseCandidateApk()
        {
            return ExecuteAndroidBuild(APK_NAME_RELEASE_CANDIDATE);
        }

        [MenuItem("Lattirune/Build/Build Android MVP 1.0 Verification APK")]
        public static bool BuildAndroidMvp1VerificationApk()
        {
            return ExecuteAndroidBuild(APK_NAME_MVP1);
        }

        [MenuItem("Lattirune/Build/Build Android Phase 2 Verification APK")]
        public static bool BuildAndroidPhase2VerificationApk()
        {
            return ExecuteAndroidBuild(APK_NAME_PHASE2);
        }

        [MenuItem("Lattirune/Build/Build Android Development APK")]
        public static bool BuildAndroidDevelopmentApk()
        {
            return ExecuteAndroidBuild(APK_NAME_PHASE1);
        }

        private static bool ExecuteAndroidBuild(string apkFileName)
        {
            Debug.Log($"[Lattirune.Build] Starting Android Build ({apkFileName})...");

            // 1. Configure Android Player Settings
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PACKAGE_ID);
            PlayerSettings.productName = "Lattirune";
            PlayerSettings.companyName = "Developer";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 2. Ensure Output Directory Exists
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, BUILD_OUTPUT_DIR);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string apkPath = Path.Combine(outputDirectory, apkFileName);

            // 3. Build Player Options
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new string[] { BOOTSTRAP_SCENE_PATH },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            // 4. Execute Build
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Lattirune.Build] Android Build SUCCEEDED: {apkPath} ({summary.totalSize / 1024 / 1024} MB, Duration: {summary.totalTime.TotalSeconds:F1}s)");
                return true;
            }
            else
            {
                Debug.LogError($"[Lattirune.Build] Android Build FAILED with result: {summary.result}");
                return false;
            }
        }
    }
}
#endif
