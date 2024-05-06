using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Paywall.Tools {

    public enum ColliderPositions { Top, Center, Bottom }

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
        /// If true, manually set edge collider points' Y position in inspector, instead of using SpriteRenderer bounds
        [field: Tooltip("If true, manually set edge collider points' Y position in inspector, instead of using SpriteRenderer bounds")]
        [field: SerializeField] public bool OverrideYPosition { get; protected set; }
        /// Where in the collider bounds to place the Y position of the edge collider
        [field: Tooltip("Where in the collider bounds to place the Y position of the edge collider")]
        [field: FieldCondition("OverrideYPosition", true, true)]
        [field: SerializeField] public ColliderPositions Position { get; protected set; }

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
            Vector2 p0 = Vector2.zero;
            Vector2 p1 = Vector2.zero;
            if (OverrideYPosition) {
                p0 = new(Renderer.bounds.min.x - SizeModification, EdgeCollider.points[0].y);
                p1 = new(Renderer.bounds.max.x + SizeModification, EdgeCollider.points[1].y);
                p0.x = transform.InverseTransformPoint(p0).x;
                p1.x = transform.InverseTransformPoint(p1).x;
            }
            else {
                switch (Position) {
                    case ColliderPositions.Top:
                        p0 = new(Renderer.bounds.min.x - SizeModification, Renderer.bounds.max.y);
                        p1 = new(Renderer.bounds.max.x + SizeModification, Renderer.bounds.max.y);
                        break;
                    case ColliderPositions.Center:
                        p0 = new(Renderer.bounds.min.x - SizeModification, Renderer.bounds.center.y);
                        p1 = new(Renderer.bounds.max.x + SizeModification, Renderer.bounds.center.y);
                        break;
                    case ColliderPositions.Bottom:
                        p0 = new(Renderer.bounds.min.x - SizeModification, Renderer.bounds.min.y);
                        p1 = new(Renderer.bounds.max.x + SizeModification, Renderer.bounds.min.y);
                        break;
                }
                p0 = transform.InverseTransformPoint(p0);
                p1 = transform.InverseTransformPoint(p1);
            }
            EdgeCollider.SetPoints(new() { p0, p1 });
        }
    }
}
