using System.Collections;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Add this component to character to allow it to drop down from one way platforms
    /// </summary>
    public class CharacterDropDown : CharacterAbilityIRE {
        /// How long to disable collision between the character and the one way platform when dropping down
        [field: Tooltip("How long to disable collision between the character and the one way platform when dropping down")]
        [field: SerializeField] public float IgnoreCollisionDuration { get; protected set; } = 0.2f;

        protected override void HandleInput() {
            base.HandleInput();
            if (!AbilityAuthorized) {
                return;
            }
            if (InputSystemManager_PW.InputActions.PlayerControls.Jump.WasPressedThisFrame()
                    && InputSystemManager_PW.InputActions.PlayerControls.Down.IsPressed()
                    && Character.Ground != null && Character.Ground.layer == PaywallLayerManager.OneWayPlatformsLayer) {
                DropDown();
            }
        }

        /// <summary>
        /// Drop down from one way
        /// </summary>
        protected virtual void DropDown() {
            StartCoroutine(DropDownCo());
        }

        protected virtual IEnumerator DropDownCo() {
            Collider2D platformCollider = Character.Ground.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(Character.CharacterBoxCollider, platformCollider);
            yield return new WaitForSeconds(IgnoreCollisionDuration);
            Physics2D.IgnoreCollision(Character.CharacterBoxCollider, platformCollider, false);
        }
    }
}
