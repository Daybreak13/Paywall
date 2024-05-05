using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Add this component to an item to allow it to magnetize
    /// </summary>
    public class MagneticItem : MonoBehaviour {
        protected GameObject _player;
        protected Rigidbody2D _rigidbody;
        protected float _speed;
        protected float _resetDistance = 10f;

        protected virtual void Awake() {
            if (_rigidbody == null) {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        protected virtual void FixedUpdate() {
            if (_player == null) { return; }
            if (Vector2.Distance(_player.transform.position, transform.position) > _resetDistance) { ResetItem(); return; }

            //Vector2 direction = (transform.position - _player.transform.position).normalized;
            //float travelSpeed = Vector2.Distance(transform.position, _player.transform.position);
            //_rigidbody.velocity = -direction * travelSpeed;

            Vector2 direction = (_player.transform.position - transform.position).normalized;
            Vector2 velocity = direction * 20f;
            _rigidbody.MovePosition(_rigidbody.position + velocity * Time.fixedDeltaTime);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (collider.CompareTag(PaywallTagManager.MagnetTag)) {
                _player = collider.gameObject;
            }
        }

        protected virtual void ResetItem() {
            _player = null;
            _rigidbody.velocity = Vector2.zero;
        }

        protected virtual void OnEnable() {
            ResetItem();
        }
    }
}
