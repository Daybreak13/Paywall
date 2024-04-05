using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Paywall.Tools;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Paywall {

    /// <summary>
    /// Class storing data references for a button so that other classes can retrieve the data easily without GetComponent
    /// Extends the built in Button UI class
    /// </summary>
    public class ButtonDataReference : Button {
        [field: Header("Button Reference")]

        /// The image component of this button
        [field: Tooltip("The image component of this button")]
        [field: SerializeField] public Image ImageComponent { get; protected set; }
        /// The text component of this button
        [field: Tooltip("The text component of this button")]
        [field: SerializeField] public TextMeshProUGUI TextComponent { get; protected set; }
        /// The button select controller component of this button
        [field: Tooltip("The button select controller component of this button")]
        [field: SerializeField] public ButtonSelectController ButtonSelect { get; protected set; }
        /// The container for this button, reference this instead of gameobject when setting active state
        [field: Tooltip("The container for this button, reference this instead of gameobject when setting active state")]
        [field: SerializeField] public GameObject Container { get; protected set; }
        /// The event to fire when the button is selected
        [field: Tooltip("The event to fire when the button is selected")]
        [field: SerializeField] public UnityEvent OnSelectEvent { get; protected set; }

        protected Color _originalColor;
        protected bool _initialized;

        protected override void Awake() {
            base.Awake();
            Initialization();
        }

        protected virtual void Initialization() {
            if (_initialized) {
                return;
            }
            _originalColor = ImageComponent.color;
            _initialized = true;
        }

        public virtual void SetColor(Color color) {
            Initialization();
            ImageComponent.color = color;
        }

        public virtual void ResetColor() {
            Initialization();
            ImageComponent.color = _originalColor;
        }

        public virtual void SetImage(Sprite sprite) {
            ImageComponent.sprite = sprite;
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            OnSelectEvent?.Invoke();
        }
    }
}
