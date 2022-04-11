using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class TapVisualizer : MonoBehaviour
    {
    
        [SerializeField] private TextMeshPro tapCountText;

        [SerializeField] private Renderer tapThingRenderer;

        private int tapCount;
        private float timeOfLastTap;

        void Update()
        {
            if (Time.time - timeOfLastTap > 0.05)
            {
                tapThingRenderer.material.color = Color.white;
            }
        }

        public void UpdateTapCount(int newTapCount)
        {
            tapCountText.text = newTapCount.ToString();

            if (newTapCount != tapCount)
            {
                tapThingRenderer.material.color = Color.black;
                timeOfLastTap = Time.time;
                tapCount = newTapCount;
            }
        }
    }
}
