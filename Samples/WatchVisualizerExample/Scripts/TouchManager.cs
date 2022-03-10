using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class TouchManager : MonoBehaviour
    {

        public GameObject touchPoint;
        private Renderer touchPointRenderer;
        public GameObject watchBody;
        [SerializeField]
        private TextMeshPro textW;
        [SerializeField]
        private TextMeshPro textH;

        private float watchDiameter;
        // Start is called before the first frame update
        void Start()
        {
            watchDiameter = watchBody.transform.localScale.x;
            touchPointRenderer = touchPoint.GetComponent<Renderer>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateTouchIndicator(float w, float h, bool isTouching)
        {
            touchPoint.transform.localPosition = new Vector3(w * watchDiameter - watchDiameter / 2, h * watchDiameter - watchDiameter / 2, 0);
            textW.text = w.ToString("F4");
            textH.text = h.ToString("F4");

            if (isTouching){
                textW.color = Color.white;
                textH.color = Color.white;
                touchPointRenderer.material.color = Color.white;

            }
            else
            {
                textW.color = Color.grey;
                textH.color = Color.grey;
                touchPointRenderer.material.color = Color.grey;
            }

        }
    }
}
