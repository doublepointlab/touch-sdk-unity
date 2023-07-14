// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using System.IO;
using UnityEditor;

#if UNITY_ANDROID
using UnityEditor.Android;

/**
 * Inserts dependencies into build.gradle.
 */
internal class BuildGradleModifier : IPostGenerateGradleAndroidProject
{

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var filepath = Path.Combine(path, "build.gradle");

        if (!File.Exists(filepath))
            return;

        string gradle = File.ReadAllText(filepath);
        int insertionIndex = gradle.IndexOf("implementation fileTree");

        if (insertionIndex < 0)
            return;

        gradle = gradle.Insert(insertionIndex, "implementation 'androidx.activity:activity-ktx:1.3.0'\n    ");
        gradle = gradle.Insert(insertionIndex, "implementation 'com.google.protobuf:protobuf-lite:3.0.0'\n    ");
        System.IO.File.WriteAllText(filepath, gradle);

    }

    public int callbackOrder { get { return 20001; } }
}
#endif // UNITY_ANDROID
