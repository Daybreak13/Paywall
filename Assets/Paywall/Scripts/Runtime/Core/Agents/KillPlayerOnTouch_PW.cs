using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

	/// <summary>
	/// Add this class to a trigger boxCollider and it'll kill all playable characters that collide with it.
	/// </summary>
	public class KillPlayerOnTouch_PW : MonoBehaviour {
		protected const string _playerHurtboxTag = "PlayerHurtbox";
		protected const string _playerTag = "Player";

		/// <summary>
		/// Handles the collision if we're in 2D mode
		/// </summary>
		/// <param name="other">the Collider2D that collides with our object</param>
		protected virtual void OnTriggerEnter2D(Collider2D other) {
			TriggerEnter(other.gameObject);
		}

		protected virtual void OnCollisionEnter2D(Collision2D collision) {
			if (!GetComponent<Collider2D>().isTrigger) {
				TriggerEnter(collision.collider.gameObject);
			}
        }

		/// <summary>
		/// Triggered when we collide with either a 2D or 3D collider
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void TriggerEnter(GameObject collidingObject) {
			// we verify that the colliding object is a PlayableCharacter with the Player tag. If not, we do nothing.			
			if (!collidingObject.CompareTag(_playerHurtboxTag)) {
				return;
			}

			PlayableCharacter player = collidingObject.GetComponent<PlayableCharacter>();
			if (player == null) {
				return;
			}

			if (player.Invincible) {
				return;
			}

			// we ask the LevelManager to kill the character
			(LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).KillCharacter(player);
		}

	}
}
