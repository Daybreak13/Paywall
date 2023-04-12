using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MoreMountains.Tools;
using Paywall.Tools;

namespace Paywall {

    public class StoreButtonNavigator : MonoBehaviour, MMEventListener<MMGameEvent> {
        /// The list of buttons that will be selected when the down button is pressed
        [field: Tooltip("The list of buttons that will be selected when the down button is pressed")]
        [field: SerializeField] public bool SetSpecificButton { get; protected set; }
        /// The list of buttons that will be selected when the down button is pressed
        [field: Tooltip("The list of buttons that will be selected when the down button is pressed")]
        //[field: FieldCondition("SetSpecificButton", true)]
        [field: SerializeField] public List<Button> Buttons { get; protected set; }
        /// The list of grids that contain the buttons that will be selected when the down button is pressed
        [field: Tooltip("The list of grids that contain the buttons that will be selected when the down button is pressed")]
        //[field: FieldCondition("SetSpecificButton", true, true)]
        [field: SerializeField] public List<GridLayoutGroup> Grids { get; protected set; }

        protected Button _thisButton;
        protected Button _activeButton;

        protected virtual void Awake() {
            _thisButton = GetComponent<Button>();
        }

        protected virtual void Start() {
            GetActiveButton();
            SetOnDownSelect();
        }

        protected virtual void GetActiveButton() {
            if (SetSpecificButton) {
                foreach (Button button in Buttons) {
                    if (button.gameObject.activeInHierarchy) {
                        _activeButton = button;
                    }
                }
            } 
            else {
                foreach (GridLayoutGroup grid in Grids) {
                    if (grid.gameObject.activeInHierarchy) {
                        _activeButton = grid.transform.GetChild(0).GetComponent<Button>();
                    }
                }
            }
        }

        public virtual void SetOnDownSelect() {
            Navigation nav = _thisButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = _activeButton;
            _thisButton.navigation = nav;
        }

        public virtual void OnMMEvent(MMGameEvent gameEvent) {
            if (gameEvent.EventName.Equals("MenuChange")) {
                GetActiveButton();
                SetOnDownSelect();
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
