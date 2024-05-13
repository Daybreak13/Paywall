using UnityEngine;

namespace Paywall.Tools {

    /// <summary>
    /// Provides a visual indicator of a collider's bounds
    /// </summary>
    public class BoundsVisualization : MonoBehaviour {
        public BoxCollider2D boxCollider2D;

        void OnDrawGizmosSelected() {
            if (boxCollider2D == null) {
                boxCollider2D = GetComponent<BoxCollider2D>();
            }
            if (boxCollider2D != null) {
                // Draw the bounds of the BoxCollider2D
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(boxCollider2D.bounds.center, boxCollider2D.bounds.size);
            }
        }
    }
}
