using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.Oculus;

#if UNITY_ANDROID
internal class OculusManifestBTFixer : IPostGenerateGradleAndroidProject
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

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        if (!OculusBuildTools.OculusLoaderPresentInSettingsForBuildTarget(BuildTargetGroup.Android))
            return;

        var manifestPath = path + k_AndroidManifestPath;
        var manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        string nodePath = "/manifest";
        CreateNameValueElementsInTag(manifestDoc, nodePath, "uses-permission", "name", "android.permission.BLUETOOTH");

        manifestDoc.Save(manifestPath);
    }

    public int callbackOrder { get { return 20000; } }
}
#endif
