#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lattirune.Editor
{
    /// <summary>
    /// Automated Android Build script for Phase 1, Phase 2, and Phase 3 MVP Release.
    /// Builds development, release candidate, and production versioned APKs and AABs targeting 1080x1920 portrait orientation.
    /// </summary>
    public static class AndroidBuildScript
    {
        public const string BUILD_OUTPUT_DIR = "Builds/Android";
        public const string APK_NAME_PHASE1 = "Lattirune-Phase1-Dev.apk";
        public const string APK_NAME_PHASE2 = "Lattirune-Phase2-Verification.apk";
        public const string APK_NAME_MVP1 = "Lattirune-MVP1-Verification.apk";
        public const string APK_NAME_RELEASE_CANDIDATE = "Lattirune-MVP1-ReleaseCandidate.apk";
        public const string APK_NAME_V100 = "Lattirune-1.0.0.apk";
        public const string AAB_NAME_V100 = "Lattirune-1.0.0.aab";
        public const string PACKAGE_ID = "com.developer.lattirune";
        public const string BOOTSTRAP_SCENE_PATH = "Assets/_Project/Scenes/Bootstrap.unity";

        [MenuItem("Lattirune/Build/Build Android 1.0.0 Production Release AAB (App Bundle)")]
        public static bool BuildProductionAAB()
        {
            return ExecuteAndroidBuild(AAB_NAME_V100, isAppBundle: true, isRelease: true);
        }

        public static bool BuildAndroidV100ReleaseAab()
        {
            return BuildProductionAAB();
        }

        [MenuItem("Lattirune/Build/Build Android 1.0.0 Production Release APK")]
        public static bool BuildAndroidV100ReleaseApk()
        {
            return ExecuteAndroidBuild(APK_NAME_V100, isAppBundle: false, isRelease: true);
        }

        [MenuItem("Lattirune/Build/Build Android MVP 1.0 Release Candidate APK")]
        public static bool BuildAndroidMvp1ReleaseCandidateApk()
        {
            return ExecuteAndroidBuild(APK_NAME_RELEASE_CANDIDATE, isAppBundle: false, isRelease: true);
        }

        [MenuItem("Lattirune/Build/Build Android MVP 1.0 Verification APK")]
        public static bool BuildAndroidMvp1VerificationApk()
        {
            return ExecuteAndroidBuild(APK_NAME_MVP1, isAppBundle: false, isRelease: false);
        }

        [MenuItem("Lattirune/Build/Build Android Phase 2 Verification APK")]
        public static bool BuildAndroidPhase2VerificationApk()
        {
            return ExecuteAndroidBuild(APK_NAME_PHASE2, isAppBundle: false, isRelease: false);
        }

        [MenuItem("Lattirune/Build/Build Android Development APK")]
        public static bool BuildAndroidDevelopmentApk()
        {
            return ExecuteAndroidBuild(APK_NAME_PHASE1, isAppBundle: false, isRelease: false);
        }

        private static bool ExecuteAndroidBuild(string outputFileName, bool isAppBundle = false, bool isRelease = false)
        {
            Debug.Log($"[Lattirune.Build] Starting Android Build ({outputFileName}, AppBundle: {isAppBundle}, Release: {isRelease})...");

            // 1. Configure Android Player Settings
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, PACKAGE_ID);
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.productName = "Lattirune";
            PlayerSettings.companyName = "Developer";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 2. Configure App Bundle Setting
            EditorUserBuildSettings.buildAppBundle = isAppBundle;

            // 3. Ensure Output Directory Exists
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, BUILD_OUTPUT_DIR);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string outputPath = Path.Combine(outputDirectory, outputFileName);

            // 4. Build Player Options
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new string[] { BOOTSTRAP_SCENE_PATH },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = isRelease ? BuildOptions.None : (BuildOptions.Development | BuildOptions.AllowDebugging)
            };

            // 5. Execute Build
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Lattirune.Build] Android Build SUCCEEDED: {outputPath} ({summary.totalSize / 1024 / 1024} MB, Duration: {summary.totalTime.TotalSeconds:F1}s)");
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
