using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paywall {

    /// Class containing dialogue lines
    [System.Serializable]
    public class DialogueUILines {
        [TextArea]
        public string Line;
        public string CharacterName;
        public Image Portrait;
    }

    /// <summary>
    /// Plays lines of dialogue
    /// </summary>
    public class DialogueManager : MonoBehaviour {
        [Header("Properties")]
        /// If true, set dialogue canvas to inactive at start
        [Tooltip("If true, set dialogue canvas to inactive at start")]
        public bool DisableOnStart = true;
        /// The speed at which text is displayed, in characters per second
        [Tooltip("The speed at which text is displayed, in characters per second")]
        public float TextSpeed = 30f;
        /// If true, requires the player to hit a button to advance dialogue. Otherwise, advance automatically.
        [Tooltip("If true, requires the player to hit a button to advance dialogue. Otherwise, advance automatically.")]
        public bool RequireInput;
        /// If advancing automatically, the delay before advancing.
        [Tooltip("If advancing automatically, the delay before advancing.")]
        [MMCondition("RequireInput", true, true)]
        public float AdvanceDelay = 1.5f;

        [Header("Components")]
        /// The canvas group containing the dialogue components
        [Tooltip("The canvas group containing the dialogue components")]
        public GameObject DialogueCanvasGroup;
        /// Dialogue screen background (fullscreen)
        [Tooltip("Dialogue screen background (fullscreen)")]
        public GameObject Background;
        /// Text object displaying the name of the character speaking
        [Tooltip("Text object displaying the name of the character speaking")]
        public TextMeshProUGUI NameDisplay;
        /// Image object displaying the portrait of the character speaking
        [Tooltip("Image object displaying the portrait of the character speaking")]
        public GameObject Portrait;
        /// The textmeshpro component
        [Tooltip("The textmeshpro component for the dialogue canvas")]
        public TextMeshProUGUI DialogueText;

        [Header("Dialogue Lines")]
        /// The list of text lines to be typed out
        [Tooltip("The list of text lines to be typed out")]
        [MMReadOnly]
        public List<DialogueUILines> TextLines = new List<DialogueUILines>();

        public IREInputActions InputActions;

        // Does the game pause for this dialogue
        protected bool _pausedDialogue;
        // Is the currently typing text done typing
        protected bool _textComplete;
        // The index of currently displayed dialogue in TextLines
        protected int _currentLine;
        // The input system manager
        protected InputSystemManager _inputSystemManager;
        // Typing coroutine
        protected Coroutine _typingCoroutine;
        protected bool _playingDialogue;

        protected virtual void Awake() {
            InputActions = new();
            foreach (Transform child in DialogueCanvasGroup.transform) {
                //child.gameObject.SetActive(false);
            }
        }

        protected virtual void Start() {
            _inputSystemManager = FindObjectOfType<InputSystemManager>();
            if (DisableOnStart) {
                DialogueCanvasGroup.SetActive(false);
            }
        }

        /// <summary>
        /// Advances the dialogue. If the dialogue is not done typing, complete the dialogue. Otherwise, go to the next set of dialogue.
        /// </summary>
        protected virtual void Advance() {
            // If the line is done playing, advance to the next line
            if (_textComplete) {
                // If there are no more lines, end the dialogue
                if (_currentLine >= TextLines.Count) {
                    CloseDialogue();
                }
                // Else, play next line
                else {
                    if (!RequireInput && (AdvanceDelay > 0f)) {
                        StartCoroutine(WaitToAdvance());
                    }
                    else {
                        _typingCoroutine = StartCoroutine(TypeCharacters());
                    }
                }
                _currentLine++;
            }
            // Reveal the rest of the dialogue line immediately
            else {
                StopAllCoroutines();
                DialogueText.maxVisibleCharacters = DialogueText.text.Length;
                _currentLine++;
                _textComplete = true;
            }
        }

        /// <summary>
        /// Wait to advance to the next line of dialogue
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator WaitToAdvance() {
            yield return new WaitForSeconds(AdvanceDelay);
            _typingCoroutine = StartCoroutine(TypeCharacters());
        }

        protected virtual void Skip() {

        }

        /// <summary>
        /// Plays given list of dialogue lines
        /// </summary>
        /// <param name="lines"></param>
        public virtual void OpenDialogue(List<DialogueUILines> lines) {
            if (_playingDialogue) {
                StopAllCoroutines();
            }
            _playingDialogue = true;
            _currentLine = 0;
            TextLines = lines;
            InputActions.Enable();
            DialogueCanvasGroup.SetActive(true);
            _typingCoroutine = StartCoroutine(TypeCharacters());
        }

        /// <summary>
        /// Stops all current dialogue and disables the dialogue canvas
        /// Also called once dialogue is over
        /// </summary>
        public virtual void CloseDialogue() {
            _playingDialogue = false;
            StopAllCoroutines();
            EventSystem.current.sendNavigationEvents = false;
            DialogueCanvasGroup.SetActive(false);
            MMGameEvent.Trigger("dialogueCloses");
            InputActions.Disable();
        }

        /// <summary>
        /// Types out a given dialogue line
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TypeCharacters() {
            _textComplete = false;
            if (TextSpeed <= 0) {
                TextSpeed = 30f;
            }
            NameDisplay.text = TextLines[_currentLine].CharacterName;
            DialogueText.text = TextLines[_currentLine].Line;

            int totalVisibleCharacters = DialogueText.text.Length;
            int counter = 0;
            // Uses tmp.maxVisibleCharacters to reveal the text (typing effect) one char at a time
            while (counter <= totalVisibleCharacters) {
                DialogueText.maxVisibleCharacters = counter;
                counter += 1;
                yield return new WaitForSeconds(1f / TextSpeed);
            }
            //_currentLine++;
            _textComplete = true;
        }

        protected virtual void OnEnable() {
            if (RequireInput) {
                InputActions.UI.Submit.started += context => Advance();
            }
        }

        protected virtual void OnDisable() {
            InputActions.Disable();
            InputActions.UI.Submit.started -= context => Advance();
        }
    }
}
