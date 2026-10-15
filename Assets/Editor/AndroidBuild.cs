#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EggMerge.EditorTools
{
    public static class AndroidBuild
    {
        const string Scene = "Assets/Scenes/Main.unity";
        const string OutApk = "Builds/Android/EggMergeRush.apk";

        [MenuItem("EggMerge/Build Android APK")]
        public static void BuildApk()
        {
            Directory.CreateDirectory("Builds/Android");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.romangames.eggmergerush");
            PlayerSettings.productName = "Egg Merge Rush";
            PlayerSettings.bundleVersion = EggMerge.Core.UpdateConfig.AppVersion;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { Scene },
                locationPathName = OutApk,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"APK ready: {Path.GetFullPath(OutApk)} ({report.summary.totalSize} bytes)");
            else
                Debug.LogError($"APK build failed: {report.summary.result}");
        }
    }
}
#endif
