using UnityEngine;
using TMPro;


namespace Psix.Examples
{
    using Psix.Interaction;
    public class DialVisualizer : VisualizerElement
    {
        [SerializeField] private TextMeshPro dialText;
        [SerializeField] private Transform dialNorth;

        int position = 0;

        public override void RegisterWatch(Watch watch)
        {
            watch.OnRotary += UpdateDialPosition;
        }

        public void UpdateDialPosition(Direction dir)
        {
            // Left handed coordinates, right?
            position += dir == Direction.Clockwise ? 1 : -1;
            dialNorth.localEulerAngles = new Vector3(0, position * (360 / 24), 0);
            dialText.text = position.ToString();
        }
    }
}
