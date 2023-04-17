using UnityEngine;
using TMPro;


namespace Psix.Examples
{
    using Psix.Interaction;
    public class DialVisualizer : MonoBehaviour
    {
        [SerializeField] private TextMeshPro dialText;
        [SerializeField] private Transform dialNorth;

        int position = 0;

        public void UpdateDialPosition(Direction dir)
        {
            // Left handed coordinates, right?
            position += dir == Direction.Clockwise ? 1 : -1;
            dialNorth.localEulerAngles = new Vector3(0, position * (360 / 24), 0);
            dialText.text = position.ToString();
        }
    }
}
