using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Paywall.Tools {

    /// <summary>
    /// Add this to a gameobject with an EdgeCollider2D and SpriteRenderer to have it resize the collider automatically based on renderer bounds
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public class EdgeColliderResizer : MonoBehaviour {
        /// Distance past the bounds to destroy this object
        [field: Tooltip("Distance past the bounds to destroy this object")]
        [field: SerializeField] public EdgeCollider2D EdgeCollider { get; protected set; }
        /// Distance past the bounds to destroy this object
        [field: Tooltip("Distance past the bounds to destroy this object")]
        [field: SerializeField] public SpriteRenderer Renderer { get; protected set; }
        /// How much to modify the edge collider length by, after resizing based on renderer
        [field: Tooltip("How much to modify the edge collider length by, after resizing based on renderer")]
        [field: SerializeField] public float SizeModification { get; protected set; }

        protected List<Vector2> _points = new();
        protected bool _initialized;

        protected virtual void Awake() {
            Initialization();
            Resize();
        }

        protected virtual void Initialization() {
            if (_initialized) return;
            _initialized = true;
            if (EdgeCollider == null) {
                EdgeCollider = GetComponent<EdgeCollider2D>();
            }
            if (Renderer == null) {
                Renderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Resize collider when sprite renderer is changed in editor
        /// </summary>
        protected virtual void OnValidate() {
            Resize();
        }

        public virtual void Resize() {
            if (!_initialized) Initialization();
            if (EdgeCollider == null || Renderer == null) return;
            Vector2 p0 = new(Renderer.bounds.min.x - SizeModification, EdgeCollider.points[0].y);
            Vector2 p1 = new(Renderer.bounds.max.x + SizeModification, EdgeCollider.points[1].y);
            EdgeCollider.SetPoints(new() { p0, p1 });
        }
    }
}
