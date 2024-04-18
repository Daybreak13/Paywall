using Paywall.Tools;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Component that manages two ground pieces of varying height in a transition
    /// </summary>
    public class MultiLevelTransition : MonoBehaviour_PW {
        /// Left piece. Reference y position for right piece position. Line this up with LeftBound.
        [field: Tooltip("Left piece. Reference y position for right piece position. Line this up with LeftBound.")]
        [field: SerializeField] public Transform LeftPiece { get; protected set; }
        /// Right piece. Variable height depending on heightdelta
        [field: Tooltip("Right piece. Variable height depending on heightdelta")]
        [field: SerializeField] public Transform RightPiece { get; protected set; }
        /// Parent transition segment controller. Retrieved via GetComponent if not set.
        [field: Tooltip("Parent transition segment controller. Retrieved via GetComponent if not set.")]
        [field: SerializeField] public TransitionSegmentController ParentController { get; protected set; }

        protected virtual void Awake() {
            if (ParentController == null) {
                ParentController = GetComponent<TransitionSegmentController>();
            }
        }

        protected virtual void OnEnable() {
            RightPiece.transform.position = new(RightPiece.position.x,
            LeftPiece.position.y + ParentController.StoredHeightDelta * ProceduralLevelGenerator.Instance.HeightInterval);
        }
    }
}
