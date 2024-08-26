using Paywall.Tools;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Add this component to a transform to rotate it every frame
    /// </summary>
    public class Rotator : MonoBehaviour {
        /// Auto calculate rotation speed based on level speed
        [field: Tooltip("Auto calculate rotation speed based on level speed")]
        [field: SerializeField] public bool UseLevelSpeed { get; protected set; } = true;
        /// Speed at which the Z value changes
        [field: Tooltip("Speed at which the Z value changes")]
        [field: FieldCondition("UseLevelSpeed", true, true)]
        [field: SerializeField] public float RotationSpeed { get; protected set; }
        /// Direction of rotation
        [field: Tooltip("Direction of rotation")]
        [field: SerializeField] public bool Clockwise { get; protected set; } = true;
        /// Block movement until passing a point set by LevelManager?
        [field: Tooltip("Block movement until passing a point set by LevelManager?")]
        [field: SerializeField] public bool BlockMovementUntil { get; protected set; } = true;

        protected float Direction => Clockwise ? -1f : 1f;
        protected Quaternion _initialRotation;

        protected virtual void Awake() {
            _initialRotation = transform.rotation;
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
            float rotateSpeed = UseLevelSpeed ? LevelManagerIRE_PW.Instance.CurrentUnmodifiedSpeed : RotationSpeed;
            transform.Rotate(Direction * rotateSpeed * Time.deltaTime * Vector3.forward);
        }

        /// <summary>
        /// Resets the rotation of this object to the initial rotation
        /// </summary>
        protected virtual void ResetRotator() {
            transform.rotation = _initialRotation;
        }

        protected virtual void OnEnable() {
            ResetRotator();
        }
    }
}
