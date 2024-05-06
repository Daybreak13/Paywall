using Paywall.Tools;
using System;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Attach this to an entry portal gameobject and bind the exit portal parameter to have the player teleport to the exit upon entering
    /// </summary>
    public class Portal : MonoBehaviour {
        /// The exit portal
        [field: Tooltip("The exit portal")]
        [field: SerializeField] public Transform Exit { get; protected set; }

        protected float _speed = 1000f;
        protected Guid _guid;
        protected bool _teleporting;
        protected PlayerCharacterIRE _character;

        protected float Distance { get {
                return Exit.position.x - transform.position.x;
            }
        }

        protected virtual void Awake() {
            _guid = Guid.NewGuid();
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.PlayerTag)) {
                _character = collider.GetComponent<PlayerCharacterIRE>();
                if (Distance > 0) {
                    _speed = Distance / Time.fixedDeltaTime / LevelManagerIRE_PW.Instance.SpeedMultiplier / 12 - LevelManagerIRE_PW.Instance.FinalSpeed;
                    LevelManagerIRE_PW.Instance.TemporarilyAddSpeedDist(_speed, Distance);
                    _character.Teleporting = true;
                }
                collider.transform.SafeSetTransformPosition(new Vector3(Exit.position.x, Exit.position.y, _character.transform.position.z), PaywallLayerManager.ObstaclesLayerMask);
                _character = null;

            }

            else if (collider.gameObject.layer != PaywallLayerManager.ObstaclesLayerMask && !collider.gameObject.CompareTag(PaywallTagManager.MagnetTag)) {
                collider.transform.SafeSetTransformPosition(new Vector3(Exit.position.x, Exit.position.y, collider.transform.position.z), PaywallLayerManager.ObstaclesLayerMask);
            }
        }
    }
}
