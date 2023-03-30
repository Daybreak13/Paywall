using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace Paywall {

	[System.Serializable]
	public class DMMultipleObjectPoolerObject {
		public GameObject GameObjectToPool;
		[Range(0f, 1f)]
		public float Chance;
		public int PoolSize;
		public bool PoolCanExpand = true;
		public bool Enabled = true;
	}

	public class ChancedBasedObjectPooler : MMObjectPooler {
		/// the list of objects to pool
		public List<DMMultipleObjectPoolerObject> Pool;
		[MMInformation("A MultipleObjectPooler is a reserve of objects, to be used by a Spawner. When asked, it will return an object from the pool (ideally an inactive one) chosen based on the pooling method you've chosen.\n- OriginalOrder will spawn objects in the order you've set them in the inspector (from top to bottom)\n- OriginalOrderSequential will do the same, but will empty each pool before moving to the next object\n- RandomBetweenObjects will pick one object from the pool, at random, but ignoring its pool size, each object has equal chances to get picked\n- PoolSizeBased randomly choses one object from the pool, based on its pool size probability (the larger the pool size, the higher the chances it'll get picked)'...", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the chosen pooling method
		public MMPoolingMethods PoolingMethod = MMPoolingMethods.RandomPoolSizeBased;
		[MMInformation("If you set CanPoolSameObjectTwice to false, the Pooler will try to prevent the same object from being pooled twice to avoid repetition. This will only affect random pooling methods, not ordered pooling.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// whether or not the same object can be pooled twice in a row. If you set CanPoolSameObjectTwice to false, the Pooler will try to prevent the same object from being pooled twice to avoid repetition. This will only affect random pooling methods, not ordered pooling.
		public bool CanPoolSameObjectTwice = true;
		/// a unique name that should match on all MMMultipleObjectPoolers you want to use together
		[MMCondition("MutualizeWaitingPools", true)]
		public string MutualizedPoolName = "";

		public List<ChancedBasedObjectPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }

		/// the actual object pool
		protected GameObject _lastPooledObject;

		protected virtual void Start() {
			DMPoolComparer comp = new DMPoolComparer();
			Pool.Sort(comp);
        }

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected override string DetermineObjectPoolName() {
			if ((MutualizedPoolName == null) || (MutualizedPoolName == "")) {
				return ("[ChancedBasedObjectPooler] " + this.name);
			}
			else {
				return ("[ChancedBasedObjectPooler] " + MutualizedPoolName);
			}
		}

		/// <summary>
		/// Fills the object pool with the amount of objects you specified in the inspector.
		/// </summary>
		public override void FillObjectPool() {
			if ((Pool == null) || (Pool.Count == 0)) {
				return;
			}

			// we create a waiting pool, if one already exists, no need to fill anything
			if (!CreateWaitingPool()) {
				return;
			}

			// if there's only one item in the Pool, we force CanPoolSameObjectTwice to true
			if (Pool.Count <= 1) {
				CanPoolSameObjectTwice = true;
			}

			int k = 0;
			// for each type of object specified in the inspector
			foreach (DMMultipleObjectPoolerObject pooledGameObject in Pool) {
				// if there's no specified number of objects to pool for that type of object, we do nothing and exit
				if (k > Pool.Count) { return; }

				// we add, one by one, the number of objects of that type, as specified in the inspector
				for (int j = 0; j < Pool[k].PoolSize; j++) {
					AddOneObjectToThePool(pooledGameObject.GameObjectToPool);
				}
				k++;
			}
		}

		/// <summary>
		/// Adds one object of the specified type to the object pool.
		/// </summary>
		/// <returns>The object that just got added.</returns>
		/// <param name="typeOfObject">The type of object to add to the pool.</param>
		protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject) {
			if (typeOfObject == null) {
				return null;
			}

			bool initialStatus = typeOfObject.activeSelf;
			typeOfObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(typeOfObject);
			typeOfObject.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool) {
				newGameObject.transform.SetParent(_waitingPool.transform);
			}
			newGameObject.name = typeOfObject.name;
			_objectPool.PooledGameObjects.Add(newGameObject);
			return newGameObject;
		}

		/// <summary>
		/// Gets a random object from the pool.
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject() {
			GameObject pooledGameObject;

			pooledGameObject = GetPooledGameObjectChanceBased();

			if (pooledGameObject != null) {
				_lastPooledObject = pooledGameObject;
			}
			else {
				_lastPooledObject = null;
			}
			return pooledGameObject;
		}

		protected virtual GameObject GetPooledGameObjectChanceBased() {
			float spawnChance = Random.value;
			int index = 0;
			DMMultipleObjectPoolerObject searchedObject = null;
			foreach (DMMultipleObjectPoolerObject obj in Pool) {
				if (spawnChance < obj.Chance) {
					searchedObject = obj;
					break;
                }
				index++;
			}

			GameObject findObject = FindInactiveObject(searchedObject.GameObjectToPool.gameObject.name, _objectPool.PooledGameObjects);
			if (findObject != null) {
				return findObject;
			}

			// if its pool can expand, we create a new one
			if (searchedObject.PoolCanExpand) {
				return AddOneObjectToThePool(searchedObject.GameObjectToPool);
			}
			else {
				// if it can't expand we return nothing
				return null;
			}
		}

		protected string _tempSearchedName;

		/// <summary>
		/// Gets an object of the type at the specified index in the Pool.
		/// Note that the whole point of this multiple object pooler is to abstract the various pools and handle
		/// the picking based on the selected mode. If you plan on just picking from different pools yourself,
		/// consider simply having multiple single object poolers.
		/// </summary>
		/// <param name="index"></param>
		public virtual GameObject GetPooledGamObjectAtIndex(int index) {
			if ((index < 0) || (index >= Pool.Count)) {
				return null;
			}

			_tempSearchedName = Pool[index].GameObjectToPool.name;
			return GetPooledGameObjectOfType(_tempSearchedName);
		}

		/// <summary>
		/// Gets an object of the specified name from the pool
		/// Note that the whole point of this multiple object pooler is to abstract the various pools and handle
		/// the picking based on the selected mode. If you plan on just picking from different pools yourself,
		/// consider simply having multiple single object poolers.
		/// </summary>
		/// <returns>The pooled game object of type.</returns>
		/// <param name="type">Type.</param>
		public virtual GameObject GetPooledGameObjectOfType(string searchedName) {
			GameObject newObject = FindInactiveObject(searchedName, _objectPool.PooledGameObjects);

			if (newObject != null) {
				return newObject;
			}
			else {
				// if we've not returned the object, that means the pool is empty (at least it means it doesn't contain any object of that specific type)
				// so if the pool is allowed to expand
				GameObject searchedObject = FindObject(searchedName, _objectPool.PooledGameObjects);
				if (searchedObject == null) {
					return null;
				}

				if (GetPoolObject(FindObject(searchedName, _objectPool.PooledGameObjects)).PoolCanExpand) {
					// we create a new game object of that type, we add it to the pool for further use, and return it.
					GameObject newGameObject = (GameObject)Instantiate(searchedObject);
					SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
					_objectPool.PooledGameObjects.Add(newGameObject);
					return newGameObject;
				}
			}

			// if the pool was empty for that object and not allowed to expand, we return nothing.
			return null;
		}

		/// <summary>
		/// Finds an inactive object in the pool based on its name.
		/// Returns null if no inactive object by that name were found in the pool
		/// </summary>
		/// <returns>The inactive object.</returns>
		/// <param name="searchedName">Searched name.</param>
		protected virtual GameObject FindInactiveObject(string searchedName, List<GameObject> list) {
			for (int i = 0; i < list.Count; i++) {
				// if we find an object inside the pool that matches the asked type
				if (list[i].name.Equals(searchedName)) {
					// and if that object is inactive right now
					if (!list[i].gameObject.activeInHierarchy) {
						// we return it
						return list[i];
					}
				}
			}
			return null;
		}

		protected virtual GameObject FindAnyInactiveObject(List<GameObject> list) {
			for (int i = 0; i < list.Count; i++) {
				// and if that object is inactive right now
				if (!list[i].gameObject.activeInHierarchy) {
					// we return it
					return list[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Finds an object in the pool based on its name, active or inactive
		/// Returns null if there's no object by that name in the pool
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="searchedName">Searched name.</param>
		protected virtual GameObject FindObject(string searchedName, List<GameObject> list) {
			for (int i = 0; i < list.Count; i++) {
				// if we find an object inside the pool that matches the asked type
				if (list[i].name.Equals(searchedName)) {
					// and if that object is inactive right now
					return list[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Returns (if it exists) the MultipleObjectPoolerObject from the original Pool based on a GameObject.
		/// Note that this is name based.
		/// </summary>
		/// <returns>The pool object.</returns>
		/// <param name="testedObject">Tested object.</param>
		protected virtual DMMultipleObjectPoolerObject GetPoolObject(GameObject testedObject) {
			if (testedObject == null) {
				return null;
			}
			int i = 0;
			foreach (DMMultipleObjectPoolerObject poolerObject in Pool) {
				if (testedObject.name.Equals(poolerObject.GameObjectToPool.name)) {
					return (poolerObject);
				}
				i++;
			}
			return null;
		}

		protected virtual bool PoolObjectEnabled(GameObject testedObject) {
			DMMultipleObjectPoolerObject searchedObject = GetPoolObject(testedObject);
			if (searchedObject != null) {
				return searchedObject.Enabled;
			}
			else {
				return false;
			}
		}

		public virtual void EnableObjects(string name, bool newStatus) {
			foreach (DMMultipleObjectPoolerObject poolerObject in Pool) {
				if (name.Equals(poolerObject.GameObjectToPool.name)) {
					poolerObject.Enabled = newStatus;
				}
			}
		}
	}
}
