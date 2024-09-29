using Paywall.Interfaces;
using UnityEngine;

namespace Paywall
{

    public class LevelBoundsManager : MonoBehaviour, ILevelBoundsManager
    {
        /// <inheritdoc />
        [field: Tooltip("")]
        [field: SerializeField] public Bounds RecycleBounds { get; private set; }
        /// <inheritdoc />
        [field: Tooltip("")]
        [field: SerializeField] public Bounds DeathBounds { get; private set; }
        /// <inheritdoc />
        [field: Tooltip("")]
        [field: SerializeField] public Transform MoveBarrier { get; private set; }
    }
}
