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
            _speed = Distance / Time.fixedDeltaTime / LevelManagerIRE_PW.Instance.SpeedMultiplier / 5 - LevelManagerIRE_PW.Instance.FinalSpeed;
        }

        int count;
        protected virtual void FixedUpdate() {
            //if (Exit.position.x <= _entryPos.x
            //    && !_exited) {
            //    //LevelManagerIRE_PW.Instance.TemporarilyAddSpeedSwitch(_speed, _guid);
            //    _exited = true;
            //    _teleporting = false;
            //}
            //if (_teleporting) {
            //    //Debug.Log(count++);
            //}

            if (_teleporting && Exit.position.x <= _character.transform.position.x) {
                _teleporting = false;
                _character.transform.SafeSetTransformPosition(Exit.position, PaywallLayerManager.ObstaclesLayerMask);
                _character.SetTeleportState(false);
                _character = null;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.PlayerTag)) {
                _character = collider.GetComponent<PlayerCharacterIRE>();
                if (Distance > 0) {
                    _character.SetTeleportState(true);
                    LevelManagerIRE_PW.Instance.TemporarilyAddSpeedDist(_speed, Distance);
                    _teleporting = true;
                }
                else {
                    collider.transform.SafeSetTransformPosition(Exit.position, PaywallLayerManager.ObstaclesLayerMask);
                    _character = null;
                }

            }
            else {

            }
        }
    }
}
