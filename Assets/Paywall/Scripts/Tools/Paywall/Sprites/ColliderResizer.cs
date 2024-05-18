using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Paywall.Tools {

    public enum ColliderPositions { Top, Center, Bottom }

    /// <summary>
    /// Add this to a gameobject with an EdgeCollider2D and SpriteRenderer to have it resize the collider automatically based on renderer bounds
    /// </summary>
    public class ColliderResizer : MonoBehaviour {
        /// Is this resizer enabled
        [field: Tooltip("Is this resizer enabled")]
        [field: SerializeField] public bool Enabled { get; protected set; } = true;
        /// Distance past the bounds to destroy this object
        [field: Tooltip("Distance past the bounds to destroy this object")]
        [field: SerializeField] public Collider2D Collider { get; protected set; }
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

        protected Bounds _unrotatedBounds;
        protected float _top;
        protected float _center;
        protected float _bottom;
        protected float _minX;
        protected float _maxX;

        protected virtual void Awake() {
            Resize();
        }

        protected virtual void GetInitialBounds() {
            Quaternion originalRot = Renderer.transform.rotation;
            Renderer.transform.rotation = Quaternion.identity;
            _unrotatedBounds = Renderer.bounds;
            _top = Renderer.transform.InverseTransformPoint(_unrotatedBounds.max).y;
            _center = Renderer.transform.InverseTransformPoint(_unrotatedBounds.center).y;
            _bottom = Renderer.transform.InverseTransformPoint(_unrotatedBounds.min).y;
            _minX = Renderer.transform.InverseTransformPoint(_unrotatedBounds.min).x;
            _maxX = Renderer.transform.InverseTransformPoint(_unrotatedBounds.max).x;
            Renderer.transform.rotation = originalRot;
        }

        /// <summary>
        /// Are conditions correct to allow resize?
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldResize() {
            if (!Enabled || !gameObject.activeInHierarchy || Collider == null || Renderer == null) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Resize collider when sprite renderer is changed in editor
        /// </summary>
        protected virtual void OnValidate() {
            Resize();
        }

        public virtual void Resize() {
            if (!ShouldResize()) {
                return;
            }
            GetInitialBounds();
            Vector2 p0 = Vector2.zero;
            Vector2 p1 = Vector2.zero;
            if (Collider is EdgeCollider2D edge) {
                if (OverrideYPosition) {
                    if (Renderer.drawMode == SpriteDrawMode.Tiled) {
                        p0 = new(-Renderer.size.x / 2f - SizeModification, edge.points[0].y);
                        p1 = new(Renderer.size.x / 2f + SizeModification, edge.points[1].y);
                    }
                    else {
                        p0 = new(Renderer.bounds.min.x - SizeModification, edge.points[0].y);
                        p1 = new(Renderer.bounds.max.x + SizeModification, edge.points[1].y);
                        p0.x = transform.InverseTransformPoint(p0).x;
                        p1.x = transform.InverseTransformPoint(p1).x;
                    }
                }
                else {
                    switch (Position) {
                        case ColliderPositions.Top:
                            p0 = new(_minX - SizeModification, _top);
                            p1 = new(_maxX + SizeModification, _top);
                            break;
                        case ColliderPositions.Center:
                            p0 = new(_minX - SizeModification, _center);
                            p1 = new(_maxX + SizeModification, _center);
                            break;
                        case ColliderPositions.Bottom:
                            p0 = new(_minX - SizeModification, _bottom);
                            p1 = new(_maxX + SizeModification, _bottom);
                            break;
                    }
                }
                edge.SetPoints(new() { p0, p1 });
            }

            if (Collider is BoxCollider2D box) {
                if (OverrideYPosition) {
                    box.size = new(Renderer.size.x, box.size.y);

                }
                else {

                }
            }

        }

        protected virtual void OnDrawGizmosSelected() {
            //Gizmos.color = Color.blue;
            //Gizmos.DrawWireCube(_unrotatedBounds.center, _unrotatedBounds.size);
        }
    }
}
