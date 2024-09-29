using Paywall.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paywall
{

    /// <summary>
    /// Class storing data references for a button so that other classes can retrieve the data easily without GetComponent
    /// Extends the built in Button UI class
    /// </summary>
    public class ButtonDataReference : Button
    {
        [field: Header("Button Reference")]

        /// The outer image component of this button (outline)
        [field: Tooltip("The outer image component of this button (outline)")]
        [field: SerializeField] public Image OuterImageComponent { get; protected set; }
        /// The inner image component of this button
        [field: Tooltip("The inner image component of this button")]
        [field: SerializeField] public Image InnerImageComponent { get; protected set; }
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

        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        protected virtual void Initialization()
        {
            if (_initialized)
            {
                return;
            }
            _originalColor = InnerImageComponent.color;
            _initialized = true;
        }

        public virtual void SetColor(Color color)
        {
            Initialization();
            InnerImageComponent.color = color;
        }

        public virtual void ResetColor()
        {
            Initialization();
            InnerImageComponent.color = _originalColor;
        }

        public virtual void SetImage(Sprite sprite)
        {
            InnerImageComponent.sprite = sprite;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            OnSelectEvent?.Invoke();
        }
    }
}
