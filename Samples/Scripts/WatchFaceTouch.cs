using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    using Interaction;

    public class TouchVisualizer : MonoBehaviour
    {
        [SerializeField] private Renderer touchPoint;

        [SerializeField] private TextMeshPro textX, textY;
        
        private const int TouchScreenWidth = 480; //resolution of galaxy watch 6 44mm, ideally would be using Screen.width and Screen.height
        
        private void OnEnable()
        {
            Watch.Instance.OnTouch += UpdateTouchIndicator;
        }
        private void OnDisable()
        {
            Watch.Instance.OnTouch -= UpdateTouchIndicator;
        }

        private void UpdateTouchIndicator(TouchEvent args)
        {
            switch (args.type)
            {
                case TouchType.Press:
                    textX.color = Color.white;
                    textY.color = Color.white;
                    touchPoint.material.color = Color.white;
                    goto case TouchType.Move;
                case TouchType.Move:
                    touchPoint.transform.localPosition = new Vector3( RemappedCoord(args.coords.x), -RemappedCoord(args.coords.y), 0);
                    textX.text = args.coords.x.ToString("F4");
                    textY.text = args.coords.y.ToString("F4");
                    break;
                case TouchType.Release:
                    textX.color = Color.grey;
                    textY.color = Color.grey;
                    touchPoint.material.color = Color.grey;
                    break;
            }
        }
        
        private static float RemappedCoord(float coord) => Mathf.Clamp01(coord / TouchScreenWidth) - .5f;
    }
}