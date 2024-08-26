using MoreMountains.Tools;
using Paywall.Tools;
using System.Collections;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Behavior and animation component for bomb
    /// Can take and cause damage
    /// </summary>
    public class Bomb_PW : MonoBehaviour {
        /// Collider representing the bomb explosion
        [field: Tooltip("Collider representing the bomb explosion")]
        [field: SerializeField] public Collider2D ExplosionCollider { get; protected set; }
        /// Do we explode on contact with the player?
        [field: Tooltip("Do we explode on contact with the player?")]
        [field: SerializeField] public bool ExplodeOnContact { get; protected set; }
        /// The health component associated with this bomb, if applicable
        [field: Tooltip("The health component associated with this bomb, if applicable")]
        [field: SerializeField] public Health_PW HealthComponent { get; protected set; }
        /// How long to wait before destroying this object on destruction
        [field: Tooltip("How long to wait before destroying this object on destruction")]
        [field: FieldNullCondition("HealthComponent", true)]
        [field: SerializeField] public float DelayBeforeDestruction { get; protected set; }
        /// Animator component
        [field: Tooltip("Animator component")]
        [field: SerializeField] public Animator AnimatorComponent { get; protected set; }

        protected bool _exploded;
        protected WaitForSeconds _wait;

        protected const string _explodingAnimatorParameterName = "Exploding";

        public virtual void Explode() {
            ExplosionCollider.enabled = true;
            _exploded = true;
            MMAnimatorExtensions.UpdateAnimatorBoolIfExists(AnimatorComponent, _explodingAnimatorParameterName, true);
            if (_wait != null) {
                StartCoroutine(DestroyCo());
            }
        }

        protected virtual void Awake() {
            if (HealthComponent == null) {
                if (DelayBeforeDestruction > 0) {
                    _wait = new(DelayBeforeDestruction);
                }
            }
            else {
                if (HealthComponent.DelayBeforeDestruction > 0) {
                    _wait = new(HealthComponent.DelayBeforeDestruction);
                }
            }
            if (AnimatorComponent == null) {
                AnimatorComponent = GetComponent<Animator>();
            }
        }

        protected virtual IEnumerator DestroyCo() {
            yield return _wait;
            gameObject.SetActive(false);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.CompareTag(PaywallTagManager.PlayerTag) && ExplodeOnContact && !_exploded) {
                Explode();
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision) {
            if (collision.gameObject.CompareTag(PaywallTagManager.PlayerTag) && ExplodeOnContact && !_exploded) {
                Explode();
            }
        }

        protected virtual void OnEnable() {
            ExplosionCollider.enabled = false;
            _exploded = false;
            MMAnimatorExtensions.UpdateAnimatorBoolIfExists(AnimatorComponent, _explodingAnimatorParameterName, false);
        }
    }
}
