using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Paywall.Tools;
using System;

namespace Paywall {

    /// Class containing one dialogue line and associated data
    [System.Serializable]
    public class DialogueLine {
        [TextArea]
        public string Line;
        public string CharacterName;
        public Image Portrait;
    }

    /// <summary>
    /// Plays lines of dialogue
    /// </summary>
    public class DialogueManager : MonoBehaviour, MMEventListener<PaywallDialogueEvent> {
        [field: Header("Properties")]
        /// If true, set dialogue canvas to inactive at start
        [field: Tooltip("If true, set dialogue canvas to inactive at start")]
        [field: SerializeField] public bool DisableOnStart { get; protected set; } = true;
        /// The speed at which text is displayed, in characters per second
        [field: Tooltip("The speed at which text is displayed, in characters per second")]
        [field: SerializeField] public float TextSpeed { get; protected set; } = 30f;
        /// If true, requires the player to hit a button to advance dialogue. Otherwise, advance automatically.
        [field: Tooltip("If true, requires the player to hit a button to advance dialogue. Otherwise, advance automatically.")]
        [field: SerializeField] public bool RequireInput { get; protected set; }
        /// If advancing automatically, the delay before advancing.
        [field: Tooltip("If advancing automatically, the delay before advancing.")]
        [field: FieldCondition("RequireInput", true, true)]
        [field: SerializeField] public float AdvanceDelay { get; protected set; } = 1.5f;

        [field: Header("Components")]
        /// The canvas group containing the dialogue components
        [field: Tooltip("The canvas group containing the dialogue components")]
        [field: SerializeField] public GameObject DialogueCanvasGroup { get; protected set; }
        /// Dialogue screen background (fullscreen)
        [field: Tooltip("Dialogue screen background (fullscreen)")]
        [field: SerializeField] public GameObject Background { get; protected set; }
        /// Text object displaying the name of the character speaking
        [field: Tooltip("Text object displaying the name of the character speaking")]
        [field: SerializeField] public TextMeshProUGUI NameDisplay { get; protected set; }
        /// Image object displaying the portrait of the character speaking
        [field: Tooltip("Image object displaying the portrait of the character speaking")]
        [field: SerializeField] public GameObject Portrait { get; protected set; }
        /// The textmeshpro component
        [field: Tooltip("The textmeshpro component for the dialogue canvas")]
        [field: SerializeField] public TextMeshProUGUI DialogueText { get; protected set; }

        [field: Header("Dialogue Lines")]
        /// The list of text lines to be typed out
        [field: Tooltip("The list of text lines to be typed out")]
        [field: MMReadOnly]
        [field: SerializeField] public List<DialogueLine> TextLines { get; protected set; } = new List<DialogueLine>();

        public IREInputActions InputActions;

        // Does the game pause for this dialogue
        protected bool _pausedDialogue;
        // Is the currently typing text done typing
        protected bool _textComplete;
        // The index of currently displayed dialogue in TextLines
        protected int _currentLine;
        // Typing coroutine
        protected Coroutine _typingCoroutine;
        // 
        protected bool _playingDialogue;
        protected int _maxChars = 100;

        protected virtual void Awake() {
            InputActions = new();
        }

        protected virtual void Start() {
            if (DisableOnStart) {
                DialogueCanvasGroup.SetActiveIfNotNull(false);
            }
        }

        /// <summary>
        /// Advances the dialogue. If the dialogue is not done typing, complete the dialogue. Otherwise, go to the next set of dialogue.
        /// </summary>
        protected virtual void Advance(InputAction.CallbackContext ctx) {
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
                //_currentLine++;
            }
            // Reveal the rest of the dialogue line immediately
            else {
                StopAllCoroutines();
                DialogueText.maxVisibleCharacters = 99999;
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
        /// Clears current dialogue and plays given list of dialogue lines in order
        /// </summary>
        /// <param name="lines"></param>
        public virtual void OpenDialogue(List<DialogueLine> lines) {
            if (_playingDialogue) {
                StopAllCoroutines();
            }
            _playingDialogue = true;
            _currentLine = 0;
            TextLines = lines;
            if (RequireInput) {
                EventSystem.current.sendNavigationEvents = true;
                InputActions.Enable();
                InputActions.UI.Submit.started += Advance;
            }
            DialogueCanvasGroup.SetActiveIfNotNull(true);
            _typingCoroutine = StartCoroutine(TypeCharacters());
        }

        /// <summary>
        /// Add dialogue lines to the queue of lines to display
        /// </summary>
        /// <param name="lines"></param>
        public virtual void AddDialogue(List<DialogueLine> lines) {
            foreach (DialogueLine line in lines) {
                TextLines.Add(line);
            }
        }

        /// <summary>
        /// Stops all current dialogue and disables the dialogue canvas
        /// Also called once dialogue is over
        /// </summary>
        public virtual void CloseDialogue() {
            _playingDialogue = false;
            TextLines.Clear();
            StopAllCoroutines();
            DialogueCanvasGroup.SetActiveIfNotNull(false);
            if (RequireInput) {
                InputActions.Disable();
                InputActions.UI.Submit.started -= Advance;
            }
            PaywallDialogueEvent.Trigger(DialogueEventTypes.Close, null);
        }

        /// <summary>
        /// Reorganizes the list of dialogue lines to be played. Splits lines that are too long into separate lines.
        /// </summary>
        /// <param name="lines"></param>
        protected virtual void SplitLines(List<DialogueLine> lines) {
            List<DialogueLine> newLines = new();
            foreach (DialogueLine line in lines) {
                if (line.Line.Length > _maxChars) {
                    string remainingLine = line.Line;
                    string newLine;

                    // Split the line until it doesn't exceed max chars
                    while (remainingLine.Length > _maxChars) {
                        int idx = 0;
                        int spaceIndex = 0;

                        // Find the index of the nearest SPC character
                        while (idx < _maxChars) {
                            spaceIndex = line.Line.IndexOf(' ', idx);
                            idx = spaceIndex + 1;
                        }

                        // Add the new line to the list of dialogue lines
                        newLine = remainingLine[..spaceIndex];
                        DialogueLine dialogueLine = line;
                        dialogueLine.Line = newLine;
                        newLines.Add(dialogueLine);

                        remainingLine = remainingLine[spaceIndex..];
                    }

                } else {
                    newLines.Add(line);
                }
            }
            TextLines = newLines;
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
            if ((TextLines[_currentLine].CharacterName != null) && (NameDisplay != null)) {
                NameDisplay.text = TextLines[_currentLine].CharacterName;
            }
            DialogueText.text = TextLines[_currentLine].Line;

            int totalVisibleCharacters = DialogueText.text.Length;
            int counter = 0;
            // Uses tmp.maxVisibleCharacters to reveal the text (typing effect) one char at a time
            while (counter <= totalVisibleCharacters) {
                DialogueText.maxVisibleCharacters = counter;
                counter += 1;
                yield return new WaitForSecondsRealtime(1f / TextSpeed);
            }
            _currentLine++;
            _textComplete = true;
        }

        public virtual void OnMMEvent(PaywallDialogueEvent dialogueEvent) {
            if (dialogueEvent.DialogueEventType == DialogueEventTypes.Open) {
                OpenDialogue(dialogueEvent.DialogueLines);
            } else {

            }
        }

        protected virtual void OnEnable() {
            if (RequireInput) {
                InputActions.Enable();
                InputActions.UI.Submit.started += Advance;
            }
            this.MMEventStartListening<PaywallDialogueEvent>();
        }

        protected virtual void OnDisable() {
            if (RequireInput) {
                InputActions.Disable();
                InputActions.UI.Submit.started -= Advance;
            }
            this.MMEventStopListening<PaywallDialogueEvent>();
        }
    }
}
