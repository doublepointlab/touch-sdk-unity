/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

using System.Xml;
using UnityEditor;

#if UNITY_ANDROID
using UnityEditor.Android;

/*
 * Inserts elements into the AndroidManifest.xml of the app module, such
 * as permission and activity declarations.
 */
internal class AndroidManifestModifier : IPostGenerateGradleAndroidProject
{
    static readonly string k_AndroidURI = "http://schemas.android.com/apk/res/android";
    static readonly string k_AndroidManifestPath = "/src/main/AndroidManifest.xml";

    void CreateNameValueElementsInTag(XmlDocument doc, string parentPath, string tag,
        string firstName, string firstValue, string secondName = null, string secondValue = null, string thirdName = null, string thirdValue = null)
    {
        var xmlNodeList = doc.SelectNodes(parentPath + "/" + tag);

        // don't create if the firstValue matches
        foreach (XmlNode node in xmlNodeList)
        {
            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (attrib.Value == firstValue)
                {
                    return;
                }
            }
        }

        XmlElement childElement = doc.CreateElement(tag);
        childElement.SetAttribute(firstName, k_AndroidURI, firstValue);

        if (secondValue != null)
        {
            childElement.SetAttribute(secondName, k_AndroidURI, secondValue);
        }

        if (thirdValue != null)
        {
            childElement.SetAttribute(thirdName, k_AndroidURI, thirdValue);
        }

        var xmlParentNode = doc.SelectSingleNode(parentPath);

        if (xmlParentNode != null)
        {
            xmlParentNode.AppendChild(childElement);
        }
    }

    void AppendActivity(XmlDocument doc, string parentPath, string name)
    {
        var xmlNode = doc.SelectNodes(parentPath + "/application").Item(0);
        XmlElement childElement = doc.CreateElement("activity");
        childElement.SetAttribute("name", k_AndroidURI, name);
        childElement.SetAttribute("exported", k_AndroidURI, "true");
        xmlNode.AppendChild(childElement);
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {

        var manifestPath = path + k_AndroidManifestPath;
        var manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        string nodePath = "/manifest";
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-permission", "name", "android.permission.BLUETOOTH");
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-permission", "name", "android.permission.BLUETOOTH_ADMIN");
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-permission", "name", "android.permission.ACCESS_FINE_LOCATION");
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-feature", "name", "android.hardware.bluetooth_le", "required", "false");
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-feature", "name", "android.hardware.top", "required", "false");

        AppendActivity(manifestDoc, nodePath, "io.port6.android.unitywrapper.HelperActivity");

        manifestDoc.Save(manifestPath);
    }

    public int callbackOrder { get { return 20000; } }
}
#endif // UNITY_ANDROID
