using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    using Interaction;
    public class GestureVisualizer : MonoBehaviour
    {
        [SerializeField] private Gesture gesture;
        [SerializeField] private TextMeshPro countText;
        [SerializeField] private ButtonFlasher flasher;
        private int gestureCount;

        private void OnEnable()
        {
            Watch.Instance.OnGesture += OnGesture;
        }
        
        private void OnDisable()
        {
            Watch.Instance.OnGesture -= OnGesture;
        }

        private void OnGesture(Gesture newGesture)
        {
            if (newGesture != gesture) return;
            gestureCount++;
            countText.text = gestureCount.ToString();
            flasher.FlashButton();
        }
    }
}
