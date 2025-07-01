using UnityEngine;

namespace Psix.Examples
{
    public class ScrollingVisualiser : MonoBehaviour
    {
        private Vector2 velocity;
        [SerializeField] private float drag;
        [SerializeField] private float velocityMultiplier = 1f;
        [SerializeField] private ScrollDirection direction = ScrollDirection.Both;

        private MeshRenderer meshRenderer;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            velocity += Watch.Instance.GravityCorrectedGyroDelta * Time.deltaTime;
            velocity *= 1 - drag * Time.deltaTime;

            if (direction == ScrollDirection.Vertical)
                velocity.x = 0;
            else if (direction == ScrollDirection.Horizontal)
                velocity.y = 0;

            meshRenderer.material.mainTextureOffset += velocity * (velocityMultiplier * Time.deltaTime);
        }
    }

    public enum ScrollDirection
    {
        Horizontal,
        Vertical,
        Both
    }
}