using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class DialManager : MonoBehaviour
    {

        [SerializeField]
        private TextMeshPro dialText;

        [SerializeField]
        private GameObject dialNorth;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void UpdateDialPosition(int position)
        {
            dialNorth.transform.localEulerAngles = new Vector3(0, position * (360 / 24), 0);
            dialText.text = position.ToString();
        }
    }
}
