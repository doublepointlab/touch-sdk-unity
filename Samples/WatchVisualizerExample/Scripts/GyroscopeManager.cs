using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class GyroscopeManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject theX;

        [SerializeField]
        private GameObject theY;

        [SerializeField]
        private GameObject theZ;

        [SerializeField]
        private TextMeshPro textX;
        [SerializeField]
        private TextMeshPro textY;
        [SerializeField]
        private TextMeshPro textZ;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void UpdateGyroscope(Vector3 gyro)
        {
            theX.transform.localEulerAngles = new Vector3(gyro.x*20, 0, 0);
            theY.transform.localEulerAngles = new Vector3(gyro.y*20, 0, 0);
            theZ.transform.localEulerAngles = new Vector3(gyro.z*20, 0, 0);

            textX.text = gyro.x.ToString("F4");
            textY.text = gyro.y.ToString("F4");
            textZ.text = gyro.z.ToString("F4");
        }
    }
}
