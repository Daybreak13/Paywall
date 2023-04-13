using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System;
using Paywall.Tools;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    public class WeightedPoolSpawner : MonoBehaviour {
		/// the minimum frequency possible, in seconds
		[field: Tooltip("the minimum frequency possible, in seconds")]
		[field: SerializeField] public float MinFrequency { get; protected set; } = 3f;
		/// the maximum frequency possible, in seconds
		[field: Tooltip("the maximum frequency possible, in seconds")]
		[field: SerializeField] public float MaxFrequency { get; protected set; } = 3f;
		/// if true, reset the object's position on spawn
		[field: Tooltip("if true, reset the object's position on spawn")]
		[field: SerializeField] public bool ResetObjectPosition { get; protected set; } = true;
		/// if true, spawn objects at its parent pooler's position
		[field: Tooltip("if true, spawn objects at its parent pooler's position")]
		[field: FieldCondition("ResetObjectPosition", true)]
		[field: SerializeField] public bool UsePoolerPosition { get; protected set; } = true;
		/// the object pooler associated to this spawner
		[field: Tooltip("the object pooler associated to this spawner")]
		[field: SerializeField] public WeightedObjectPooler ObjectPooler { get; protected set; } 

		protected float _lastSpawnTimestamp = 0f;
		protected float _nextFrequency = 0f;

		/// <summary>
		/// On Start we initialize our spawner
		/// </summary>
		protected virtual void Start() {
			Initialization();
		}

		/// <summary>
		/// Grabs the associated object pooler if there's one, and initalizes the frequency
		/// </summary>
		protected virtual void Initialization() {
			if (GetComponent<WeightedObjectPooler>() != null) {
				ObjectPooler = GetComponent<WeightedObjectPooler>();
			}
			if (GetComponentInChildren<WeightedObjectPooler>() != null) {
				ObjectPooler = GetComponentInChildren<WeightedObjectPooler>();
			}
			if (ObjectPooler == null) {
				Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
				return;
			}
			DetermineNextFrequency();
		}

		/// <summary>
		/// Every frame we check whether or not we should spawn something
		/// </summary>
		protected virtual void Update() {
			if (Time.timeSinceLevelLoad - _lastSpawnTimestamp > _nextFrequency) {
				Spawn();
			}
		}

		/// <summary>
		/// Spawns an object out of the pool if there's one available.
		/// If it's an object with Health, revives it too.
		/// </summary>
		protected virtual void Spawn() {
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

			// mandatory checks
			if (nextGameObject == null) { return; }
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

			// we activate the object
			nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

			// we check if our object has an Health component, and if yes, we revive our character
			Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health>();
			if (objectHealth != null) {
				objectHealth.Revive();
			}

			// we reset our timer and determine the next frequency
			_lastSpawnTimestamp = Time.time;
			DetermineNextFrequency();
		}

		/// <summary>
		/// Determines the next frequency by randomizing a value between the two specified in the inspector.
		/// </summary>
		protected virtual void DetermineNextFrequency() {
			_nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
		}
	}
}
