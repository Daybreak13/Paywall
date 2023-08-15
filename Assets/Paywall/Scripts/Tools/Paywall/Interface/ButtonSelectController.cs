using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paywall.Tools {

    /// <summary>
    /// Fires an event when the button this component is attached to is selected
    /// </summary>
    public class ButtonSelectController : MonoBehaviour, ISelectHandler {
        /// The event to fire when the button is selected
        [field: Tooltip("The event to fire when the button is selected")]
        [field: SerializeField] public UnityEvent OnSelectEvent { get; protected set; }

        public virtual void OnSelect(BaseEventData eventData) {
            OnSelectEvent?.Invoke();
        }
    }
}
