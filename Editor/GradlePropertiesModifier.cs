// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using System.IO;
using UnityEditor;

#if UNITY_ANDROID
using UnityEditor.Android;

/**
 * Inserts some properties into the gradle.properties of the gradle project.
 */
internal class GradlePropertiesModifier : IPostGenerateGradleAndroidProject
{

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var filepath = Path.Combine(path, "../gradle.properties");

        var newProperties = "android.useAndroidX=true\nandroid.enableJetifier=true\n";

        if (File.Exists(filepath)) {
            string existing = File.ReadAllText(filepath);
            newProperties = string.Concat(newProperties, existing);
        }

        System.IO.File.WriteAllText(filepath, newProperties);

    }

    public int callbackOrder { get { return 20002; } }
}
#endif // UNITY_ANDROID
