using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class TapManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro tapCountText;

        [SerializeField]
        private GameObject tapThing;

        private int tapCount;
        private float timeOfLastTap;

        private Renderer tapThingRenderer;
        // Start is called before the first frame update
        void Start()
        {
            tapThingRenderer = tapThing.GetComponent<Renderer>();
        }

        // Update is called once per frame
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
