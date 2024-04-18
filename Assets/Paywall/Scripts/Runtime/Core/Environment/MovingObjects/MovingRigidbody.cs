using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;
using System;

namespace Paywall {

    public class MovingRigidbody : MonoBehaviour {
        [field: Header("Settings")]

        /// Speed modifier / denominator applied to LevelSpeed
        [Tooltip("Speed modifier / denominator applied to LevelSpeed")]
        [field: SerializeField] public float Speed { get; protected set; } = 1f;
        /// Movement direction of this enemy
        [Tooltip("Movement direction of this enemy")]
        [field: SerializeField] public Vector3 Direction { get; protected set; } = Vector3.left;
        /// If true, add this Speed modifier to the segment (level) speed
        [Tooltip("If true, add this Speed modifier to the segment (level) speed")]
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
        protected float _stallTime;
        protected Vector2 _knockbackForce;
        protected float _knockbackDecceleration = 32f;  // How fast to deccelerate from knockback velocity
        protected float _snapBackAcceleration = 3f;     // How fast to return to original velocity

        // Divide speed by this to get final velocity
        protected float _speedMult = 10f;

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
                _knockbackDecceleration = -Mathf.Abs(_knockbackDecceleration);
            } else {
                _snapBackAcceleration = Mathf.Abs(_snapBackAcceleration);
                _knockbackDecceleration = Mathf.Abs(_knockbackDecceleration);
            }
            //_knockbackDeceleration *= Mathf.Pow(0.9f, _rigidbody2D.mass - 1);
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
                _speedMult = LevelManagerIRE_PW.Instance.SpeedMultiplier;
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
                _movement = (Speed / _speedMult) * Time.deltaTime * Direction;
            }
            else {
                if (UseEnemySpeed && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress)) {
                    _movement = (Speed / _speedMult) * LevelManagerIRE_PW.Instance.InitialSpeed * Time.deltaTime * Direction;
                } 
                else if (UseEnemySpeed) {
                    _movement = ((Speed + LevelManagerIRE_PW.Instance.SegmentSpeed) / _speedMult) * LevelManagerIRE_PW.Instance.InitialSpeed * Time.deltaTime * Direction;
                }
                else {
                    _movement = (Speed / _speedMult) * LevelManagerIRE_PW.Instance.FinalSpeed * Time.deltaTime * Direction;
                }

                if (BlockMovementUntil && _movementBlockedUntil) {
                    if ((_spawnBarrier != null) && (transform.position.x > _spawnBarrier.transform.position.x)) {
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

        /// <summary>
        /// If the rigidbody has been knocked back, return it to its original velocity over time
        /// </summary>
        protected virtual void HandleKnockback() {
            if (!_knockbackApplied) {
                return;
            }

            // Adjust decceleration according to rigidbody mass
            //  * Mathf.Pow(0.9f, _rigidbody2D.mass - 1)
            float adjustedDecceleration = _knockbackDecceleration;

            _currentKnockbackTime += Time.fixedDeltaTime;

            // Apply knockback return acceleration if applicable
            if (_currentKnockbackTime <= Time.fixedDeltaTime) {
                //return;
            }

            Vector2 levelSpeed = (LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            Vector2 speedCap = levelSpeed;

            Vector2 newVelocity;
            // If we are still traveling backwards (in knockback), deccelerate the knockback velocity until it equals speed cap
            if (_rigidbody2D.velocity.x > speedCap.x) {
                newVelocity = new(_rigidbody2D.velocity.x + (adjustedDecceleration * Time.fixedDeltaTime), _rigidbody2D.velocity.y);
                if (newVelocity.x <= speedCap.x) {
                    newVelocity = speedCap;
                    _stallTime = 0;
                }
            }
            // Otherwise, apply return velocity acceleration
            else {
                _stallTime += Time.fixedDeltaTime;
                // Pause before applying return velocity
                if (_stallTime >= 0.7f) {
                    newVelocity = new(_rigidbody2D.velocity.x + (_snapBackAcceleration * Time.fixedDeltaTime), _rigidbody2D.velocity.y);
                } else {
                    newVelocity = new(speedCap.x, _rigidbody2D.velocity.y);
                }
            }

            // If the velocity exceeds or equals original velocity, stop acceleration
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
                _movement = Speed * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }
            else if (UseEnemySpeed) {
                _movement = (Speed + LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }
            else {
                _movement = (Speed + LevelManagerIRE_PW.Instance.Speed) * LevelManagerIRE_PW.Instance.SpeedMultiplier * Direction;
            }

            // If knockback is applied, HandleKnockback controls the velocity instead of this function
            if (_knockbackApplied) {
                HandleKnockback();
                return;
            }

            if (BlockMovementUntil && _movementBlockedUntil) {
                if ((_spawnBarrier != null) && (transform.position.x > _spawnBarrier.transform.position.x)) {
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
                // Determine knockback velocity. The higher the rigidbody's mass, the less knockback it takes
                // knockback v = force * (modifier) ^ (mass - 1)
                _knockbackForce = force * Mathf.Pow(0.9f, _rigidbody2D.mass - 1f);

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
