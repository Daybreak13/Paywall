using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Paywall
{

    /// <summary>
    /// Add this component to a store button to assist with UI navigation
    /// Only the top bar buttons should have this component
    /// </summary>
    public class StoreButtonNavigator : MonoBehaviour, MMEventListener<MMGameEvent>
    {
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

        protected virtual void Awake()
        {
            _thisButton = GetComponent<Button>();
        }

        protected virtual void Start()
        {
            GetActiveButton();
            SetSelectOnDown();
        }

        /// <summary>
        /// Get the first active button of the upgrade buttons grid
        /// </summary>
        protected virtual void GetActiveButton()
        {
            if (SetSpecificButton)
            {
                foreach (Button button in Buttons)
                {
                    if (button.gameObject.activeInHierarchy)
                    {
                        _activeButton = button;
                    }
                }
            }
            else
            {
                foreach (GridLayoutGroup grid in Grids)
                {
                    if (grid.gameObject.activeInHierarchy)
                    {
                        _activeButton = grid.transform.GetChild(0).GetComponent<Button>();
                    }
                }
            }
        }

        /// <summary>
        /// Set the button's navigation mode to explicit, and set the SelectOnDown object
        /// </summary>
        public virtual void SetSelectOnDown()
        {
            Navigation nav = _thisButton.navigation;
            //nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = _activeButton;
            _thisButton.navigation = nav;
        }

        /// <summary>
        /// When the upgrade button grid is changed to a different one (upgrade page change), set the new navigation
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName.Equals("MenuChange"))
            {
                GetActiveButton();
                SetSelectOnDown();
            }
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
