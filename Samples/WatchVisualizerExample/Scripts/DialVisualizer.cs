using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class DialVisualizer : MonoBehaviour
    {
        [SerializeField] private TextMeshPro dialText;
        [SerializeField] private Transform dialNorth;

        public void UpdateDialPosition(int position)
        {
            dialNorth.localEulerAngles = new Vector3(0, position * (360 / 24), 0);
            dialText.text = position.ToString();
        }
    }
}
