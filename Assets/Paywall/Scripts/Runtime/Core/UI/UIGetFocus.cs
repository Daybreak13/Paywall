using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.CorgiEngine;
using UnityEngine.InputSystem;

namespace Paywall {

    /// <summary>
    /// Gets focus on enable, as well as returning focus when the currentSelectedGameObject is null and Navigate button is hit
    /// </summary>
    public class UIGetFocus : MonoBehaviour {
        /// If true, set UI focus to this object when it is enabled
        [field: Tooltip("If true, set UI focus to this object when it is enabled")]
        [field: SerializeField] public bool GetFocusOnEnable { get; protected set; } = true;

        public IREInputActions InputActions;

        protected virtual void GetFocus(InputAction.CallbackContext context) {
            if ((EventSystem.current.currentSelectedGameObject == null)) {
                EventSystem.current.SetSelectedGameObject(this.gameObject, null);
            }
        }

        public virtual void SetFocus() {
            EventSystem.current.SetSelectedGameObject(this.gameObject, null);
        }

        protected virtual void OnEnable() {
            if (InputActions == null) {
                InputActions = new();
            }
            if (GetFocusOnEnable) {
                EventSystem.current.SetSelectedGameObject(this.gameObject, null);
            }
            InputActions.Enable();
            InputActions.UI.Navigate.performed += GetFocus;
        }

        protected virtual void OnDisable() {
            InputActions.Disable();
            InputActions.UI.Navigate.performed -= GetFocus;
        }

    }
}
