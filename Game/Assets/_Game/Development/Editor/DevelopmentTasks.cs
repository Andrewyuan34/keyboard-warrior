using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KeyboardWarrior.Development
{
    public static class DevelopmentTasks
    {
        [MenuItem("Keyboard Warrior/Configure development project")]
        public static void Configure()
        {
            Validation.ValidationProject.CreateScene();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            var code = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Microsoft VS Code", "Code.exe");
            if (File.Exists(code))
                Unity.CodeEditor.CodeEditor.SetExternalScriptEditor(code);
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
            AssetDatabase.SaveAssets();
            Debug.Log("DEVELOPMENT_SETUP_OK Unity=" + Application.unityVersion);
        }

        [MenuItem("Keyboard Warrior/Build Windows validation")]
        public static void BuildWindows()
        {
            var output = Environment.GetEnvironmentVariable("KW_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(output))
                output = Path.GetFullPath(Path.Combine(Application.dataPath,
                    "../../Builds/Windows/Validation/KeyboardWarrior.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Validation.ValidationProject.ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("WINDOWS_BUILD_OK " + output + " bytes=" + report.summary.totalSize);
        }
    }
}
