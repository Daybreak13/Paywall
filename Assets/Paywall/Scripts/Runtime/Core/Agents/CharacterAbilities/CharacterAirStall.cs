

using MoreMountains.Tools;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Slows fall speed when jump button is held while falling
    /// </summary>
    public class CharacterAirStall : CharacterAbilityIRE, MMEventListener<PaywallModuleEvent>, MMEventListener<MMGameEvent> {
        /// The max amount of continuous time that stalling can occur
		[field: Tooltip("The max amount of continuous time that stalling can occur")]
        [field: SerializeField] public float MaxStallTime { get; protected set; } = 4f;
        /// The max fall speed allowed while stalling
		[field: Tooltip("The max fall speed allowed while stalling")]
        [field: SerializeField] public float MaxFallSpeed { get; protected set; } = 0.1f;
        /// Rate at which fall speed decelerates to MaxFallSpeed
		[field: Tooltip("Rate at which fall speed decelerates to MaxFallSpeed")]
        [field: SerializeField] public float Deceleration { get; protected set; } = 4f;
        /// Module SO corresponding to this ability
		[field: Tooltip("Module SO corresponding to this ability")]
        [field: SerializeField] public ScriptableModule StallModule { get; protected set; }

        protected bool _stalling;
        protected bool _stallingEnabled;        // Stalling is disabled when the duration has run out. Reenabled when grounded
        protected bool _stallThisFrame;
        protected float _stallStartTime = float.MaxValue;
        protected float _stallTime;

        protected float MaxFallSpeedInternal { get { return -Mathf.Abs(MaxFallSpeed); } }

        /// <summary>
        /// Do we have a valid stall input
        /// </summary>
        protected bool ValidInput {
            get {
                return (InputSystemManager_PW.InputActions.PlayerControls.Jump.IsPressed()
                && !_character.Grounded && _stallingEnabled);
            }
        }

        /// <summary>
        /// If jump button is being held, we stall
        /// </summary>
        protected override void HandleInput() {
            base.HandleInput();
            if (!AbilityAuthorized) {
                return;
            }
            if (_character.Grounded) { 
                _stallingEnabled = true; 
                return; 
            }

            if (ValidInput) {
                if (!_stalling && _character.CharacterRigidBody.velocity.y < 0) { InitiateStall(); }
            }
            else if (_stalling) {
                _stalling = false;
            }
        }

        /// <summary>
        /// Begin air stall
        /// </summary>
        protected virtual void InitiateStall() {
            _stallTime = 0f;
            _stallStartTime = Time.time;
            _stallThisFrame = true;
            _stalling = true;
        }

        public override void LateProcessAbility() {
            base.LateProcessAbility();
            HandleAirStall();
        }

        /// <summary>
        /// Since we're working with Rigidbody, fixed update is used to keep in line with physics updates
        /// Set rigidbody velocity when stalling
        /// </summary>
        protected virtual void FixedUpdate() {
            if (_stalling) {
                if (_character.CharacterRigidBody.velocity.y < MaxFallSpeedInternal) {
                    _character.CharacterRigidBody.velocity = new(_character.CharacterRigidBody.velocity.x, MaxFallSpeedInternal);
                }
            }
        }

        /// <summary>
        /// Handles air stall in LateProcessAbility each frame
        /// </summary>
        protected virtual void HandleAirStall() {
            if (!_stalling) {
                return;
            }

            if (_stallThisFrame) {
                _stallThisFrame = false;
                return;
            }

            if (_character.CharacterRigidBody.velocity.y < 0) {
                _stallTime += Time.deltaTime;
            }

            if (_stallTime > MaxStallTime) {
                _stalling = false;
                _stallingEnabled = false;
            }
        }

        public virtual void OnMMEvent(PaywallModuleEvent moduleEvent) {
            if (moduleEvent.Module.Name.Equals(StallModule.Name)) {
                if (PaywallProgressManager.Instance.ModulesDict[moduleEvent.Module.Name].IsActive) {
                    AbilityPermitted = true;
                }
                else {
                    AbilityPermitted = false;
                }
            }
        }

        public virtual void OnMMEvent(MMGameEvent gameEvent) {
            if (gameEvent.EventName.Equals("Jump")) {
                _stallingEnabled = true;
                _stalling = false;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.MMEventStartListening<PaywallModuleEvent>();
            this.MMEventStartListening<MMGameEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<PaywallModuleEvent>();
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
