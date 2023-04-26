using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;

namespace Paywall {

    /// <summary>
    /// If the player jumps on an object with this component, it will launch the player
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LaunchPad : MonoBehaviour {
        [field: Header("Launch Pad")]

        /// If true, use launch force instead of fixed height
        [field: Tooltip("If true, use launch force instead of fixed height")]
        [field: SerializeField] public bool UseLaunchForce;
        /// Launch force
        [field: Tooltip("Launch force")]
        [field: FieldCondition("UseLaunchForce", true, false)]
        [field: SerializeField] public float LaunchForce = 10f;
        /// Launch height
        [field: Tooltip("Launch height")]
        [field: FieldCondition("UseLaunchForce", true, true)]
        [field: SerializeField] public float LaunchHeight = 2f;
        /// How many rays to cast to check for collision above
        [field: Tooltip("How many rays to cast to check for collision above")]
        [field: SerializeField] public int RaysToCast = 5;

        protected const string _playerTag = "Player";
        protected Collider2D _collider2D;
        protected BoxCollider2D _collidingObject;
        protected bool _collidingAbove;
        protected PaywallPlayableCharacter _player;

        protected virtual void Awake() {
            _collider2D = GetComponent<Collider2D>();
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.CompareTag(_playerTag)) {
                _collidingObject = (BoxCollider2D) collision.collider;
                _player = collision.collider.GetComponent<PaywallPlayableCharacter>();
                if (_player != null) {
                    _collidingAbove = CastRaysAbove();
                    if (CastRaysAbove()) {
                        if (UseLaunchForce) {
                            _player.ApplyExternalJumpForce(LaunchForce, true, true);
                        } else {
                            _player.ApplyExternalJumpForce(LaunchHeight, false, true);
                        }
                    }
                }
            }
        }

        protected virtual bool CheckIfAbove() {
            float bottom = _collidingObject.bounds.min.y;
            float top = _collider2D.bounds.max.y;
            if ((bottom - top) >= -0.05f) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the player is directly above this object
        /// </summary>
        /// <returns></returns>
        protected virtual bool CastRaysAbove() {
            Vector2 leftOrigin = new(_collider2D.bounds.min.x, _collider2D.bounds.max.y);
            Vector2 centerOrigin = new(_collider2D.bounds.center.x, _collider2D.bounds.max.y);
            Vector2 rightOrigin = new(_collider2D.bounds.max.x, _collider2D.bounds.max.y);
            RaycastHit2D raycastHit2D;
            raycastHit2D = Physics2D.Raycast(leftOrigin, Vector2.up, 1 << LayerMask.NameToLayer(_playerTag));
            if (raycastHit2D.collider != null) {
                _collidingAbove = true;
                return true;
            }
            raycastHit2D = Physics2D.Raycast(centerOrigin, Vector2.up, 1 << LayerMask.NameToLayer(_playerTag));
            if (raycastHit2D.collider != null) {
                _collidingAbove = true;
                return true;
            }
            raycastHit2D = Physics2D.Raycast(rightOrigin, Vector2.up, 1 << LayerMask.NameToLayer(_playerTag));
            if (raycastHit2D.collider != null) {
                _collidingAbove = true;
                return true;
            }
            _collidingAbove = false;
            return false;
        }

    }
}
