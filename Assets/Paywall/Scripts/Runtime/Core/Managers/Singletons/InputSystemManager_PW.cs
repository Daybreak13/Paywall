using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Paywall
{

    /// <summary>
    /// Primary input system manager for runner gameplay
    /// </summary>
    public class InputSystemManager_PW : MonoBehaviour
    {
        // Static Input Actions referenced by any class that needs to take inputs
        public static IREInputActions InputActions { get; protected set; }

        public static event Action RebindComplete;
        public static event Action RebindCanceled;
        public static event Action<InputAction, int> RebindStarted;

        protected bool _initialized;

        protected virtual void Awake()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            if (_initialized) return;
            InputActions ??= new IREInputActions();
            InputActions.Enable();
            _initialized = true;
        }

        protected virtual void Update()
        {
            HandleKeyboard();
        }

        protected virtual void HandleKeyboard()
        {
            if (InputActions.PlayerControls.Jump.WasPressedThisFrame())
            {
                MainActionButtonDown();
            }
        }

        public virtual void MainActionButtonDown()
        {
            if ((LevelManagerIRE_PW.Instance.ControlScheme == LevelManagerIRE_PW.Controls.SingleButton)
                || (LevelManagerIRE_PW.Instance.ControlScheme == LevelManagerIRE_PW.Controls.Swipe))
            {
                if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.GameOver)
                {
                    return;
                }
                if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.LifeLost)
                {
                    LevelManagerIRE_PW.Instance.LifeLostAction();
                    return;
                }
            }
        }

        /// <summary>
        /// Handles pauses when the game is not in progress (that is handled by CharacterPauseIRE)
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void PauseButtonDown(InputAction.CallbackContext ctx)
        {
            if (GameManagerIRE_PW.HasInstance && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress))
            {
                GameManagerIRE_PW.Instance.Pause();
            }
        }

        #region Rebind Controls

        public static void StartRebind(string actionName, int bindingIndex, TextMeshProUGUI statusText, bool excludeMouse)
        {
            InputAction action = InputActions.asset.FindAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Couldn't find action or binding");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isComposite)
                    DoRebind(action, bindingIndex, statusText, true, excludeMouse);
            }
            else
                DoRebind(action, bindingIndex, statusText, false, excludeMouse);
        }

        private static void DoRebind(InputAction actionToRebind, int bindingIndex, TextMeshProUGUI statusText, bool allCompositeParts, bool excludeMouse)
        {
            if (actionToRebind == null || bindingIndex < 0)
                return;

            statusText.text = $"Press a {actionToRebind.expectedControlType}";

            actionToRebind.Disable();

            var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

            rebind.OnComplete(operation => {
                actionToRebind.Enable();
                operation.Dispose();

                if (allCompositeParts)
                {
                    var nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isComposite)
                        DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
                }

                SaveBindingOverride(actionToRebind);
                RebindComplete?.Invoke();
            });

            rebind.OnCancel(operation => {
                actionToRebind.Enable();
                operation.Dispose();

                RebindCanceled?.Invoke();
            });

            rebind.WithCancelingThrough("<Keyboard>/escape");

            if (excludeMouse)
                rebind.WithControlsExcluding("Mouse");

            RebindStarted?.Invoke(actionToRebind, bindingIndex);
            rebind.Start(); //actually starts the rebinding process
        }

        public static string GetBindingName(string actionName, int bindingIndex)
        {
            InputActions ??= new IREInputActions();

            InputAction action = InputActions.asset.FindAction(actionName);
            return action.GetBindingDisplayString(bindingIndex);
        }

        private static void SaveBindingOverride(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
            }
        }

        public static void LoadBindingOverride(string actionName)
        {
            InputActions ??= new IREInputActions();

            InputAction action = InputActions.asset.FindAction(actionName);

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                    action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }

        public static void ResetBinding(string actionName, int bindingIndex)
        {
            InputAction action = InputActions.asset.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Could not find action or binding");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                    action.RemoveBindingOverride(i);
            }
            else
                action.RemoveBindingOverride(bindingIndex);

            SaveBindingOverride(action);
        }

        #endregion

        /// <summary>
        /// On enable we enable our input actions
        /// </summary>
        protected virtual void OnEnable()
        {
            Initialization();
            InputActions.Enable();
            InputActions.PlayerControls.Pause.performed += PauseButtonDown;
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable()
        {
            InputActions.PlayerControls.Pause.performed -= PauseButtonDown;
            InputActions.Disable();
        }
    }
}
