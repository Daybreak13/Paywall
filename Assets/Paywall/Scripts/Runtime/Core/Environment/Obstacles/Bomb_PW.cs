using UnityEngine;

namespace Paywall {

    public class Bomb_PW : MonoBehaviour {
        /// Collider representing the bomb explosion
        [field: Tooltip("Collider representing the bomb explosion")]
        [field: SerializeField] public Collider2D BombCollider { get; protected set; }

        protected virtual void Awake() {
            BombCollider.enabled = false;
        }

        public virtual void Explode() {
            BombCollider.enabled = true;
        }
    }
}
