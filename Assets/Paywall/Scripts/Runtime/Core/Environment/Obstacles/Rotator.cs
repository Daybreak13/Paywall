using UnityEngine;

namespace Paywall {

    public class Rotator : MonoBehaviour {
        /// Starting angle of the object to rotate
        [field: Tooltip("Starting angle of the object to rotate")]
        [field: SerializeField] public float StartAngle { get; protected set; }
        /// Radius of the rotating object
        [field: Tooltip("Radius of the rotating object")]
        [field: SerializeField] public float Radius { get; protected set; }

        protected virtual void Rotate() {

        }
    }
}
