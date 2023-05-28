using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;

namespace Paywall {

    public class MovingRigidbody : MonoBehaviour {
        [field: Header("Settings")]

        /// Movement speed of this enemy
        [Tooltip("Movement speed of this enemy")]
        [field: SerializeField] public float Speed { get; protected set; } = 1f;
        /// Movement direction of this enemy
        [Tooltip("Movement direction of this enemy")]
        [field: SerializeField] public Vector3 Direction { get; protected set; } = Vector3.left;
        /// If true, use the EnemySpeed parameter of LevelManager instead of regular Speed
        [Tooltip("If true, use the EnemySpeed parameter of LevelManager instead of regular Speed")]
        [field: SerializeField] public bool UseEnemySpeed { get; protected set; }
        /// If true, check if this object is past the SpawnBarrier (LevelManager), and don't move until we are past it
        [Tooltip("If true, check if this object is past the SpawnBarrier (LevelManager), and don't move until we are past it")]
        [field: SerializeField] public bool BlockMovementUntil { get; protected set; }

        [field: Header("Behaviour")]

        /// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
        [field: Tooltip("if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.")]
        [field: SerializeField] public bool DirectionCanBeChangedBySpawner { get; protected set; } = true;
        /// the space this object moves into, either world or local
        [field: SerializeField] public Space MovementSpace { get; protected set; } = Space.World;
        /// If true, use rigidbody physics movement instead of transform movement
        [field: Tooltip("If true, use rigidbody physics movement instead of transform movement")]
        [field: SerializeField] public bool UseRigidbody { get; protected set; } = true;
        /// Should we reset velocity if object is knocked back
        [field: Tooltip("Should we reset velocity if object is knocked back")]
        [field: SerializeField] public bool ShouldResetVelocity { get; protected set; } = true;
        /// Speed at which velocity is reset
        [field: Tooltip("Speed at which velocity is reset")]
        [field: FieldCondition("ShouldResetVelocity", true)]
        [field: SerializeField] public float ResetVelocityAcceleration { get; protected set; } = 1f;


        protected Rigidbody2D _rigidbody2D;
        protected Vector2 _movement;
        protected GameObject _spawnBarrier;
        protected float _initialSpeed;
        protected float _currentSpeed;
        protected bool _movementBlockedUntil;

        protected bool _knockbackApplied;
        protected float _currentKnockbackTime;
        protected Vector2 _knockbackForce;
        protected float _knockbackDeceleration = 20f;
        protected float _snapBackAcceleration = 2f;

        // Divide speed by this to get final velocity
        protected float _denominator = 10f;

        protected bool ShouldUseRigidBody {
            get {
                if (UseRigidbody && _rigidbody2D != null) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        protected virtual void Awake() {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _initialSpeed = _currentSpeed = Speed;
            if (BlockMovementUntil) {
                _movementBlockedUntil = true;
            }
            if (Direction.x < 0 || Direction.y < 0) {
                _snapBackAcceleration = -Mathf.Abs(_snapBackAcceleration);
                _knockbackDeceleration = -Mathf.Abs(_knockbackDeceleration);
            } else {
                _snapBackAcceleration = Mathf.Abs(_snapBackAcceleration);
                _knockbackDeceleration = Mathf.Abs(_knockbackDeceleration);
            }
        }

        /// <summary>
        /// Get spawn barrier, set speed
        /// </summary>
        protected virtual void Start() {
            if (LevelManagerIRE_PW.HasInstance) {
                _spawnBarrier = LevelManagerIRE_PW.Instance.SpawnBarrier;
                // If the object is a level segment, use the global speed
                if (gameObject.CompareTag("LevelSegment")) {
                    Speed = LevelManagerIRE_PW.Instance.SegmentSpeed;
                }
            }
        }

        protected virtual void Update() {
            if (ShouldUseRigidBody) {
                return;
            }

            if (_knockbackApplied) {
                return;
            }

            Move();
        }

        /// <summary>
        /// Moves via transform.Translate
        /// </summary>
        protected virtual void Move() {
            if (!LevelManagerIRE_PW.HasInstance) {
                _movement = (Speed / _denominator) * Time.deltaTime * Direction;
            }
            else {
                if (UseEnemySpeed && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress)) {
                    _movement = (Speed / _denominator) * LevelManagerIRE_PW.Instance.InitialSpeed * Time.deltaTime * Direction;
                } 
                else if (UseEnemySpeed) {
                    _movement = ((Speed + LevelManagerIRE_PW.Instance.SegmentSpeed) / _denominator) * LevelManagerIRE_PW.Instance.InitialSpeed * Time.deltaTime * Direction;
                }
                else {
                    _movement = (Speed / _denominator) * LevelManagerIRE_PW.Instance.Speed * Time.deltaTime * Direction;
                }

                if (BlockMovementUntil && _movementBlockedUntil) {
                    if (transform.position.x > _spawnBarrier.transform.position.x) {
                        Speed = 0;
                    }
                    else {
                        Speed = _initialSpeed;
                        _movementBlockedUntil = false;
                    }
                }

            }
            transform.Translate(_movement, MovementSpace);
        }

        /// <summary>
        /// Determine if we are allowed to move, then move
        /// </summary>
        protected virtual void FixedUpdate() {
            if (!ShouldUseRigidBody) {
                return;
            }

            RigidbodyMove();
        }

        protected virtual void HandleKnockback() {
            if (!_knockbackApplied) {
                return;
            }

            // Apply knockback return acceleration if applicable    
            _currentKnockbackTime += Time.fixedDeltaTime;

            Vector2 levelSpeed = (LevelManagerIRE_PW.Instance.SegmentSpeed / _denominator) * LevelManagerIRE_PW.Instance.Speed * Direction;
            Vector2 newVelocity;
            if (_rigidbody2D.velocity.x > 0) {
                newVelocity = new(_rigidbody2D.velocity.x + (_knockbackDeceleration * Time.fixedDeltaTime), _rigidbody2D.velocity.y);
            }
            else {
                newVelocity = new(_rigidbody2D.velocity.x + (_snapBackAcceleration * Time.fixedDeltaTime), _rigidbody2D.velocity.y);
            }

            if (_rigidbody2D.velocity.x <= _movement.x) {
                _knockbackApplied = false;
                _rigidbody2D.velocity = _movement;
            }
            else {
                _rigidbody2D.velocity = newVelocity;
            }            
            
        }

        /// <summary>
        /// Moves this rigidbody at a constant speed based on the Speed parameter and the LevelManager's speed
        /// If it is an enemy, continue moving at relative speed if EnemySpeed is enabled
        /// </summary>
        protected virtual void RigidbodyMove() {
            if (UseEnemySpeed && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress)) {
                _movement = (Speed / _denominator) * LevelManagerIRE_PW.Instance.InitialSpeed * Direction;
            }
            else if (UseEnemySpeed) {
                _movement = ((Speed + LevelManagerIRE_PW.Instance.SegmentSpeed) / _denominator) * LevelManagerIRE_PW.Instance.Speed * Direction;
            }
            else {
                _movement = (Speed / _denominator) * LevelManagerIRE_PW.Instance.Speed * Direction;
            }

            if (_knockbackApplied) {
                HandleKnockback();
                return;
            }

            if (BlockMovementUntil && _movementBlockedUntil) {
                if (transform.position.x > _spawnBarrier.transform.position.x) {
                    Speed = 0;
                }
                else {
                    Speed = _initialSpeed;
                    _movementBlockedUntil = false;
                }
            }
            _rigidbody2D.velocity = new Vector2(_movement.x, _rigidbody2D.velocity.y);
        }

        /// <summary>
        /// Applies knockback force to this object
        /// </summary>
        /// <param name="force"></param>
        public virtual void ApplyKnockback(Vector2 force) {
            if (ShouldUseRigidBody) {
                // Determine knockback velocity
                // force * (3/4) ^ (mass - 1)
                _knockbackForce = force * Mathf.Pow(3f / 4f, _rigidbody2D.mass - 1f);

                // Determine knockback velocity
                float vf = Physics.ElasticCollision(force.x, _rigidbody2D.velocity.x, 1f, _rigidbody2D.mass, ElasticCollisionReturns.VFinal2);

                _rigidbody2D.velocity = new(_knockbackForce.x, _rigidbody2D.velocity.y);
                _knockbackApplied = true;
                _currentKnockbackTime = 0f;

            } 
            else {

            }
        } 

        /// <summary>
        /// Reset movement and block if necessary on disable
        /// </summary>
        protected virtual void OnDisable() {
            _movementBlockedUntil = true;
            if (UseRigidbody) {
                _rigidbody2D.velocity = Vector2.zero;
            }
        }

    }
}
