using TMPro;
using UnityEngine;

namespace Psix.Examples
{
    public class WatchInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshPro watchInfoText;

        private void OnEnable()
        {
            Watch.Instance.OnConnect += UpdateInfo;
        }

        private void OnDisable()
        {
            Watch.Instance.OnConnect -= UpdateInfo;
        }

        private void UpdateInfo()
        {
            var info = "";
            info += "Manufacturer: " + Watch.Instance.Manufacturer + "\n";
            info += "Device Name: " + Watch.Instance.DeviceName + "\n";
            info += "App Id: " + Watch.Instance.AppId + "\n";
            info += "App Version: " + Watch.Instance.AppVersion + "\n";
            info += "Model Info: " + Watch.Instance.ModelInfo + "\n";

            watchInfoText.text = info;
        }
    }
}