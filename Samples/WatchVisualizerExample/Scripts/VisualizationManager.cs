using UnityEngine;
using TMPro;
namespace Psix.Examples
{

    public class VisualizationManager : MonoBehaviour
    {
        public VisualizerElement[] elements;
        public TextMeshPro connectionText;

        void Start()
        {
            if (Watch.Instance == null){
                Debug.Log("VisualizationManager: Null watch");
                return;
            }
            foreach (var elem in elements){
                elem.RegisterWatch(Watch.Instance);
                Watch.Instance.OnDetectedGesturesChange += elem.RegisterGestures;
            }

            Watch.Instance.OnConnect += () => { connectionText.text = "Connected"; };
            Watch.Instance.OnDisconnect += () => { connectionText.text = "Disconnected"; };
        }
    }
}
