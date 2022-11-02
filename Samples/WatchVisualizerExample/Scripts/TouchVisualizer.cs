using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class TouchVisualizer : MonoBehaviour
    {

        [SerializeField] private Transform touchPoint;
        [SerializeField] private Renderer touchPointRenderer;
        [SerializeField] private Transform watchBody;
        
        [SerializeField] private TextMeshPro textW;
        [SerializeField] private TextMeshPro textH;

        private float watchDiameter;

        void Start()
        {
            watchDiameter = watchBody.localScale.x;
            touchPoint.localPosition = new Vector3(0,0,0);
        }

        public void UpdateTouchIndicator(float w, float h, bool isTouching)
        {
            touchPoint.localPosition = new Vector3(w * watchDiameter - watchDiameter / 2, h * watchDiameter - watchDiameter / 2, 0);
            textW.text = w.ToString("F4");
            textH.text = h.ToString("F4");

            if (isTouching) {
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
