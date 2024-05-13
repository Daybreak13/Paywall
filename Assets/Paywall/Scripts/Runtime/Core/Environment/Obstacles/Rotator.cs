using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Add this component to a transform to rotate it every frame
    /// </summary>
    public class Rotator : MonoBehaviour {
        /// Speed at which the Z value changes
        [field: Tooltip("Speed at which the Z value changes")]
        [field: SerializeField] public float RotationSpeed { get; protected set; }
        /// Direction of rotation
        [field: Tooltip("Speed at which the Z value changes")]
        [field: SerializeField] public bool Clockwise { get; protected set; } = true;
        /// Block movement until passing a point set by LevelManager?
        [field: Tooltip("Block movement until passing a point set by LevelManager?")]
        [field: SerializeField] public bool BlockMovementUntil { get; protected set; } = true;

        protected float Direction => Clockwise ? -1f : 1f;
        protected float _startAngle;

        protected virtual void Awake() {
            _startAngle = transform.rotation.z;
        }

        protected virtual void Update() {
            Rotate();
        }

        /// <summary>
        /// Should we rotate
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldRotate() {
            if (BlockMovementUntil) {
                if (transform.position.x <= LevelManagerIRE_PW.Instance.MoveBarrier.transform.position.x) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// Rotate every frame based on level speed
        /// </summary>
        protected virtual void Rotate() {
            if (!ShouldRotate()) {
                return;
            }
            transform.Rotate(Direction * LevelManagerIRE_PW.Instance.CurrentUnmodifiedSpeed * Time.deltaTime * Vector3.forward);
        }

        protected virtual void ResetRotator() {
            transform.rotation = new(transform.rotation.x, transform.rotation.y, _startAngle, transform.rotation.w);
        }

        protected virtual void OnEnable() {
            ResetRotator();
        }
    }
}
