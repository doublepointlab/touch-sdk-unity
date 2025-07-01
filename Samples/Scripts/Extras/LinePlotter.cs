using UnityEngine;

namespace Psix.Examples
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(RectTransform))]
    public class LinePlotter : MonoBehaviour
    {
        [SerializeField] private int maxPoints = 100;
        private LineRenderer lineRenderer;
        private float[] values;
        private Vector2 plotterSize;
        private float xStep;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            plotterSize = GetComponent<RectTransform>().rect.size;
            xStep = plotterSize.x / maxPoints;
            values = new float[maxPoints];
        }

        public void AddPoint(float value, float min, float max)
        {
            value = Mathf.Clamp(value, min, max);
            
            for (var i = 0; i < values.Length - 1; i++)
            {
                values[i] = values[i + 1];
            }

            values[^1] = Remap(value, min, max);

            var points = new Vector3[maxPoints];
            for (var i = 0; i < values.Length; i++)
            {
                points[i] = new Vector3(i * xStep - plotterSize.x / 2, values[i] * plotterSize.y, 0);
            }

            lineRenderer.positionCount = maxPoints;
            lineRenderer.SetPositions(points);
        }
        
        private static float Remap(float value, float min, float max)
        {
            return (value - min) / (max - min) - .5f;
        }
    }
}
