using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    using Psix.Interaction;

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
            touchPoint.localPosition = new Vector3(0, 0, 0);
        }

        public void UpdateTouchIndicator(TouchEvent args)
        {
            switch (args.type)
            {
                case TouchType.On:
                    textW.color = Color.white;
                    textH.color = Color.white;
                    touchPointRenderer.material.color = Color.white;
                    goto case TouchType.Move;
                case TouchType.Move:
                    touchPoint.localPosition = new Vector3(args.coords.x * watchDiameter - watchDiameter / 2, args.coords.y * watchDiameter - watchDiameter / 2, 0);
                    textW.text = args.coords.x.ToString("F4");
                    textH.text = args.coords.y.ToString("F4");
                    break;
                case TouchType.Off:
                    textW.color = Color.grey;
                    textH.color = Color.grey;
                    touchPointRenderer.material.color = Color.grey;
                    break;


            }
        }
    }
}