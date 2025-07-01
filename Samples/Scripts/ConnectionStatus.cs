using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class WatchConnectedExample : MonoBehaviour
    {
        [SerializeField] private Color connectedColor, disconnectedColor;
        [SerializeField] private Image bluetoothIcon;
        [SerializeField] private GameObject connectWatchPrompt;

        private void OnEnable()
        {
            Watch.Instance.OnConnect += OnWatchConnected;
            Watch.Instance.OnDisconnect += OnWatchDisconnected;

            if (Watch.Instance.Connected)
                OnWatchConnected();
            else
                OnWatchDisconnected();
        }

        private void OnDisable()
        {
            Watch.Instance.OnConnect -= OnWatchConnected;
            Watch.Instance.OnDisconnect -= OnWatchDisconnected;
        }

        private void OnWatchConnected()
        {
            connectWatchPrompt.SetActive(false);
            bluetoothIcon.color = connectedColor;
        }

        private void OnWatchDisconnected()
        {
            connectWatchPrompt.SetActive(true);
            bluetoothIcon.color = disconnectedColor;
        }
    }
}