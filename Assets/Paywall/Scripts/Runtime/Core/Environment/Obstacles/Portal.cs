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

        protected float _speed;
        protected bool _teleporting;
        protected PlayerCharacterIRE _character;
        protected float _distance;

        public static readonly int TeleportFrames = 10;      // How many FixedUpdate frames should the teleport last


        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.PlayerTag)) {
                _character = collider.GetComponent<PlayerCharacterIRE>();
                _distance = Exit.position.x - _character.transform.position.x;
                if (_distance > 0) {
                    // Speed is divided by SpeedMultiplier, because MovingRigidbodies will multiply by the SpeedMultiplier
                    _speed = _distance / Time.fixedDeltaTime / LevelManagerIRE_PW.Instance.SpeedMultiplier / TeleportFrames;
                    LevelManagerIRE_PW.Instance.TemporarilyAddSpeedDist(_speed, _distance);
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
