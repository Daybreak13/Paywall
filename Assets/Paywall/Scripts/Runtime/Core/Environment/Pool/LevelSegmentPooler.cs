using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace Paywall {

	/// <summary>
	/// Pooler specifically for LevelSegments
	/// Used by ProceduralLevelGenerator
	/// </summary>
    public class LevelSegmentPooler : MMObjectPooler {

		/// the game object we'll instantiate 
		[field: Tooltip("the game object we'll instantiate")]
		[field: SerializeField] public LevelSegmentController SegmentToPool { get; protected set; } 
		/// the number of objects we'll add to the pool
		[field: Tooltip("the number of objects we'll add to the pool")]
		[field: SerializeField] public int PoolSize { get; protected set; } = 3;
		/// if true, the pool will automatically add objects to the itself if needed
		[field: Tooltip("if true, the pool will automatically add objects to the itself if needed")]
		[field: SerializeField] public bool PoolCanExpand { get; protected set; } = true;

		/// the actual object pool
		protected List<GameObject> _pooledGameObjects;

		public List<LevelSegmentPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }

		/// <summary>
		/// Fills the object pool with the gameobject type you've specified in the inspector
		/// </summary>
		public override void FillObjectPool() {
			if (SegmentToPool == null) {
				return;
			}

			// if we've already created a pool, we exit
			if ((_objectPool != null) && (_objectPool.PooledGameObjects.Count > PoolSize)) {
				return;
			}

			CreateWaitingPool();

			// we initialize the list we'll use to 
			_pooledGameObjects = new List<GameObject>();

			int objectsToSpawn = PoolSize;

			if (_objectPool != null) {
				objectsToSpawn -= _objectPool.PooledGameObjects.Count;
				_pooledGameObjects = new List<GameObject>(_objectPool.PooledGameObjects);
			}

			// we add to the pool the specified number of objects
			for (int i = 0; i < objectsToSpawn; i++) {
				AddOneObjectToThePool();
			}
		}

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected override string DetermineObjectPoolName() {
			return ("[LevelSegmentPooler] " + SegmentToPool.name);
		}

		/// <summary>
		/// This method returns one inactive object from the pool
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject() {
			// we go through the pool looking for an inactive object
			for (int i = 0; i < _pooledGameObjects.Count; i++) {
				if (!_pooledGameObjects[i].gameObject.activeInHierarchy) {
					// if we find one, we return it
					return _pooledGameObjects[i];
				}
			}
			// if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
			if (PoolCanExpand) {
				return AddOneObjectToThePool();
			}
			// if the pool is empty and can't grow, we return nothing.
			return null;
		}

		/// <summary>
		/// Adds one object of the specified type (in the inspector) to the pool.
		/// </summary>
		/// <returns>The one object to the pool.</returns>
		protected virtual GameObject AddOneObjectToThePool() {
			if (SegmentToPool == null) {
				Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any SegmentToPool defined.", gameObject);
				return null;
			}

			bool initialStatus = SegmentToPool.gameObject.activeSelf;
			SegmentToPool.gameObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(SegmentToPool.gameObject);
			SegmentToPool.gameObject.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool) {
				newGameObject.transform.SetParent(_waitingPool.transform);
			}
			newGameObject.name = SegmentToPool.name + "-" + _pooledGameObjects.Count;

			_pooledGameObjects.Add(newGameObject);

			_objectPool.PooledGameObjects.Add(newGameObject);

			return newGameObject;
		}
	}
}
