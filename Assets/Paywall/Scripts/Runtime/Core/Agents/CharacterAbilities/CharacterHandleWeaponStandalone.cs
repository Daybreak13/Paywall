using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.CorgiEngine;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    public class CharacterHandleWeaponStandalone : CharacterAbilityIRE, MMEventListener<RunnerItemPickEvent> {
        [field: Header("Weapon Components")]

        /// The projectile weapon prefab
        [field: Tooltip("The projectile weapon prefab")]
        [field: SerializeField] public WeaponStandalone WeaponComponent { get; protected set; }

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
        [field: SerializeField] public WeaponStandalone CurrentWeapon { get; protected set; }

        protected bool _sameFrame;
        protected bool _buffering = false;
        protected float _bufferEndsAt = 0f;
        protected Coroutine _airStallCoroutine;
        protected int _ammoLastFrame;
        protected int _magSizeLastFrame;

        protected override void Initialization() {
            base.Initialization();
            Setup();
            CurrentWeapon.SetOwner(_character, this);
            if (CurrentWeapon.MagazineBased) {
                _ammoLastFrame = CurrentWeapon.CurrentAmmoLoaded;
                _magSizeLastFrame = CurrentWeapon.MagazineSize;
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

        /// <summary>
        /// Every frame, check for inputs and stop or use the weapon depending
        /// </summary>
        public override void ProcessAbility() {
            base.ProcessAbility();
            if (InputActions.PlayerControls.Attack.WasPerformedThisFrame()) {
                _sameFrame = true;
                AttackStart();
            }
            if (InputActions.PlayerControls.Attack.WasReleasedThisFrame()) {
                if (_sameFrame) {
                    StartCoroutine(WaitNextFrameToStopAttack());
                }
                else {
                    StopAttack();
                }
            }
            _sameFrame = false;
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

        #region Callbacks

        /// <summary>
        /// What to do when the attack button is pressed
        /// Use if using callbacks
        /// </summary>
        /// <param name="context"></param>
        protected virtual void AttackPerformed(InputAction.CallbackContext context) {
            AttackStart();
        }

        /// <summary>
        /// What to do when the attack button is released
        /// Use if using callbacks
        /// </summary>
        /// <param name="context"></param>
        protected virtual void AttackStopped(InputAction.CallbackContext context) {

        }

        #endregion

        /// <summary>
        /// Wait one frame to stop attack
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator WaitNextFrameToStopAttack() {
            yield return new WaitForEndOfFrame();
            StopAttack();
        }

        /// <summary>
        /// Stops the weapon firing
        /// </summary>
        protected virtual void StopAttack() {
            WeaponComponent.StopWeapon();
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

            WeaponComponent.RequestShoot();
        }

        /// <summary>
        /// Set magazine size of current weapon
        /// </summary>
        /// <param name="size"></param>
        public virtual void SetCurrentWeaponMagSize(int size) {
            if (!CurrentWeapon.MagazineBased) {
                return;
            }
            CurrentWeapon.SetMagazineSize(size);
            CurrentWeapon.SetCurrentAmmo(size);
        }

        /// <summary>
        /// Updates the GUI ammo display during Update() cycle
        /// </summary>
        protected virtual void UpdateAmmoDisplay() {
            if ((GUIManagerIRE_PW.HasInstance) && (_character != null)) {
                // If nothing has changed, do nothing
                if ((_ammoLastFrame == CurrentWeapon.CurrentAmmoLoaded) && (_magSizeLastFrame == CurrentWeapon.MagazineSize)) {
                    return;
                }
                _ammoLastFrame = CurrentWeapon.CurrentAmmoLoaded;
                _magSizeLastFrame = CurrentWeapon.MagazineSize;
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).UpdateAmmoBar(CurrentWeapon.CurrentAmmoLoaded, 0, CurrentWeapon.MagazineSize);
            }
        }

        /// <summary>
        /// Determines if we need to stop air stalling (y velocity changed)
        /// </summary>
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
            CurrentWeapon.SetCurrentAmmo(CurrentWeapon.MagazineSize);
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
            //InputSystemManager_PW.InputActions.PlayerControls.Attack.performed += AttackPerformed;
            //InputSystemManager_PW.InputActions.PlayerControls.Attack.canceled += AttackStopped;
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<RunnerItemPickEvent>();
            //InputSystemManager_PW.InputActions.PlayerControls.Attack.performed -= AttackPerformed;
            //InputSystemManager_PW.InputActions.PlayerControls.Attack.canceled -= AttackStopped;
        }
    }
}
