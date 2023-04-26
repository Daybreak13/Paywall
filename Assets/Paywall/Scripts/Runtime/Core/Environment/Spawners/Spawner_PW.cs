using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using System;
using Paywall.Tools;

namespace Paywall {

	/// <summary>
	/// Weighted pool spawner
	/// </summary>
    public class Spawner_PW : MonoBehaviour {
		[Header("Size")]

		/// the minimum size of a spawned object
		public Vector3 MinimumSize = new Vector3(1, 1, 1);
		/// the maximum size of a spawned object
		public Vector3 MaximumSize = new Vector3(1, 1, 1);
		/// if set to true, the random size will preserve the original's aspect ratio
		public bool PreserveRatio = false;

		[Header("Rotation")]

		/// the minimum size of a spawned object
		public Vector3 MinimumRotation;
		/// the maximum size of a spawned object
		public Vector3 MaximumRotation;

		[Header("When can it spawn?")]

		/// if true, the spawner can spawn, if not, it won't spawn
		public bool Spawning = true;
		/// if true, only spawn objects while the game is in progress
		public bool OnlySpawnWhileGameInProgress = true;
		/// Initial delay before the first spawn, in seconds.
		public float InitialDelay = 0f;

		[field: Header("Other Settings")]

		/// if true, reset the object's position on spawn
		[field: Tooltip("if true, reset the object's position on spawn")]
		[field: SerializeField] public bool ResetObjectPosition { get; protected set; } = true;
		/// if true, spawn objects at its parent pooler's position
		[field: Tooltip("if true, spawn objects at its parent pooler's position")]
		[field: FieldCondition("ResetObjectPosition", true)]
		[field: SerializeField] public bool UsePoolerPosition { get; protected set; } = true;
		/// the object pooler associated with this spawner
		[field: Tooltip("the object pooler associated with this spawner")]
		[field: SerializeField] public WeightedObjectPooler ObjectPooler { get; protected set; }

		protected float _startTime;
		protected WeightedObjectPooler _weightedObjectPooler;

        protected virtual void Awake() {
			_startTime = Time.time;
			if (ObjectPooler == null) {
				if (!TryGetComponent(out _weightedObjectPooler)) {
					_weightedObjectPooler = GetComponentInChildren<WeightedObjectPooler>();
				}
			} else {
				_weightedObjectPooler = ObjectPooler;
            }
        }

		/// <summary>
		/// Same as base except the object pooler is replaced with WeightedObjectPooler
		/// Also resets Health component
		/// </summary>
		/// <param name="spawnPosition"></param>
		/// <param name="triggerObjectActivation"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
        public virtual GameObject Spawn(Vector3 spawnPosition, bool triggerObjectActivation = true) {
			// if the spawner can only spawn while the game is in progress, we wait until we're in that state
			if (OnlySpawnWhileGameInProgress) {
				if (MoreMountains.InfiniteRunnerEngine.GameManager.Instance.Status != MoreMountains.InfiniteRunnerEngine.GameManager.GameStatus.GameInProgress) {
					return null;
				}
			}

			if ((Time.time - _startTime < InitialDelay) || (!Spawning)) {
				return null;
			}

			/// we get the next object in the pool and make sure it's not null
			GameObject nextGameObject = _weightedObjectPooler.GetPooledGameObject();
			if (nextGameObject == null) {
				return null;
			}

			return ResizeRepositionObject(nextGameObject, spawnPosition, triggerObjectActivation);
		}

		public virtual GameObject ResizeRepositionObject(GameObject nextGameObject, Vector3 spawnPosition, bool triggerObjectActivation = true) {
			/// we rescale the object
			Vector3 newScale;
			if (!PreserveRatio) {
				newScale = new Vector3(UnityEngine.Random.Range(MinimumSize.x, MaximumSize.x), UnityEngine.Random.Range(MinimumSize.y, MaximumSize.y), UnityEngine.Random.Range(MinimumSize.z, MaximumSize.z));
			}
			else {
				newScale = Vector3.one * UnityEngine.Random.Range(MinimumSize.x, MaximumSize.x);
			}
			nextGameObject.transform.localScale = newScale;

			// we adjust the object's position based on its renderer's size
			if (nextGameObject.GetComponent<MMPoolableObject>() == null) {
				throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
			}


			// we position the object
			if (UsePoolerPosition) {
				nextGameObject.transform.position = nextGameObject.transform.parent.transform.parent.position;
			}
			else {
				nextGameObject.transform.position = this.transform.position;
			}

			// we set the object's rotation
			nextGameObject.transform.eulerAngles = new Vector3(
				UnityEngine.Random.Range(MinimumRotation.x, MaximumRotation.x),
				UnityEngine.Random.Range(MinimumRotation.y, MaximumRotation.y),
				UnityEngine.Random.Range(MinimumRotation.z, MaximumRotation.z)
				);

			// we activate the object
			nextGameObject.gameObject.SetActive(true);

			if (triggerObjectActivation) {
				if (nextGameObject.GetComponent<MMPoolableObject>() != null) {
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
				foreach (Transform child in nextGameObject.transform) {
					if (child.gameObject.GetComponent<ReactivateOnSpawn>() != null) {
						child.gameObject.GetComponent<ReactivateOnSpawn>().Reactivate();
					}
				}
			}

			// we check if our object has an Health component, and if yes, we revive our character
			Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health>();
			if (objectHealth != null) {
				objectHealth.Revive();
			}

			return (nextGameObject);
		}

    }
}
