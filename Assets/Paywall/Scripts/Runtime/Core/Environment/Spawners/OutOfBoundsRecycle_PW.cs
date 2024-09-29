using MoreMountains.Tools;
using UnityEngine;

namespace Paywall
{

    public enum OutOfBoundsTypes { Recycle, Death }

    /// <summary>
    /// Recycles a poolable object so it can be pulled again
    /// </summary>
    [RequireComponent(typeof(MMPoolableObject))]
    public class OutOfBoundsRecycle_PW : MonoBehaviour
    {
        /// Distance past the bounds to destroy this object
        [field: Tooltip("Distance past the bounds to destroy this object")]
        [field: SerializeField] public float DestroyDistanceBehindBounds { get; protected set; } = 1f;
        /// Distance past the bounds to destroy this object
        [field: Tooltip("Distance past the bounds to destroy this object")]
        [field: SerializeField] public OutOfBoundsTypes OutOfBoundType { get; protected set; } = OutOfBoundsTypes.Recycle;

        protected Health_PW _health;
        protected EdgeCollider2D _edgeCollider;
        protected MMPoolableObject _mmpo;

        protected virtual void Awake()
        {
            if (gameObject.CompareTag(PaywallTagManager.EnemyTag))
            {
                TryGetComponent(out _health);
            }
            TryGetComponent(out _edgeCollider);
            TryGetComponent(out _mmpo);
        }

        /// <summary>
        /// On update, if the object meets the level's recycling conditions, we recycle it
        /// </summary>
        protected virtual void Update()
        {
            Bounds bounds;
            if (_edgeCollider != null && _mmpo.BoundsBasedOn == MMObjectBounds.WaysToDetermineBounds.Collider2D)
            {
                bounds = _edgeCollider.bounds;
            }
            else
            {
                bounds = _mmpo.GetBounds();
            }

            if (LevelManagerIRE_PW.Instance.CheckRecycleCondition(bounds, DestroyDistanceBehindBounds, OutOfBoundType))
            {
                GetComponent<MMPoolableObject>().Destroy();

                // Level segments decrement active segment count when recycled
                if (gameObject.CompareTag(PaywallTagManager.LevelSegmentTag))
                {
                    ProceduralLevelGenerator.Instance.DecrementActiveObjects();
                }

                // If an object is pushed OOB by player damage, it should give EX on death
                if (_health != null && _health.CurrentHealth < _health.InitialHealth)
                {
                    if (transform.position.x > LevelManagerIRE_PW.Instance.RecycleBounds.min.x
                        && transform.position.y <= LevelManagerIRE_PW.Instance.RecycleBounds.min.y)
                    {
                        PaywallKillEvent.Trigger(true, gameObject);
                    }
                }
            }
        }
    }
}
