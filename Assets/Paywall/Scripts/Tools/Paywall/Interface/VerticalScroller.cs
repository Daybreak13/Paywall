using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Paywall.Tools {

    public class VerticalScroller : MonoBehaviour {
        [field: Header("Binding")]

        /// The layout group that contain's the scroller's elements
        [Tooltip("The layout group that contain's the scroller's elements")]
        [field: SerializeField] public VerticalLayoutGroup Content { get; protected set; }
		/// The scene's UI camera
		[Tooltip("The scene's UI camera")]
		[field: SerializeField] public Camera UICamera { get; protected set; }

		[field: Header("Optional Bindings")]

        /// The button that moves the scroller up
        [Tooltip("The button that moves the scroller up")]
        [field: SerializeField] public Button UpButton { get; protected set; }
        /// The button that moves the scroller up
        [Tooltip("The button that moves the scroller up")]
        [field: SerializeField] public Button DownButton { get; protected set; }

        [field: Header("Scroller Setup")]

        /// The initial and current index
        [Tooltip("The initial and current index")]
        [field: SerializeField] public int CurrentIndex { get; protected set; } = 0;
        /// The number of items in the scroller that should be moved every time
        [Tooltip("The number of items in the scroller that should be moved every time")]
        [field: SerializeField] public int Pagination { get; protected set; } = 1;
        /// The percentage of distance that, when reached, will stop movement
        [Tooltip("The percentage of distance that, when reached, will stop movement")]
        [field: SerializeField] public float ThresholdInPercent { get; protected set; } = 1f;

        [field: Header("Speed")]
        /// The duration (in seconds) of the scroller's movement 
        [Tooltip("The duration (in seconds) of the scroller's movement ")]
        [field: SerializeField] public float MoveDuration { get; protected set; } = 0.05f;

        [field: Header("Focus")]
        /// Bind here the scroller item that should have focus initially
        [Tooltip("Bind here the scroller item that should have focus initially")]
        [field: SerializeField] public GameObject InitialFocus { get; protected set; }
        /// If this is true, the mouse will be forced back on Start
        [Tooltip("If this is true, the mouse will be forced back on Start")]
        [field: SerializeField] public bool ForceMouseVisible { get; protected set; } = true;

		[field: Header("Navigation")]

		/// If true, set the root object of the explicit navigation
		[Tooltip("If true, set the root object of the explicit navigation")]
		[field: SerializeField] public bool UseExplicitNavigationRoot { get; protected set; }
		/// The top object in the explicit navigation hierarchy
		[Tooltip("The top object in the explicit navigation hierarchy")]
		[field: FieldCondition("UseExplicitNavigationRoot", true)]
		[field: SerializeField] public GameObject ExplicitNavigationRoot { get; protected set; }


		protected float _elementHeight;
        protected int _contentLength = 0;
        protected float _spacing;
        protected Vector2 _initialPosition;
        protected RectTransform _rectTransform;

        protected bool _lerping = false;
        protected float _lerpStartedTimestamp;
        protected Vector2 _startPosition;
        protected Vector2 _targetPosition;

        /// <summary>
		/// On Start we initialize our scroller
		/// </summary>
		protected virtual void Start() {
			StartCoroutine(Initialization());
			if (UseExplicitNavigationRoot && ExplicitNavigationRoot != null) {
				SetNavigation();
			}
        }

        /// <summary>
		/// Initializes the scroller, grabs the rect transform, computes the elements' dimensions, and inits position
		/// sizeDelta will be 0 on first frame, so we wait one frame to grab the size
		/// </summary>
		protected virtual IEnumerator Initialization() {
			yield return new WaitForEndOfFrame();
            _rectTransform = Content.gameObject.GetComponent<RectTransform>();
            _initialPosition = _rectTransform.anchoredPosition;

            // we compute the Content's element width
            _contentLength = 0;
            foreach (Transform tr in Content.transform) {
                _elementHeight = tr.gameObject.GetComponent<RectTransform>().sizeDelta.y;
                _contentLength++;
            }
            _spacing = Content.spacing;

            // we position our scroller at the desired initial index
            _rectTransform.anchoredPosition = DeterminePosition();

            if (InitialFocus != null) {
                EventSystem.current.SetSelectedGameObject(InitialFocus, null);
            }

            if (ForceMouseVisible) {
                Cursor.visible = true;
            }
        }

		/// <summary>
		/// Sets the navigation mode to explicit and sets the selectOn objects
		/// </summary>
		public virtual void SetNavigation() {
			for (int i = 0; i < Content.transform.childCount; i++) {
				Navigation nav = Content.transform.GetChild(i).GetComponent<Button>().navigation;
				nav.mode = Navigation.Mode.Explicit;
				if (i == 0) {
					if (ExplicitNavigationRoot != null) { 
						nav.selectOnUp = ExplicitNavigationRoot.GetComponent<Selectable>();
					}
				} else {
					nav.selectOnUp = Content.transform.GetChild(i - 1).GetComponent<Button>();
                }
				if ((i + 1) < Content.transform.childCount) {
					nav.selectOnDown = Content.transform.GetChild(i + 1).GetComponent<Button>();
                }
				Content.transform.GetChild(i).GetComponent<Button>().navigation = nav;

			}
        }

		/// <summary>
		/// Moves the scroller up.
		/// </summary>
		public virtual void MoveUp() {
			if (!CanMoveUp()) {
				return;
			}
			else {
				CurrentIndex += Pagination;
				MoveToCurrentIndex();
			}
		}

		/// <summary>
		/// Moves the scroller down.
		/// </summary>
		public virtual void MoveDown() {
			if (!CanMoveDown()) {
				return;
			}
			else {
				CurrentIndex -= Pagination;
				MoveToCurrentIndex();
			}
		}

		/// <summary>
		/// Initiates movement to the current index
		/// </summary>
		protected virtual void MoveToCurrentIndex() {
			_startPosition = _rectTransform.anchoredPosition;
			_targetPosition = DeterminePosition();
			_lerping = true;
			_lerpStartedTimestamp = Time.time;
		}

		/// <summary>
		/// Determines the target position based on the current index value.
		/// </summary>
		/// <returns>The position.</returns>
		protected virtual Vector2 DeterminePosition() {
			return _initialPosition - (Vector2.down * CurrentIndex * (_elementHeight + _spacing));
		}

		public virtual bool CanMoveUp() {
			return (CurrentIndex + Pagination < _contentLength);
		}

		/// <summary>
		/// Determines whether this scroller can move down.
		/// </summary>
		/// <returns><c>true</c> if this instance can move down; otherwise, <c>false</c>.</returns>
		public virtual bool CanMoveDown() {
			return (CurrentIndex - Pagination >= 0);
		}

		/// <summary>
		/// On Update we move the scroller if required, and handles button states
		/// </summary>
		protected virtual void Update() {
			if (_lerping) {
				LerpPosition();
			}
			HandleButtons();
			HandleFocus();
		}

		protected virtual void HandleFocus() {
			if (!_lerping && Time.timeSinceLevelLoad > 0.5f) {
				if (EventSystem.current.currentSelectedGameObject != null &&
					EventSystem.current.currentSelectedGameObject.transform.IsChildOf(Content.transform)) {

					float yPos = EventSystem.current.currentSelectedGameObject.transform.position.y;
					Vector3[] worldCorners = new Vector3[4];		// 0: bottom left 1: top left 2: top right: 3: bottom right
					transform.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
					if (yPos < worldCorners[0].y) {
						MoveUp();
                    }
					if (yPos > worldCorners[1].y) {
						MoveDown();
                    }
				}
			}
		}

		/// <summary>
		/// Handles the buttons, enabling and disabling them if needed
		/// </summary>
		protected virtual void HandleButtons() {
			if (UpButton != null) {
				if (CanMoveUp()) {
					UpButton.enabled = true;
				}
				else {
					UpButton.enabled = false;
				}
			}
			if (DownButton != null) {
				if (CanMoveDown()) {
					DownButton.enabled = true;
				}
				else {
					DownButton.enabled = false;
				}
			}
		}

		/// <summary>
		/// Lerps the scroller's position.
		/// </summary>
		protected virtual void LerpPosition() {
			float timeSinceStarted = Time.time - _lerpStartedTimestamp;
			float percentageComplete = timeSinceStarted / MoveDuration;

			_rectTransform.anchoredPosition = Vector2.Lerp(_startPosition, _targetPosition, percentageComplete);

			//When we've completed the lerp, we set _isLerping to false
			if (percentageComplete >= ThresholdInPercent) {
				_lerping = false;
			}
		}

	}
}
