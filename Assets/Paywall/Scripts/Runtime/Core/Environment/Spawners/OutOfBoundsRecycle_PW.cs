using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;

namespace Paywall {

	[RequireComponent(typeof(MMPoolableObject))]
    public class OutOfBoundsRecycle_PW : MonoBehaviour {
		public float DestroyDistanceBehindBounds = 5f;
		public bool ShouldDecrementActives;

		protected const string _tag = "LevelSegment";

		/// <summary>
		/// On update, if the object meets the level's recycling conditions, we recycle it
		/// </summary>
		protected virtual void Update() {
			if (LevelManager.Instance.CheckRecycleCondition(GetComponent<MMPoolableObject>().GetBounds(), DestroyDistanceBehindBounds)) {
				GetComponent<MMPoolableObject>().Destroy();
				if (gameObject.CompareTag(_tag)) {
					ProceduralLevelGenerator.Instance.DecrementActiveObjects();
				}
			}
		}
	}
}
