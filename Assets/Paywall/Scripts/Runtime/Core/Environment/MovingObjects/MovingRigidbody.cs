using Paywall.Interfaces;
using Paywall.Tools;
using UnityEngine;
using Zenject;

namespace Paywall
{

    /// <summary>
    /// Add to Rigidbody2D to move it every FixedUpdate
    /// </summary>
    public class MovingRigidbody : MonoBehaviour
    {
        [field: Header("Settings")]

        /// Speed modifier / denominator applied to LevelSpeed
        [Tooltip("Speed modifier / denominator applied to LevelSpeed")]
        [field: SerializeField] public float SpeedModifier { get; protected set; } = 1f;
        /// Movement direction of this enemy
        [Tooltip("Movement direction of this enemy")]
        [field: SerializeField] public Vector3 Direction { get; protected set; } = Vector3.left;
        /// If true, add this Speed modifier to the segment (level) speed
        [Tooltip("If true, add this Speed modifier to the segment (level) speed")]
        [field: SerializeField] public bool UseEnemySpeed { get; protected set; }
        /// If true, check if this object is past the SpawnBarrier (LevelManager), and don't move until we are past it
        [Tooltip("If true, check if this object is past the SpawnBarrier (LevelManager), and don't move until we are past it")]
        [field: SerializeField] public bool BlockMovementUntil { get; protected set; }
        /// This is the value used to determine how much knockback this rigidbody takes
        [Tooltip("This is the value used to determine how much knockback this rigidbody takes")]
        [field: SerializeField] public float KnockbackModifier { get; protected set; } = 5f;
        /// Can we apply a jump force to this?
        [Tooltip("Can we apply a jump force to this?")]
        [field: SerializeField] public bool CanJump { get; protected set; }

        [field: Header("Behaviour")]

        /// Should we reset velocity if object is knocked back
        [field: Tooltip("Should we reset velocity if object is knocked back")]
        [field: SerializeField] public bool ShouldResetVelocity { get; protected set; } = true;
        /// Speed at which velocity is reset
        [field: Tooltip("Speed at which velocity is reset")]
        [field: FieldCondition("ShouldResetVelocity", true)]
        [field: SerializeField] public float ResetVelocityAcceleration { get; protected set; } = 1f;

        protected Rigidbody2D _rigidbody2D;
        protected Vector2 _movement;
        protected Transform _moveBarrier;
        protected float _initialSpeed;
        protected float _currentSpeed;
        protected float _initialGravityScale;
        protected bool _movementBlockedUntil;

        protected bool _knockbackApplied;
        protected float _stallTime;
        protected float _maxStallTime = 0.7f;
        protected Vector2 _knockbackForce;
        protected float _knockbackDecceleration = 20f;  // How fast to deccelerate from knockback velocity
        protected float _snapBackAcceleration = 3f;     // How fast to return to original velocity
        protected Vector2 _levelSpeed;
        protected float _snapBackVelocity;

        // Divide speed by this to get final velocity
        protected float _speedMult = 10f;

        // Dependency injections
        protected ILevelSpeedManager _speedManager;
        protected ILevelBoundsManager _boundsManager;

        /// <summary>
        /// Dependency injection constructor
        /// </summary>
        /// <param name="levelSpeedManager"></param>
        [Inject]
        public void Construct(ILevelSpeedManager speedManager, ILevelBoundsManager boundsManager)
        {
            _speedManager = speedManager;
            _boundsManager = boundsManager;
        }

        /// <summary>
        /// Applies knockback force to this object
        /// </summary>
        /// <param name="force"></param>
        public virtual void ApplyKnockback(Vector2 force)
        {
            // Determine knockback velocity. The higher the rigidbody's knockback mod, the less knockback it takes
            _knockbackForce = force * Mathf.Pow(0.9f, KnockbackModifier - 1f);
            _levelSpeed = (LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;

            //_rigidbody2D.velocity = new(_knockbackForce.x + _levelSpeed.x, _rigidbody2D.velocity.y);
            _knockbackApplied = true;
            _stallTime = 0;
            _snapBackVelocity = 0;
        }

        /// <summary>
        /// Add force to rigidbody to perform a jump of a given height
        /// </summary>
        /// <param name="height"></param>
        public virtual void PerformJump(float height)
        {
            float force = Mathf.Sqrt(height * -2 * (Physics2D.gravity.y * _initialGravityScale)) * _rigidbody2D.mass;
            _rigidbody2D.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Get rigidbody and speed, block movement
        /// </summary>
        protected virtual void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _initialSpeed = _currentSpeed = SpeedModifier;
            _initialGravityScale = _rigidbody2D.gravityScale;
            if (BlockMovementUntil)
            {
                _movementBlockedUntil = true;
            }
        }

        /// <summary>
        /// Get spawn barrier, set speed
        /// </summary>
        protected virtual void Start()
        {
            if (LevelManagerIRE_PW.HasInstance)
            {
                _moveBarrier = LevelManagerIRE_PW.Instance.MoveBarrier;
                // If the object is a level segment, use the global speed
                if (gameObject.CompareTag("LevelSegment"))
                {
                    SpeedModifier = LevelManagerIRE_PW.Instance.SegmentSpeed;
                }
                _speedMult = LevelManagerIRE_PW.Instance.SpeedMultiplier;
            }
        }

        /// <summary>
        /// Determine if we are allowed to move, then move
        /// </summary>
        protected virtual void FixedUpdate()
        {
            RigidbodyMove();
        }

        /// <summary>
        /// If the rigidbody has been knocked back, return it to its original velocity over time
        /// </summary>
        protected virtual void HandleKnockback()
        {
            // Speed cap is level speed
            _levelSpeed = (LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;

            Vector2 newVelocity;
            // If we are currently getting knocked back, deccelerate the knockback velocity until it reaches 0
            if (_knockbackForce.x > 0)
            {
                _knockbackForce.x += _knockbackDecceleration * Time.fixedDeltaTime * Direction.x;
                if (_knockbackForce.x <= 0)
                {
                    _knockbackForce.x = 0;
                    _stallTime = 0;
                }
                newVelocity = new(_knockbackForce.x + _levelSpeed.x, _rigidbody2D.velocity.y);
            }
            else
            {
                _stallTime += Time.fixedDeltaTime;
                // If we are no longer stalling, accelerate back to MovingRigidbody.Speed
                if (_stallTime >= _maxStallTime)
                {
                    _snapBackVelocity += _snapBackAcceleration * Time.fixedDeltaTime * Direction.x;
                }
                // If we are stalling, set speed to the level speed so relative motion is 0
                else
                {
                    _snapBackVelocity = 0;
                }
                newVelocity = new(_snapBackVelocity + _levelSpeed.x, _rigidbody2D.velocity.y);
            }

            _rigidbody2D.velocity = newVelocity;

            float targetSpeed = _levelSpeed.x - SpeedModifier * LevelManagerIRE_PW.Instance.SpeedMultiplier;
            // If the velocity exceeds or equals original velocity, stop acceleration
            if (_rigidbody2D.velocity.x <= targetSpeed)
            {
                _knockbackApplied = false;
                _rigidbody2D.velocity = new(targetSpeed, _rigidbody2D.velocity.y);
            }
        }

        /// <summary>
        /// Moves this rigidbody at a constant speed based on the Speed parameter and the LevelManager's speed
        /// If it is an enemy, continue moving at relative speed if EnemySpeed is enabled
        /// </summary>
        protected virtual void RigidbodyMove()
        {
            // If knockback is applied, HandleKnockback controls the velocity instead of this function
            if (_knockbackApplied)
            {
                HandleKnockback();
                return;
            }

            // If we block until passing the move barrier, set speed to 0
            if (BlockMovementUntil && _movementBlockedUntil)
            {
                if ((_moveBarrier != null) && (transform.position.x > _moveBarrier.position.x))
                {
                    SpeedModifier = 0;
                }
                else
                {
                    SpeedModifier = _initialSpeed;
                    _movementBlockedUntil = false;
                }
            }
            _movement = GetMovement();
            _rigidbody2D.velocity = new Vector2(_movement.x, _rigidbody2D.velocity.y);
        }

        /// <summary>
        /// Get the movement velocity to set the rigidbody to
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 GetMovement()
        {
            Vector2 movement;
            // If the game is paused and we're an enemy, move at our speed without segment speed
            if (UseEnemySpeed && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress))
            {
                movement = SpeedModifier * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }
            // If the game is not paused and we're an enemy, our speed is the segment speed + our speed
            else if (UseEnemySpeed)
            {
                movement = (SpeedModifier + LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }
            // If we're not an enemy, just add the speeds normally
            else
            {
                movement = (SpeedModifier + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }
            return movement;
        }

        /// <summary>
        /// Reset movement and block if necessary on disable
        /// </summary>
        protected virtual void OnDisable()
        {
            if (BlockMovementUntil)
            {
                _movementBlockedUntil = true;
            }
            _rigidbody2D.velocity = Vector2.zero;
        }

    }
}
