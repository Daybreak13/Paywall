using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    public class MovingRigidbody : MonoBehaviour {
        /// Movement speed of this enemy
        [Tooltip("Movement speed of this enemy")]
        [field: SerializeField] public float Speed { get; protected set; } = 1f;
        /// Movement direction of this enemy
        [Tooltip("Movement direction of this enemy")]
        [field: SerializeField] public Vector3 Direction { get; protected set; } = Vector3.left;
        /// If true, use the EnemySpeed parameter of LevelManager instead of regular Speed
        [Tooltip("If true, use the EnemySpeed parameter of LevelManager instead of regular Speed")]
        [field: SerializeField] public bool UseEnemySpeed { get; protected set; }

        protected Rigidbody2D _rigidbody2D;
        Vector2 _movement;

        protected virtual void Awake() {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate() {
            //Move();
            SetVelocity();
        }

        public virtual void Move() {
            _movement = (Speed / 10f) * LevelManager.Instance.Speed * Time.fixedDeltaTime * Direction;
            _movement += _rigidbody2D.position;
            _rigidbody2D.MovePosition(_movement);
        }

        public virtual void SetVelocity() {
            _movement = (1f / 10f) * LevelManager.Instance.Speed * Direction;
            _rigidbody2D.velocity = new Vector2(_movement.x, _rigidbody2D.velocity.y);
        }
    }
}
