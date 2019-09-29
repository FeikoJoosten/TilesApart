using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class PlatformSwitcher {
    [MenuItem("Platform/PC, Mac and Linux Standalone")]
    static void SwitchPlatformToDesktop() {
        SwitchTo(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
    }

    [MenuItem("Platform/PC, Mac and Linux Standalone", true)]
    static bool SwitchPlatformToDesktopValidate() {
        return EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX;
    }

    [MenuItem("Platform/iOS")]
    static void SwitchPlatformToIOS() {
        SwitchTo(BuildTargetGroup.iOS, BuildTarget.iOS);
    }

    [MenuItem("Platform/iOS", true)]
    static bool SwitchPlatformToIOSValidate() {
        return EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS;
    }

    [MenuItem("Platform/Android")]
    static void SwitchPlatformToAndroid() {
        SwitchTo(BuildTargetGroup.Android, BuildTarget.Android);
    }

    [MenuItem("Platform/Android", true)]
    static bool SwitchPlatformToAndroidValidate() {
        return EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android;
    }

    public static void SwitchTo(BuildTargetGroup targetGroup, BuildTarget targetPlatform) {
        var currentPlatform = EditorUserBuildSettings.activeBuildTarget;
        if (currentPlatform == targetPlatform)
            return;

        // Don't switch when compiling
        if (EditorApplication.isCompiling) {
            Debug.LogWarning("Could not switch platform because unity is compiling");
            return;
        }

        // Don't switch while playing
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogWarning("Could not switch platform because unity is in playMode");
            return;
        }

        Debug.Log("Switching platform from " + currentPlatform + " to " + targetPlatform);

        string libDir = "Library";
        string libDirCurrent = libDir + "_" + currentPlatform;
        string libDirTarget = libDir + "_" + targetPlatform;

        // If target dir doesn't exist yet, make a copy of the current one
        if (!Directory.Exists(libDirTarget)) {
            Debug.Log("Making a copy of " + libDir + " because " + libDirTarget + " doesn't exist yet");
            CopyFilesRecursively(new DirectoryInfo(libDir), new DirectoryInfo(libDirTarget));
        }

        // Safety check, libDirCurrent shouldn't exist (current data is stored in libDir)
        if (Directory.Exists(libDirCurrent))
            Directory.Delete(libDirCurrent, true);

        // Rename dirs
        Directory.Move(libDir, libDirCurrent);
        Directory.Move(libDirTarget, libDir);

        EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, targetPlatform);
    }


    public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));

        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name));
    }
}