using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.CorgiEngine;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    public class CharacterHandleWeaponIRE : CharacterAbilityIRE, MMEventListener<RunnerItemPickEvent> {
        [field: Header("Weapon Components")]

        /// The projectile weapon prefab
        [field: Tooltip("The projectile weapon prefab")]
        [field: SerializeField] public Weapon WeaponComponent { get; protected set; }

        [field: Header("Other Settings")]

        /// If true, instantiate a new weapon on start using the WeaponComponent prefab
        [field: Tooltip("If true, instantiate a new weapon on start using the WeaponComponent prefab")]
        [field: SerializeField] public bool AttachWeapon { get; protected set; }
        /// the position the weapon will be attached to. If left blank, will be this.transform.
        [field: Tooltip("the position the weapon will be attached to. If left blank, will be this.transform.")]
        [field: FieldCondition("AttachWeapon", true)]
        [field: SerializeField] public Transform WeaponAttachment { get; protected set; }
        /// If true, instantiate a new weapon on start using the WeaponComponent prefab
        [field: Tooltip("If true, instantiate a new weapon on start using the WeaponComponent prefab")]
        [field: SerializeField] public bool InstantiateNewWeapon { get; protected set; }

        [field: Header("Buffering")]

        /// whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them
        [field: Tooltip("whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them")]
        [field: SerializeField] public bool BufferInput { get; protected set; }
        /// if this is true, every new input will prolong the buffer
        [field: FieldCondition("BufferInput", true)]
        [field: Tooltip("if this is true, every new input will prolong the buffer")]
        [field: SerializeField] public bool NewInputExtendsBuffer { get; protected set; }
        /// the maximum duration for the buffer, in seconds
        [field: FieldCondition("BufferInput", true)]
        [field: Tooltip("the maximum duration for the buffer, in seconds")]
        [field: SerializeField] public float MaximumBufferDuration { get; protected set; } = 0.25f;

        [field: Header("Debug")]
        /// returns the current equipped weapon
        [field: MMReadOnly]
        [field: Tooltip("returns the current equipped weapon")]
        [field: SerializeField] public Weapon CurrentWeapon { get; protected set; }

        protected bool _buffering = false;
        protected float _bufferEndsAt = 0f;
        protected Coroutine _airStallCoroutine;

        protected override void Initialization() {
            base.Initialization();
            Setup();
            if (CurrentWeapon is ProjectileWeapon_PW) {
                (CurrentWeapon as ProjectileWeapon_PW).SetOwner(_character, this);
            }
            if (CurrentWeapon.MagazineBased) {
                if (GUIManagerIRE_PW.HasInstance) {
                    (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).UpdateAmmoBar(CurrentWeapon.CurrentAmmoLoaded, 0, CurrentWeapon.MagazineSize);
                }
            }
        }

        protected virtual void Setup() {
            if (AttachWeapon) {
                if (WeaponAttachment == null) {
                    WeaponAttachment = transform;
                }
            }
            if (InstantiateNewWeapon) {

            } else {
                CurrentWeapon = WeaponComponent;
            }
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            UpdateAmmoDisplay();
            HandleBuffer();
        }

        protected virtual void LateUpdate() {
            HandleAirStall();
        }

        /// <summary>
		/// Triggers an attack if the weapon is idle and an input has been buffered
		/// </summary>
		protected virtual void HandleBuffer() {
            if (CurrentWeapon == null) {
                return;
            }

            // if we are currently buffering an input and if the weapon is now idle
            if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)) {
                // and if our buffer is still valid, we trigger an attack
                if (Time.time < _bufferEndsAt) {
                    AttackStart();
                }
                _buffering = false;
            }
        }
        protected int _count = 0;
        protected virtual void AttackPerformed(InputAction.CallbackContext context) {
            AttackStart();
        }

        protected virtual void AttackStart() {
            if (!AbilityAuthorized || WeaponComponent == null) {
                return;
            }
            //  if we've decided to buffer input, and if the weapon is in use right now
            if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle)) {
                // if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
                if (!_buffering || NewInputExtendsBuffer) {
                    _buffering = true;
                    _bufferEndsAt = Time.time + MaximumBufferDuration;
                }
            }

            WeaponComponent.WeaponInputStart();
        }

        public virtual void SetCurrentWeaponMagSize(int size) {
            if (!CurrentWeapon.MagazineBased) {
                return;
            }
            CurrentWeapon.MagazineSize = size;
            UpdateAmmoDisplay();
        }

        protected virtual void UpdateAmmoDisplay() {
            if ((GUIManagerIRE_PW.HasInstance) && (_character != null)) {
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).UpdateAmmoBar(CurrentWeapon.CurrentAmmoLoaded, 0, CurrentWeapon.MagazineSize);
            }
        }

        protected virtual void HandleAirStall() {
            if ((_rigidbody2D.velocity.y > 0) && (_airStallCoroutine != null)) {
                StopCoroutine(_airStallCoroutine);
                _rigidbody2D.gravityScale = _initialGravityScale;
            }
        }

        /// <summary>
        /// Stalls the player in the air
        /// </summary>
        /// <param name="stallTime"></param>
        public virtual void AirStall(float stallTime) {
            if (_rigidbody2D.velocity.y <= 0) {
                if (_airStallCoroutine != null) {
                    StopCoroutine(_airStallCoroutine);
                }
                _airStallCoroutine = StartCoroutine(AirStallCo(stallTime));
            }
        }

        /// <summary>
        /// Air stall coroutine
        /// </summary>
        /// <param name="stallTime"></param>
        /// <returns></returns>
        protected virtual IEnumerator AirStallCo(float stallTime) {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0f);
            _rigidbody2D.gravityScale = 0;
            yield return new WaitForSeconds(stallTime);
            _rigidbody2D.gravityScale = _initialGravityScale;
        }

        public override void ResetAbility() {
            CurrentWeapon.CurrentAmmoLoaded = CurrentWeapon.MagazineSize;
        }

        /// <summary>
        /// When an ammo fragment is picked up, increase fragment count
        /// </summary>
        /// <param name="itemPickEvent"></param>
        public virtual void OnMMEvent(RunnerItemPickEvent itemPickEvent) {
            if ((itemPickEvent.PickedPowerUpType == PowerUpTypes.Ammo) && _character.CompareTag("Player")) {

            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.MMEventStartListening<RunnerItemPickEvent>();
            _inputManager.InputActions.PlayerControls.Attack.performed += AttackPerformed;
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<RunnerItemPickEvent>();
            _inputManager.InputActions.PlayerControls.Attack.performed -= AttackPerformed;
        }
    }
}
