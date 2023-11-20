using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace Paywall {

    public class ReBindUI : MonoBehaviour {
        [SerializeField]
        private InputActionReference inputActionReference; //this is on the SO

        [SerializeField]
        private bool excludeMouse = true;
        [Range(0, 10)]
        [SerializeField]
        private int selectedBinding;
        [SerializeField]
        private InputBinding.DisplayStringOptions displayStringOptions;
        [Header("Binding Info - DO NOT EDIT")]
        [SerializeField]
        [MMReadOnly]
        private InputBinding inputBinding;
        private int bindingIndex;

        private string actionName;

        [Header("UI Fields")]
        [SerializeField]
        private Text actionText;
        [SerializeField]
        private Button rebindButton;
        [SerializeField]
        private Text rebindText;
        [SerializeField]
        private Button resetButton;

        private void OnEnable() {
            rebindButton.onClick.AddListener(() => DoRebind());
            resetButton.onClick.AddListener(() => ResetBinding());

            if (inputActionReference != null) {
                InputSystemManager_PW.LoadBindingOverride(actionName);
                GetBindingInfo();
                UpdateUI();
            }

            InputSystemManager_PW.rebindComplete += UpdateUI;
            InputSystemManager_PW.rebindCanceled += UpdateUI;
        }

        private void OnDisable() {
            InputSystemManager_PW.rebindComplete -= UpdateUI;
            InputSystemManager_PW.rebindCanceled -= UpdateUI;
        }

        private void OnValidate() {
            if (inputActionReference == null)
                return;

            GetBindingInfo();
            UpdateUI();
        }

        private void GetBindingInfo() {
            if (inputActionReference.action != null)
                actionName = inputActionReference.action.name;

            if (inputActionReference.action.bindings.Count > selectedBinding) {
                inputBinding = inputActionReference.action.bindings[selectedBinding];
                bindingIndex = selectedBinding;
            }
        }

        private void UpdateUI() {
            if (actionText != null)
                actionText.text = actionName;

            if (rebindText != null) {
                if (Application.isPlaying) {
                    rebindText.text = InputSystemManager_PW.GetBindingName(actionName, bindingIndex);
                }
                else
                    rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
            }
        }

        private void DoRebind() {
            InputSystemManager_PW.StartRebind(actionName, bindingIndex, rebindText, excludeMouse);
        }

        private void ResetBinding() {
            InputSystemManager_PW.ResetBinding(actionName, bindingIndex);
            UpdateUI();
        }
    }
}
