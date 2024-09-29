using UnityEngine.InputSystem;

namespace Paywall
{

    /// <summary>
    /// Attach this component to a character to allow them to pause the game
    /// </summary>
    public class CharacterPauseIRE : CharacterAbilityIRE
    {
        /// <summary>
        /// Handle input callback
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void HandleCallback(InputAction.CallbackContext ctx)
        {
            PerformAbility();
        }

        /// <summary>
        /// Perform pause ability
        /// </summary>
        protected virtual void PerformAbility()
        {
            if (GameManagerIRE_PW.HasInstance)
            {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Pause();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InputSystemManager_PW.InputActions.PlayerControls.Pause.performed += HandleCallback;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            InputSystemManager_PW.InputActions.PlayerControls.Pause.performed -= HandleCallback;
        }

    }
}
