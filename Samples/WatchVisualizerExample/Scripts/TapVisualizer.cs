using UnityEngine;
using TMPro;


namespace Psix.Examples
{
    using Psix.Interaction;
    public class TapVisualizer : MonoBehaviour
    {

        [SerializeField] private TextMeshPro tapCountText;

        [SerializeField] private Renderer tapThingRenderer;

        private int tapCount = 0;
        private float timeOfLastTap;

        void Update()
        {
            if (Time.time - timeOfLastTap > 0.05)
            {
                tapThingRenderer.material.color = Color.white;
            }
        }

        public void UpdateTapCount(Gesture gesture)
        {
            if (gesture != Gesture.Tap)
                return;
            tapCount++;
            tapCountText.text = tapCount.ToString();

            tapThingRenderer.material.color = Color.black;
            timeOfLastTap = Time.time;
        }
    }
}