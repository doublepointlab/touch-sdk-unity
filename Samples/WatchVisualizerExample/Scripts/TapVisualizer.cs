using UnityEngine;
using TMPro;
using Psix.Interaction;
using System.Collections.Generic;


namespace Psix.Examples
{
    using Psix.Interaction;
    public class TapVisualizer : VisualizerElement
    {

        [SerializeField] private TextMeshPro tapCountText;

        [SerializeField] private TextMeshPro surfaceTapCountText;

        [SerializeField] private Renderer tapThingRenderer;

        private int tapCount = 0;
        private int surfaceTapCount = 0;

        private bool releaseEnabled = false;

        private float timeOfLastTap;

        public override void RegisterWatch(Watch watch)
        {
            watch.OnGesture += OnGesture;
        }

        override public void RegisterGestures(HashSet<Gesture> gestures)
        {
            if (!gestures.Contains(Gesture.SurfaceTap)){
                surfaceTapCountText.SetText("--");
            }
            if (!gestures.Contains(Gesture.PinchTap)){
                tapCountText.SetText("--");
            }
            releaseEnabled = gestures.Contains(Gesture.PinchRelease);
        }

        void Update()
        {
            if (!releaseEnabled)
            {
                if (Time.time - timeOfLastTap > 0.2)
                    OnRelease();
            }
        }

        private void OnTap()
        {
            tapCount++;
            tapCountText.text = tapCount.ToString();
            tapThingRenderer.material.color = Color.black;
            timeOfLastTap = Time.time;
        }

        private void OnRelease()
        {
            tapThingRenderer.material.color = Color.white;
        }

        public void OnGesture(Gesture gesture)
        {
            if (gesture == Gesture.PinchTap)
            {
                OnTap();
            }
            else if (gesture == Gesture.PinchRelease)
            {
                OnRelease();
            }
            else if (gesture == Gesture.SurfaceTap)
            {
                surfaceTapCount++;
                surfaceTapCountText.text = surfaceTapCount.ToString();
            }

        }
    }
}