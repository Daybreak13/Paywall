using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;

namespace Paywall {

	/// <summary>
	/// Recycles a poolable object so it can be pulled again
	/// </summary>
	[RequireComponent(typeof(MMPoolableObject))]
    public class OutOfBoundsRecycle_PW : MonoBehaviour {
		/// Distance past the bounds to destroy this object
		[field: Tooltip("Distance past the bounds to destroy this object")]
		[field: SerializeField] public float DestroyDistanceBehindBounds { get; protected set; } = 1f;

		protected const string _tag = "LevelSegment";

		/// <summary>
		/// On update, if the object meets the level's recycling conditions, we recycle it
		/// </summary>
		protected virtual void Update() {
			if (LevelManagerIRE_PW.Instance.CheckRecycleCondition(GetComponent<MMPoolableObject>().GetBounds(), DestroyDistanceBehindBounds)) {
				GetComponent<MMPoolableObject>().Destroy();
				if (gameObject.CompareTag(_tag)) {
					ProceduralLevelGenerator.Instance.DecrementActiveObjects();
				}
			}
		}
	}
}
