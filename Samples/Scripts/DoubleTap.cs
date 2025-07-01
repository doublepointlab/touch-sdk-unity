using System;
using TMPro;
using UnityEngine;

namespace Psix.Examples
{
    using Interaction;
    
    public class DoubleTap : MonoBehaviour
    {
        [SerializeField] private ButtonFlasher flasher;
        [SerializeField] private TextMeshPro countText;
        private DateTime lastTapTime;
        private const float DoubleTapTime = 0.5f;
        private int gestureCount;
        private void OnEnable()
        {
            Watch.Instance.OnGesture += OnGesture;
        }
    
        private void OnDisable()
        {
            Watch.Instance.OnGesture -= OnGesture;
        }
    
        private void OnGesture(Gesture gesture)
        {
            if (gesture != Gesture.PinchRelease)
                return;
            if((DateTime.Now - lastTapTime).TotalSeconds < DoubleTapTime)
                OnDoubleTap();  
            
            lastTapTime = DateTime.Now;
        }
        private void OnDoubleTap()
        {
            gestureCount++;
            countText.text = gestureCount.ToString();
            flasher.FlashButton();
        }
    }
}