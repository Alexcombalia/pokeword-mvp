using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build de Android desde línea de comandos (Unity -batchmode -executeMethod BuildAndroid.Build).
// Usa las escenas y ajustes de Build Settings, igual que el botón Build del editor.
public static class BuildAndroid
{
    public static void Build()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .Distinct()
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "APK-Build/LinguaNoir.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[BuildAndroid] {report.summary.result} — {report.summary.totalSize} bytes, {report.summary.totalErrors} errores");
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
