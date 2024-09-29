using UnityEngine;

namespace Paywall
{

    public class CharacterAbilityIRE : MonoBehaviour
    {
        /// Is the ability enabled
        [field: Tooltip("Is the ability enabled")]
        [field: SerializeField] public bool AbilityPermitted { get; protected set; } = true;

        public bool AbilityInitialized { get { return _initialized; } }
        public IREInputActions InputActions { get; protected set; }
        public CharacterIRE Character { get; protected set; }

        protected bool _initialized;
        protected Animator _animator;
        protected Rigidbody2D _rigidbody2D;
        protected BoxCollider2D _boxCollider;
        protected float _initialGravityScale;

        /// <summary>
        /// Is the ability allowed to be used
        /// </summary>
        public bool AbilityAuthorized {
            get {
                if (Character != null)
                {
                    if (Character.ConditionState.CurrentState != CharacterStates_PW.ConditionStates.Normal)
                    {
                        return false;
                    }
                }
                if (GameManagerIRE_PW.HasInstance)
                {
                    if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status != GameManagerIRE_PW.GameStatus.GameInProgress)
                    {
                        return false;
                    }
                }
                return AbilityPermitted;
            }

        }

        protected virtual void Awake()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            if (_initialized) return;
            if (Character == null)
            {
                Character = GetComponentInParent<CharacterIRE>();
            }
            if (Character != null)
            {
                _animator = Character.CharacterAnimator;
                _rigidbody2D = Character.CharacterRigidBody;
                _boxCollider = Character.CharacterBoxCollider;
                _initialGravityScale = _rigidbody2D.gravityScale;
                _initialized = true;
            }
            InputActions = new();
        }

        public virtual void SetCharacter(CharacterIRE character)
        {
            Character = character;
        }

        /// <summary>
        /// Sets the ability permission to on or off
        /// </summary>
        /// <param name="permit"></param>
        public virtual void PermitAbility(bool permit)
        {
            AbilityPermitted = permit;
        }

        protected virtual void InternalHandleInput()
        {
            if (Character.CharacterType == CharacterTypes.AI)
            {
                return;
            }
            HandleInput();
        }

        /// <summary>
        /// Handles user input. To be overridden.
        /// </summary>
        protected virtual void HandleInput()
        {

        }

        /// <summary>
        /// Early update. To be overridden
        /// </summary>
        public virtual void EarlyProcessAbility()
        {
            InternalHandleInput();
        }

        /// <summary>
        /// Update. To be overridden.
        /// </summary>
        public virtual void ProcessAbility()
        {

        }

        /// <summary>
        /// Late update. To be overridden
        /// </summary>
        public virtual void LateProcessAbility()
        {

        }

        /// <summary>
        /// Update animator every frame
        /// </summary>
        public virtual void UpdateAnimator()
        {

        }

        /// <summary>
        /// Resets abilities. Usually called on player out of bounds death.
        /// </summary>
        public virtual void ResetAbility()
        {

        }

        protected virtual void OnEnable()
        {
            Initialization();
            InputActions.Enable();
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            InputActions.Disable();
        }

        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
        }

    }
}
