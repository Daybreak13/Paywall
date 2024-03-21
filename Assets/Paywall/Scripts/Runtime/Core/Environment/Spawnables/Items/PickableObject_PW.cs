using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

	/// <summary>
	/// Pickable object base class
	/// Needs to be extended
	/// </summary>
    public class PickableObject_PW : MonoBehaviour {
		/// The effect to instantiate when the coin is hit
		[field: Tooltip("The effect to instantiate when the coin is hit")]
		[field: SerializeField] public GameObject PickEffect { get; protected set; }
		/// The FMOD StudioEventEmitter playing the sound effect
		[field: Tooltip("The FMOD StudioEventEmitter playing the sound effect")]
		[field: SerializeField] public StudioEventEmitter FMODStudioEventEmitter { get; protected set; }

		protected GameObject _picker;

        protected virtual void Awake() {
            if (FMODStudioEventEmitter == null) {
                FMODStudioEventEmitter = GetComponent<StudioEventEmitter>();
            }
        }

		/// <summary>
		/// Handles the collision if we're in 2D mode
		/// </summary>
		/// <param name="other">the Collider2D that collides with our object</param>
		protected virtual void OnTriggerEnter2D(Collider2D other) {
			TriggerEnter(other.gameObject);
		}

		/// <summary>
		/// Handles the collision if we're in 3D mode
		/// </summary>
		/// <param name="other">the Collider that collides with our object</param>
		protected virtual void OnTriggerEnter(Collider other) {
			TriggerEnter(other.gameObject);
		}

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		protected virtual void TriggerEnter(GameObject collidingObject) {
			// if what's colliding with this object isn't the player, we do nothing and exit
			if (collidingObject.GetComponent<PlayerCharacterIRE>() == null) {
				return;
			}

			_picker = collidingObject;

			// adds an instance of the effect at the coin's position
			if (PickEffect != null) {
				Instantiate(PickEffect, transform.position, transform.rotation);
			}

			ObjectPicked();
			// we deactivate the gameobject
			gameObject.SetActive(false);
		}

		public virtual void PlayPickSound() {

        }

		/// <summary>
		/// Override this to describe what happens when that object gets picked.
		/// </summary>
		protected virtual void ObjectPicked() {

		}

	}
}
