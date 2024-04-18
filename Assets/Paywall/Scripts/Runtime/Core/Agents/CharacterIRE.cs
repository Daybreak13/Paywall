using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Paywall.Tools;

namespace Paywall {

    public class CharacterIRE : MonoBehaviour_PW {
        /// Type of character this is
		[field: Tooltip("Type of character this is")]
        [field: SerializeField] public CharacterTypes CharacterType { get; protected set; } = CharacterTypes.AI;

        [field: Header("Sprite")]

        /// The character's sprite renderer. If left blank, component will use GetComponent to find it
        [field: Tooltip("The character's sprite renderer. If left blank, component will use GetComponent to find it")]
        [field: SerializeField] public SpriteRenderer Model { get; protected set; }
        /// Flicker the sprite when hit
        [field: Tooltip("Flicker the sprite when hit")]
        [field: SerializeField] public bool FlickerSpriteOnHit { get; protected set; }
        /// Flicker duration. Sprite flickers between flicker color and normal color, this is the duration it will rest on either one before flickering to the other color.
        [field: Tooltip("Flicker duration. Sprite flickers between flicker color and normal color, this is the duration it will rest on either one before flickering to the other color.")]
        [field: FieldCondition("FlickerSpriteOnHit", true)]
        [field: SerializeField] public float FlickerDuration { get; protected set; } = 0.1f;

        [field: Header("Animator")]

        /// should we use the default mecanim ?
        [field: Tooltip("should we use the default mecanim ?")]
        [field: SerializeField] public bool UseDefaultMecanim { get; protected set; } = true;

        [field: Header("Position")]

        /// returns true if the character is currently grounded
        // if true, the object will try to go back to its starting position
        [field: Tooltip("if true, the object will try to go back to its starting position")]
        [field: SerializeField] public bool ShouldResetPosition { get; protected set; } = true;
        // the speed at which the object should try to go back to its starting position
        [field: Tooltip("the speed at which the object should try to go back to its starting position")]
        [field: SerializeField] public float ResetPositionSpeed { get; protected set; } = 0.5f;

        [field: Header("Grounded")]

        /// the distance between the character and the ground
        [field: Tooltip("the distance between the character and the ground")]
        [field: MMReadOnly]
        [field: SerializeField] public float DistanceToTheGround { get; protected set; }
        /// the distance tolerance at which a character is considered grounded
        [field: Tooltip("the distance tolerance at which a character is considered grounded")]
        [field: SerializeField] public float GroundDistanceTolerance { get; protected set; } = 0.05f;

        [field: Header("Invincibility")]

        /// the duration (in seconds) of invincibility on spawn
        [field: Tooltip("the duration (in seconds) of invincibility on spawn")]
        [field: SerializeField] public float InitialInvincibilityDuration { get; protected set; } = 3f;

        /// The duration of the invincibility which activates upon taking damage/spawning
        [field: Tooltip("The duration of the invincibility which activates upon taking damage/spawning")]
        [field: SerializeField] public float TempInvincibilityDuration { get; protected set; } = 1f;
        /// Is the character currently invincible
        [field: Tooltip("Is the character currently invincible")]
        [field: SerializeField] public bool Invincible { get; protected set; }

        // State Machines
        /// the movement state machine 
        public MMStateMachine<CharacterStates_PW.MovementStates> MovementState { get; protected set; }
        /// the condition state machine
        public MMStateMachine<CharacterStates_PW.ConditionStates> ConditionState { get; protected set; }

        public bool Grounded { get; protected set; }
        public GameObject Ground { get { return _ground; } }
        public float DistanceToRight { get; protected set; }
        [field: MMReadOnly]
        [field: SerializeField] public bool CollidingRight { get; protected set; }
        public Animator CharacterAnimator { get; protected set; }

        protected float _distanceToTheGroundRaycastLength = 10f;
        protected GameObject _ground;
        protected LayerMask _collisionMaskSave;
        protected float _awakeAt;
        protected GameObject _rightObject;

        protected Vector3 _raycastLeftOrigin;
        protected Vector3 _raycastRightOrigin;

        protected bool _initialInvincibilityActive = true;
        protected bool _postDamageInvincible;

        protected bool _initialized;
        protected float _remainingInvincibility;
        protected float _initialGravity;
        protected Color _initialColor;
        protected Color _flickerColor;
        protected Coroutine _flickerCoroutine;

        public Rigidbody2D CharacterRigidBody { get; protected set; }
        public BoxCollider2D CharacterBoxCollider { get; protected set; }

        protected Vector2 _boundsTopLeftCorner;
        protected Vector2 _boundsTopRightCorner;
        protected Vector2 _boundsBottomLeftCorner;
        protected Vector2 _boundsBottomRightCorner;
        protected Vector2 _boundsCenter;

        protected const string _playerIgnoreTag = "PlayerIgnore";

        CharacterAbilityIRE[] _characterAbilities;

        protected virtual void Awake() {
            if (!_initialized) {
                Initialization();
            }
        }

        protected virtual void Initialization() {
            if (Model == null) {
                Model = GetComponentInChildren<SpriteRenderer>();
            }
            _initialColor = _flickerColor = Model.material.color;
            _flickerColor.a = 0.1f;
            MovementState = new MMStateMachine<CharacterStates_PW.MovementStates>(gameObject, true);
            ConditionState = new MMStateMachine<CharacterStates_PW.ConditionStates>(gameObject, true);
            CharacterRigidBody = gameObject.MMGetComponentNoAlloc<Rigidbody2D>();
            _initialGravity = CharacterRigidBody.gravityScale;
            CharacterBoxCollider = gameObject.MMGetComponentNoAlloc<BoxCollider2D>();
            CacheAbilities();
            _initialized = true;
        }

        /// <summary>
        /// Caches list of CharacterAbilities attached to this character
        /// </summary>
        protected virtual void CacheAbilities() {
            _characterAbilities = GetComponentsInChildren<CharacterAbilityIRE>();
            foreach (CharacterAbilityIRE ability in _characterAbilities) {
                ability.SetCharacter(this);
            }
        }

        protected virtual void Update() {

            // we determine the distance between the ground and the character
            ComputeDistanceToTheGround();
            if (Grounded) {
                // If we are in jump startup rigidbody is beginning to move up, don't change movement state out of jumping
                if (!(MovementState.CurrentState == CharacterStates_PW.MovementStates.Jumping
                    && CharacterRigidBody.velocity.y > 0)) {
                    MovementState.ChangeState(CharacterStates_PW.MovementStates.Running);
                }
            }
            else {
                // If we are airborne and moving down, change state to falling
                if (MovementState.CurrentState != CharacterStates_PW.MovementStates.Jumping
                    && CharacterRigidBody.velocity.y < 0) {
                    MovementState.ChangeState(CharacterStates_PW.MovementStates.Falling);
                }
            }

            EarlyProcessAbilities();
            ProcessAbilities();
            LateProcessAbilities();

            HandleInvincibility();

            // we send our various states to the animator.      
            UpdateAnimator();

        }

        protected virtual void EarlyProcessAbilities() {
            foreach (CharacterAbilityIRE ability in _characterAbilities) {
                ability.EarlyProcessAbility();
            }
        }

        protected virtual void ProcessAbilities() {
            foreach (CharacterAbilityIRE ability in _characterAbilities) {
                ability.ProcessAbility();
            }
        }

        protected virtual void LateProcessAbilities() {
            foreach (CharacterAbilityIRE ability in _characterAbilities) {
                ability.LateProcessAbility();
            }
        }

        protected virtual void FixedProcessAbilities() {

        }

        public virtual void ResetCharacter() {
            CharacterRigidBody.gravityScale = _initialGravity;
            CharacterRigidBody.velocity = Vector3.zero;
            ResetAbilities();
        }

        /// <summary>
        /// Reset all character abilities
        /// </summary>
        protected virtual void ResetAbilities() {
            foreach (CharacterAbilityIRE ability in _characterAbilities) {
                ability.ResetAbility();
            }
        }

        /// <summary>
        /// Update rigidbody
        /// </summary>
        protected virtual void FixedUpdate() {
            CheckCollisionRight();
        }

        public virtual void Die() {
            Destroy(this);
        }

        protected virtual bool CheckCollisionRight() {
            Vector2 topOrigin = new(CharacterBoxCollider.bounds.max.x, CharacterBoxCollider.bounds.max.y);
            Vector2 bottomOrigin = new(CharacterBoxCollider.bounds.max.x, CharacterBoxCollider.bounds.min.y);
            int RaysToCast = 5;
            RaycastHit2D raycastHit2D;

            for (int i = 0; i < RaysToCast; i++) {
                Vector2 originPoint = Vector2.Lerp(topOrigin, bottomOrigin, i / (RaysToCast - 1));
                raycastHit2D = Physics2D.Raycast(originPoint, Vector2.right, 1f + CharacterBoxCollider.edgeRadius, 1 << LayerMask.NameToLayer("Ground"));
                if (raycastHit2D.collider != null) {
                    if (raycastHit2D.distance < (GroundDistanceTolerance + CharacterBoxCollider.edgeRadius)) {
                        CollidingRight = true;
                        _rightObject = raycastHit2D.collider.gameObject;
                        if (raycastHit2D.distance <= 0) {
                            //float vx = raycastHit2D.collider.gameObject.GetComponent<>
                            //CharacterRigidBody.velocity = new Vector2(CharacterRigidBody.velocity.y);
                        }
                        return true;
                    }
                }
            }
            CollidingRight = false;
            _rightObject = null;
            return false;
        }

        #region PlayableCharacter overrides

        protected virtual void ComputeDistanceToTheGround() {

            DistanceToTheGround = -1;

            _raycastLeftOrigin = CharacterBoxCollider.bounds.min;
            _raycastRightOrigin = CharacterBoxCollider.bounds.min;
            _raycastRightOrigin.x = CharacterBoxCollider.bounds.max.x;

            // we cast a ray to the bottom to check if we're above ground and determine the distance
            RaycastHit2D raycastLeft = MMDebug.RayCast(_raycastLeftOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
            if (raycastLeft) {
                DistanceToTheGround = raycastLeft.distance;
                _ground = raycastLeft.collider.gameObject;
            }
            RaycastHit2D raycastRight = MMDebug.RayCast(_raycastRightOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
            if (raycastRight) {
                if (raycastLeft) {
                    if (raycastRight.distance < DistanceToTheGround) {
                        DistanceToTheGround = raycastRight.distance;
                        _ground = raycastRight.collider.gameObject;
                    }
                }
                else {
                    DistanceToTheGround = raycastRight.distance;
                    _ground = raycastRight.collider.gameObject;
                }
            }

            if (!raycastLeft && !raycastRight) {
                // if the raycast hasn't hit the ground, we set the distance to -1
                DistanceToTheGround = -1;
                _ground = null;
            }
            if ((CharacterType == CharacterTypes.Player) && (_ground != null) && (_ground.CompareTag(_playerIgnoreTag))) {
                _ground = null;
                Grounded = false;
                return;
            }
            Grounded = DetermineIfGroudedConditionsAreMet();
        }

        /// <summary>
        /// Determines if grouded conditions are met.
        /// </summary>
        /// <returns><c>true</c>, if if grouded conditions are met was determined, <c>false</c> otherwise.</returns>
        protected virtual bool DetermineIfGroudedConditionsAreMet() {
            // if the distance to the ground is equal to -1, this means the raycast never found the ground, thus there's no ground, thus the character isn't grounded anymore
            if (DistanceToTheGround == -1) {
                return (false);
            }
            // if the distance to the ground is within the tolerated bounds, the character is grounded, otherwise it's not.
            if (DistanceToTheGround < (GroundDistanceTolerance + CharacterBoxCollider.edgeRadius)) {
                return (true);
            }
            else {
                return (false);
            }
        }

        /// <summary>
        /// Gets the character bounds.
        /// </summary>
        /// <returns>The character bounds.</returns>
        protected virtual Bounds GetCharacterBounds() {
            if (gameObject.MMGetComponentNoAlloc<Collider>() != null) {
                return gameObject.MMGetComponentNoAlloc<Collider>().bounds;
            }

            if (gameObject.MMGetComponentNoAlloc<Collider2D>() != null) {
                return gameObject.MMGetComponentNoAlloc<Collider2D>().bounds;
            }

            return gameObject.MMGetComponentNoAlloc<Renderer>().bounds;
        }

        /// <summary>
        /// This is called at Update() and sets each of the animators parameters to their corresponding State values
        /// </summary>
        protected virtual void UpdateAnimator() {
            if (CharacterAnimator == null) { return; }

            // we send our various states to the animator.		
            if (UseDefaultMecanim) {
                UpdateAllMecanimAnimators();
            }
        }

        /// <summary>
        /// Updates all mecanim animators.
        /// </summary>
        protected virtual void UpdateAllMecanimAnimators() {
            MMAnimatorExtensions.UpdateAnimatorBoolIfExists(CharacterAnimator, "Grounded", Grounded);
            MMAnimatorExtensions.UpdateAnimatorFloatIfExists(CharacterAnimator, "VerticalSpeed", CharacterRigidBody.velocity.y);
        }

        #endregion

        /// <summary>
        /// External functions can call this to apply force to the character's rigidbody
        /// Use the CharacterJumpIRE component 
        /// </summary>
        /// <param name="force"></param>
        public virtual void ApplyForce(Vector2 force) {
            CharacterRigidBody.AddForce(force * CharacterRigidBody.mass);
        }

        /// <summary>
        /// External functions can call this to set the rigidbody gravity scale
        /// </summary>
        /// <param name="gravityScale"></param>
        public virtual void SetGravityScale(float gravityScale) {
            CharacterRigidBody.gravityScale = gravityScale;
        }

        public virtual void ResetGravityScale() {
            CharacterRigidBody.gravityScale = _initialGravity;
        }

        /// <summary>
        /// Sets the invincibility duration
        /// </summary>
        /// <param name="duration"></param>
        public virtual void SetInvincibilityDuration(float duration) {
            Invincible = true;
            _remainingInvincibility = duration;
        }

        /// <summary>
        /// Toggles invincibility
        /// If we have temp duration invincibility, turn it off
        /// </summary>
        /// <param name="state"></param>
        public virtual void ToggleInvincibility(bool state) {
            _remainingInvincibility = -1f;
            if (_flickerCoroutine != null) {
                StopCoroutine(_flickerCoroutine);
            }
            Invincible = state;
        }

        /// <summary>
        /// Temporary invincibility activated after taking damage. Flicker sprite.
        /// </summary>
        public virtual void ActivateDamageInvincibility() {
            Invincible = true;
            _remainingInvincibility = TempInvincibilityDuration;

            if (!FlickerSpriteOnHit) return;
            if (_flickerCoroutine != null) {
                StopCoroutine(_flickerCoroutine);
            }
            _flickerCoroutine = StartCoroutine(MMImage.Flicker(Model, _initialColor, _flickerColor, FlickerDuration, _remainingInvincibility));
        }

        /// <summary>
        /// Called every frame to handle invincibility frames
        /// If invincibility duration has expired, turn invincibility off
        /// </summary>
        protected virtual void HandleInvincibility() {
            if (_remainingInvincibility > 0) {
                _remainingInvincibility -= Time.deltaTime;
                if (_remainingInvincibility <= 0) {
                    _remainingInvincibility = -1f;
                    Invincible = false;
                }
            }
        }

        #region Raycasts

        protected virtual void CastRaysBelow() {
            SetRaysParameters();
            Vector3 raycastLeftOrigin = CharacterBoxCollider.bounds.min;
            Vector3 raycastRightOrigin = CharacterBoxCollider.bounds.min;
            raycastRightOrigin.x = CharacterBoxCollider.bounds.max.x;
            GameObject hitObject = null;
            float distanceToObject;

            // we cast a ray to the bottom to check if we're above ground and determine the distance
            RaycastHit2D raycastLeft = MMDebug.RayCast(raycastLeftOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
            if (raycastLeft) {
                distanceToObject = raycastLeft.distance;
                hitObject = raycastLeft.collider.gameObject;
            }
            RaycastHit2D raycastRight = MMDebug.RayCast(raycastRightOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
            if (raycastRight) {
                if (raycastLeft) {
                    if (raycastRight.distance < DistanceToTheGround) {
                        distanceToObject = raycastRight.distance;
                        hitObject = raycastRight.collider.gameObject;
                    }
                }
                else {
                    distanceToObject = raycastRight.distance;
                    hitObject = raycastRight.collider.gameObject;
                }
            }

            if (!raycastLeft && !raycastRight) {
                // if the raycast hasn't hit anything, we set the distance to -1
                distanceToObject = -1;
                hitObject = null;
            }

        }

        /// <summary>
        /// Creates a rectangle with the boxcollider's size for ease of use and draws debug lines along the different raycast origin axis
        /// </summary>
        public virtual void SetRaysParameters() {
            float top = CharacterBoxCollider.offset.y + (CharacterBoxCollider.size.y / 2f);
            float bottom = CharacterBoxCollider.offset.y - (CharacterBoxCollider.size.y / 2f);
            float left = CharacterBoxCollider.offset.x - (CharacterBoxCollider.size.x / 2f);
            float right = CharacterBoxCollider.offset.x + (CharacterBoxCollider.size.x / 2f);

            _boundsTopLeftCorner.x = left;
            _boundsTopLeftCorner.y = top;

            _boundsTopRightCorner.x = right;
            _boundsTopRightCorner.y = top;

            _boundsBottomLeftCorner.x = left;
            _boundsBottomLeftCorner.y = bottom;

            _boundsBottomRightCorner.x = right;
            _boundsBottomRightCorner.y = bottom;

            _boundsTopLeftCorner = transform.TransformPoint(_boundsTopLeftCorner);
            _boundsTopRightCorner = transform.TransformPoint(_boundsTopRightCorner);
            _boundsBottomLeftCorner = transform.TransformPoint(_boundsBottomLeftCorner);
            _boundsBottomRightCorner = transform.TransformPoint(_boundsBottomRightCorner);
            _boundsCenter = CharacterBoxCollider.bounds.center;
        }

        #endregion
    }
}
