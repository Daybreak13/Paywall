using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

namespace Paywall {

	/// <summary>
	/// Object pooler specifically for spawnables (items, pickups, enemies, etc.)
	/// </summary>
    public class SpawnableObjectPooler : MMObjectPooler {
		/// the game object we'll instantiate 
		[field: Tooltip("the game object we'll instantiate")]
		[field: SerializeField] public SpawnablePoolableObject SpawnableToPool { get; protected set; }
		/// the number of objects we'll add to the pool
		[field: Tooltip("the number of objects we'll add to the pool")]
		[field: SerializeField] public int PoolSize { get; protected set; } = 3;
		/// if true, the pool will automatically add objects to the itself if needed
		[field: Tooltip("if true, the pool will automatically add objects to the itself if needed")]
		[field: SerializeField] public bool PoolCanExpand { get; protected set; } = true;

		/// the actual object pool
		protected List<GameObject> _pooledGameObjects;

		public List<SpawnableObjectPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }

		/// <summary>
		/// Fills the object pool with the gameobject type you've specified in the inspector
		/// </summary>
		public override void FillObjectPool() {
			if (SpawnableToPool == null) {
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
			return ("[SpawnableObjectPooler] " + SpawnableToPool.name);
		}

		/// <summary>
		/// This method returns one inactive object from the pool
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject() {
			// we go through the pool looking for an inactive object
			for (int i = 0; i < _pooledGameObjects.Count; i++) {
				if (!_pooledGameObjects[i].gameObject.activeInHierarchy) {
                    // we check if our object has an Health component, and if yes, we revive our character
                    Health objectHealth = _pooledGameObjects[i].gameObject.MMGetComponentNoAlloc<Health>();
                    if (objectHealth != null) {
                        objectHealth.Revive();
                    }
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
			if (SpawnableToPool == null) {
				Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any SegmentToPool defined.", gameObject);
				return null;
			}

			bool initialStatus = SpawnableToPool.gameObject.activeSelf;
			SpawnableToPool.gameObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(SpawnableToPool.gameObject);
			SpawnableToPool.gameObject.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool) {
				newGameObject.transform.SetParent(_waitingPool.transform);
			}
			newGameObject.name = SpawnableToPool.name + "-" + _pooledGameObjects.Count;

			// Set the object's parent pooler, so that the parent can be reset when it is recycled
			if (newGameObject.TryGetComponent(out SpawnablePoolableObject spawnable)) {
				spawnable.SetPoolerParent(newGameObject.transform.parent);
			}

			_pooledGameObjects.Add(newGameObject);

			_objectPool.PooledGameObjects.Add(newGameObject);

			return newGameObject;
		}
	}
}
