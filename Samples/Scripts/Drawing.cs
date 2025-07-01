using UnityEngine;

namespace Psix.Examples
{
    public class DrawingVisualiser : MonoBehaviour
    {
        private Rect rect;

        [SerializeField] private float velocityMultiplier;
        [SerializeField] private Transform tip;
        [SerializeField] private TrailRenderer trail;

        private Vector2 tipPosition;

        private void Start()
        {
            rect = GetComponent<RectTransform>().rect;
            trail.Clear();
        }

        void Update()
        {
            UpdateTipPosition();
        }

        private void UpdateTipPosition()
        {
            tipPosition -= GravityCorrectedGyroDelta * (Time.deltaTime * velocityMultiplier);
            tipPosition = ClampPointToLocalRectTransform(tipPosition);
            tip.localPosition = tipPosition;
        }

        private static Vector2 GravityCorrectedGyroDelta
        {
            get
            {
                var angularVelocity = Watch.Instance.AngularVelocity;
                var gravityVector = Watch.Instance.Gravity.normalized;

                var deltaY = angularVelocity.x * gravityVector.y - angularVelocity.y * gravityVector.x;
                var deltaX = angularVelocity.y * gravityVector.y + angularVelocity.x * gravityVector.x;

                return new Vector2(-deltaX, deltaY);
            }
        }

        private Vector2 ClampPointToLocalRectTransform(Vector2 localPoint)
        {
            localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
            localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);
            return localPoint;
        }
    }
}
