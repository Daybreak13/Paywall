using Paywall.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall
{

    public enum Direction { Left, Right, Down, Up }

    /// <summary>
    /// Spawns random breakables in a pattern
    /// </summary>
    public class BreakableChain : MonoBehaviour_PW
    {
        /// What type of breakable to get from poolers
        [field: Tooltip("What type of breakable to get from poolers")]
        [field: SerializeField] public ScriptableSpawnType SpawnablePoolerType { get; protected set; }
        /// Minimum possible length of breakable chain
        [field: Tooltip("Minimum possible length of breakable chain")]
        [field: SerializeField] public int MinLength { get; protected set; } = 3;
        /// Maximum possible length of breakable chain
        [field: Tooltip("Maximum possible length of breakable chain")]
        [field: SerializeField] public int MaxLength { get; protected set; } = 3;
        /// Spawn via script call instead of OnEnable
        [field: Tooltip("Spawn via script call instead of OnEnable")]
        [field: SerializeField] public bool ManualSpawn { get; protected set; }
        /// What direction to spawn breakables from this transform?
        [field: Tooltip("What direction to spawn breakables from this transform?")]
        [field: SerializeField] public Direction SpawnDirection { get; protected set; } = Direction.Right;

        protected List<GameObject> _spawnables = new();
        protected static System.Random _random;
        protected float _spawnIncrement = 1f;

        /// <summary>
        /// Set min and max lengths
        /// Pass in a negative value for min or max to retain the previous length
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public virtual void SetLengths(int min, int max)
        {
            if (min > 0) MinLength = min;
            if (max > 0 && max >= MinLength) MaxLength = max;
        }

        /// <summary>
        /// Call this to manually trigger Spawn()
        /// </summary>
        public virtual Vector2 ForceSpawn()
        {
            if (ManualSpawn) return Spawn();
            return Vector2.zero;
        }

        /// <summary>
        /// Spawn breakables at regular intervals in a horizontal chain with random variable length
        /// Spawn starts offset length / 2, so that the parent transform remains in the middle of the chain
        /// Spawn goes left to right
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Vector2 position of the rightmost border</returns>
        protected virtual Vector2 Spawn(int min = -1, int max = -1)
        {
            int a, b;
            if (min < 0 && max < 0)
            {
                a = MinLength; b = MaxLength;
            }
            else
            {
                a = min; b = max;
            }
            _random ??= RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
            int length = _random.Next(a, b + 1);
            float offset = 0.5f;     // -length / 2f + 0.5f

            Vector2 prevPosition = new(transform.position.x, transform.position.y);
            Vector2 currentPos = Vector2.zero;

            switch (SpawnDirection)
            {
                case Direction.Left:
                    prevPosition.x -= offset;
                    break;
                case Direction.Right:
                    prevPosition.x += offset;
                    break;
                case Direction.Down:
                    prevPosition.y -= offset;
                    break;
                case Direction.Up:
                    prevPosition.y += offset;
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                GameObject spawnable = ProceduralLevelGenerator.Instance.SpawnPoolerDict[SpawnablePoolerType.ID].Pooler.GetPooledGameObject();
                spawnable.SetActive(true);
                spawnable.transform.SetParent(transform);
                _spawnables.Add(spawnable);
                spawnable.transform.position = prevPosition;
                currentPos = prevPosition;
                switch (SpawnDirection)
                {
                    case Direction.Left:
                        prevPosition.x -= _spawnIncrement;
                        break;
                    case Direction.Right:
                        prevPosition.x += _spawnIncrement;
                        break;
                    case Direction.Down:
                        prevPosition.y -= _spawnIncrement;
                        break;
                    case Direction.Up:
                        prevPosition.y += _spawnIncrement;
                        break;
                }
            }

            currentPos.x += 0.5f;
            return currentPos;
        }

        protected virtual void OnEnable()
        {
            if (!ManualSpawn) Spawn();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Vector2 startPosition = new(transform.position.x, transform.position.y);
            Vector2 endPosition = startPosition;
            float offset = 0.5f;
            switch (SpawnDirection)
            {
                case Direction.Left:
                    startPosition.x -= offset;
                    endPosition.x -= MaxLength * _spawnIncrement;
                    break;
                case Direction.Right:
                    startPosition.x += offset;
                    endPosition.x += MaxLength * _spawnIncrement;
                    break;
                case Direction.Down:
                    startPosition.y -= offset;
                    endPosition.y -= MaxLength * _spawnIncrement;
                    break;
                case Direction.Up:
                    startPosition.y += offset;
                    endPosition.y += MaxLength * _spawnIncrement;
                    break;
            }

            Gizmos.DrawWireSphere(startPosition, 0.2f);
            Gizmos.DrawWireSphere(endPosition, 0.2f);
        }
    }
}
