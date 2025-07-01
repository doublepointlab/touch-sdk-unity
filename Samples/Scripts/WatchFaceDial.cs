using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    using Interaction;
    public class DialVisualizer : MonoBehaviour
    {
        [SerializeField] private TextMeshPro dialText;
        [SerializeField] private Transform dialNorth;

        int position;

        private void OnEnable()
        {
            Watch.Instance.OnRotary += UpdateDialPosition;
        }
        
        private void OnDisable()
        {
            Watch.Instance.OnRotary -= UpdateDialPosition;
        }

        private void UpdateDialPosition(Direction dir)
        {
            position += dir == Direction.Clockwise ? -1 : 1;
            dialNorth.localEulerAngles = new Vector3(0, 0, position * (360f / 24));
            dialText.text = position.ToString();
        }
    }
}
