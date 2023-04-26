using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using Paywall.Tools;

namespace Paywall {

    public class Stompable_PW : LaunchPad {
		[field: Header("Stompable")]

		/// If true, this object kills the player on touch
		[field: Tooltip("If true, this object kills the player on touch")]
		[field: SerializeField] public bool KillsPlayer { get; protected set; }
		/// If true, this object kills the player on touch even if stomped on
		[field: Tooltip("If true, this object kills the player on touch even if stomped on")]
		[field: FieldCondition("KillsPlayer", true)]
		[field: SerializeField] public bool KillsPlayerRegardless { get; protected set; }

		protected override void OnCollisionEnter2D(Collision2D collision) {
			base.OnCollisionEnter2D(collision);
			if (collision.collider.CompareTag(_playerTag) && (_player != null)) {
				if (KillsPlayer) {
					// The player made contact with something that hits no matter what, or they made contact without stomping
					if (KillsPlayerRegardless || !_collidingAbove) {
						KillPlayer(collision.gameObject);
					}
				}
				if (_collidingAbove) {
					DestroySelf();
				}
			}
        }

		protected virtual void DestroySelf() {
			gameObject.SetActive(false);
        }

		/// <summary>
		/// Triggered when we collide with either a 2D or 3D collider
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void KillPlayer(GameObject collidingObject) {
			// we verify that the colliding object is a PlayableCharacter with the Player tag. If not, we do nothing.

			if (_player == null) {
				return;
			}

			if (_player.Invincible) {
				return;
			}

			// we ask the LevelManager to kill the character
			(LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).KillCharacter(_player);
		}

	}
}
