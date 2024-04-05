using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using System;
using UnityEngine.SceneManagement;
using Paywall.Tools;

namespace Paywall {

    /// <summary>
    /// Death types a player can suffer
    /// </summary>
    public enum DeathTypes { Health, OutOfBounds }

    /// <summary>
    /// Handles distance traveled, level speed, playable character, loading levels
    /// </summary>
    public class LevelManagerIRE_PW : Singleton_PW<LevelManagerIRE_PW>, MMEventListener<MMGameEvent> {
        public enum Controls { SingleButton, LeftRight, Swipe }

        #region Property Fields

        /// The current speed the level is traveling at
        [field: Tooltip("The current speed the level is traveling at")]
        [field: SerializeField] public float Speed { get; protected set; }
        /// The current speed the level is traveling at
        [field: Tooltip("The current speed the level is traveling at")]
        [field: MMReadOnly]
        [field: SerializeField] public float EnemySpeed { get; protected set; }
        /// The distance traveled since the start of the level
        [field: Tooltip("The distance traveled since the start of the level")]
        [field: SerializeField] public float DistanceTraveled { get; protected set; }

        /// the prefab you want for your player
        [field: Header("Prefabs")]

        [field: SerializeField] public GameObject StartingPosition { get; protected set; }
        /// the list of playable characters - use this to tell what characters you want in your level, don't access that at runtime
        [field: SerializeField] public List<PlayerCharacterIRE> PlayableCharacters { get; protected set; }
        /// the list of playable characters currently instantiated in the game - use this to know what characters ARE currently in your level at runtime
        [field: SerializeField] public List<PlayerCharacterIRE> CurrentPlayableCharacters { get; protected set; }
        /// the x distance between each character
        [field: SerializeField] public float DistanceBetweenCharacters { get; protected set; } = 1f;
        /// the elapsed time since the start of the level
        [field: SerializeField] public float RunningTime { get; protected set; }
        /// the amount of points a player gets per unit distance traveled
        [field: SerializeField] public float PointsPerUnit { get; protected set; } = 1f;

        [field: Space(10)]
        [field: Header("Level Bounds")]
        /// the line after which objects can be recycled
        [field: SerializeField] public Bounds RecycleBounds { get; protected set; }

        [field: Space(10)]
        /// the line after which playable characters will die - leave it to zero if you don't want to use it
        [field: SerializeField] public Bounds DeathBounds { get; protected set; }
        [field: Space(10)]
        [field: SerializeField] public GameObject SpawnBarrier { get; protected set; }

        [field: Space(10)]
        [field: Header("Speed")]
        /// the initial speed of the level
        [field: SerializeField] public float InitialSpeed { get; protected set; } = 10f;
        /// the maximum speed the level will run at
        [field: SerializeField] public float MaximumSpeed { get; protected set; } = 50f;
        /// the maximum speed the level will run at
        [field: SerializeField] public float SpeedIncrement { get; protected set; } = 7.5f;
        /// Current speed not counting temp speed modifiers
        [field: MMReadOnly]
        [field: SerializeField] public float CurrentUnmodifiedSpeed { get; protected set; }
        /// the global speed modifier for level segments
        [field: Tooltip("the global speed modifier for level segments")]
        [field: SerializeField] public float SegmentSpeed { get; protected set; } = 1f;

        [field: Space(10)]
        [field: Header("Intro and Outro durations")]
        /// duration of the initial fade in
        [field: SerializeField] public float IntroFadeDuration { get; protected set; } = 1f;
        /// duration of the fade to black at the end of the level
        [field: SerializeField] public float OutroFadeDuration { get; protected set; } = 1f;


        [field: Space(10)]
        [field: Header("Start")]
        /// the duration (in seconds) of the initial countdown
        [field: SerializeField] public int StartCountdown { get; protected set; }
        /// the text displayed at the end of the countdown
        [field: SerializeField] public string StartText { get; protected set; }
        [field: Space(10)]
        [field: Header("Mobile Controls")]
        /// the mobile control scheme applied to this level
        [field: SerializeField] public Controls ControlScheme { get; protected set; }

        [field: Space(10)]
        [field: Header("Life Lost")]
        /// the effect we instantiate when a life is lost
        [field: SerializeField] public GameObject LifeLostExplosion { get; protected set; }

        #endregion

        // protected stuff
        protected DateTime _started;
        protected float _savedPoints;
        protected float _recycleX;
        protected Bounds _tmpRecycleBounds;

        protected bool _tempSpeedAddedActive;
        protected float _tempSpeedSwitchFactor;
        protected float _temporarySpeedFactorRemainingTime;
        protected float _temporarySavedSpeed;

        protected bool _retainEnemySpeed;
        protected int _coroutineCount = 0;

        protected const string _enterSupplyDepotEventName = "EnterSupplyDepot";

        /// <summary>
        /// Is the temp speed switch active
        /// </summary>
        public bool TempSpeedSwitchActive { get; protected set; }

        /// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start() {
            Speed = CurrentUnmodifiedSpeed = InitialSpeed;
            DistanceTraveled = 0;

            InstantiateCharacters();
            if (ProceduralLevelGenerator.HasInstance) {
                ProceduralLevelGenerator.Instance.Initialize();
            }

            ManageControlScheme();

            // storage
            _savedPoints = (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Points;
            _started = DateTime.UtcNow;
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.BeforeGameStart);
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPointsPerSecond(PointsPerUnit);

            if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW) != null) {
                // set the level name in the GUI
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetLevelName(SceneManager.GetActiveScene().name);
                // fade in
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).FaderOn(false, IntroFadeDuration);
            }

            PrepareStart();
        }

        /// <summary>
		/// Handles everything before the actual start of the game.
		/// </summary>
		protected virtual void PrepareStart() {
            //if we're supposed to show a countdown we schedule it, otherwise we just start the level
            if (StartCountdown > 0) {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.BeforeGameStart);
                StartCoroutine(PrepareStartCountdown());
            }
            else {
                LevelStart();
            }
        }

        /// <summary>
        /// Handles the initial start countdown display
        /// </summary>
        /// <returns>The start countdown.</returns>
        protected virtual IEnumerator PrepareStartCountdown() {
            int countdown = StartCountdown;
            (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetCountdownActive(true);

            // while the countdown is active, we display the current value, and wait for a second and show the next
            while (countdown > 0) {
                if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).CountdownText != null) {
                    (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetCountdownText(countdown.ToString());
                }
                countdown--;
                yield return new WaitForSeconds(1f);
            }

            // when the countdown reaches 0, and if we have a start message, we display it
            if ((countdown == 0) && (StartText != "")) {
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetCountdownText(StartText);
                yield return new WaitForSeconds(1f);
            }

            // we turn the countdown inactive, and start the level
            (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetCountdownActive(false);
            LevelStart();
        }

        /// <summary>
        /// Handles the start of the level : starts the autoincrementation of the score, sets the proper status and triggers the corresponding event.
        /// </summary>
        public virtual void LevelStart() {
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.GameInProgress);
            MMEventManager.TriggerEvent(new MMGameEvent("GameStart"));
        }

        /// <summary>
        /// Instantiates all the playable characters and feeds them to the gameManager
        /// </summary>
        protected virtual void InstantiateCharacters() {
            CurrentPlayableCharacters = new List<PlayerCharacterIRE>();

            if (PlayableCharacters == null) {
                return;
            }

            if (PlayableCharacters.Count == 0) {
                return;
            }

            // for each character in the PlayableCharacters list
            for (int i = 0; i < PlayableCharacters.Count; i++) {
                // we instantiate the corresponding prefab
                PlayerCharacterIRE instance = (PlayerCharacterIRE)Instantiate(PlayableCharacters[i]);
                // we position it based on the StartingPosition point
                instance.transform.position = new Vector3(StartingPosition.transform.position.x + i * DistanceBetweenCharacters, StartingPosition.transform.position.y, StartingPosition.transform.position.z);
                // we set manually its initial position
                instance.SetInitialPosition(instance.transform.position);
                // we feed it to the game manager
                CurrentPlayableCharacters.Add(instance);
            }
            MMEventManager.TriggerEvent(new MMGameEvent("PlayableCharactersInstantiated"));
        }

        /// <summary>
        /// Resets the level : repops dead characters, sets everything up for a new game
        /// </summary>
        public virtual void ResetLevel() {
            InstantiateCharacters();
            PrepareStart();
        }

        /// <summary>
        /// Turns buttons on or off depending on the chosen mobile control scheme
        /// </summary>
        protected virtual void ManageControlScheme() {
            String buttonPath = "";
            switch (ControlScheme) {
                case Controls.SingleButton:
                    buttonPath = "Canvas/MainActionButton";
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW) == null) { return; }
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath) == null) { return; }
                    (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath).gameObject.SetActive(true);
                    break;

                case Controls.LeftRight:
                    buttonPath = "Canvas/LeftRight";
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW) == null) { return; }
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath) == null) { return; }
                    (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath).gameObject.SetActive(true);
                    break;

                case Controls.Swipe:
                    buttonPath = "Canvas/SwipeZone";
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW) == null) { return; }
                    if ((GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath) == null) { return; }
                    (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).transform.Find(buttonPath).gameObject.SetActive(true);
                    break;
            }

        }

        /// <summary>
        /// Update points, increment distance traveled, accelerate level speed, handle speed factor
        /// Only execute if game is in progress
        /// </summary>
        public virtual void Update() {
            if (!_retainEnemySpeed) {
                EnemySpeed = Speed;
            }
            if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
                return;
            }

            _savedPoints = (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Points;
            _started = DateTime.UtcNow;

            // we increment the total distance traveled so far
            DistanceTraveled += Speed / 10f * SegmentSpeed * Time.deltaTime;
            if (GUIManagerIRE_PW.HasInstance) {
                GUIManagerIRE_PW.Instance.RefreshDistance();
            }

            HandleSpeedFactor();

            RunningTime += Time.deltaTime;
            if (!_tempSpeedAddedActive && !TempSpeedSwitchActive && Speed != 0) {
                CurrentUnmodifiedSpeed = Speed;
            }
        }

        /// <summary>
        /// Temporarily add to the current speed
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyAddSpeed(float factor, float duration, bool retainEnemySpeed = false) {
            if (!_tempSpeedAddedActive && !TempSpeedSwitchActive) {
                CurrentUnmodifiedSpeed = Speed;
            }

            if (retainEnemySpeed) {
                _retainEnemySpeed = true;
            }

            _temporarySpeedFactorRemainingTime = duration;

            Speed += factor;
            _tempSpeedAddedActive = true;

            StartCoroutine(TemporarilyAddSpeedCo(factor, duration));
        }

        protected IEnumerator TemporarilyAddSpeedCo(float factor, float duration) {
            _coroutineCount++;
            yield return new WaitForSeconds(duration);
            Speed -= factor;
            _coroutineCount--;
            if (_coroutineCount == 0) {
                _tempSpeedAddedActive = false;
            }
        }

        /// <summary>
        /// Temporarily add to speed. Only one speed switch is active at a time, and must be turned off manually.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="on"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyAddSpeedSwitch(float factor, bool on, bool retainEnemySpeed = false) {
            if (on) {
                if (TempSpeedSwitchActive) {
                    Speed -= _tempSpeedSwitchFactor;
                }

                if (!_tempSpeedAddedActive) {
                    CurrentUnmodifiedSpeed = Speed;
                }

                _retainEnemySpeed = retainEnemySpeed;

                _tempSpeedSwitchFactor = factor;

                Speed += _tempSpeedSwitchFactor;
                TempSpeedSwitchActive = true;
            }
            else if (TempSpeedSwitchActive) {
                TempSpeedSwitchActive = false;
                Speed -= _tempSpeedSwitchFactor;
            }
        }

        /// <summary>
        /// Sets current speed to zero, or restores it
        /// Resets all temp speed modifiers
        /// </summary>
        /// <param name="on"></param>
        public virtual void SetZeroSpeed(bool on, bool retainEnemySpeed) {
            if (on) {
                if (!_tempSpeedAddedActive && !TempSpeedSwitchActive) {
                    CurrentUnmodifiedSpeed = Speed;
                }
                _coroutineCount = 0;
                StopAllCoroutines();
                _retainEnemySpeed = retainEnemySpeed;
                Speed = 0;
                TempSpeedSwitchActive = false;
                _tempSpeedAddedActive = false;
            }
            else {
                Speed = CurrentUnmodifiedSpeed;
                _retainEnemySpeed = false;
            }
        }

        /// <summary>
        /// Handles speed multipliers
        /// </summary>
        protected virtual void HandleSpeedFactor() {
            // Points per second increases/decreases proportionally to the ratio of the current speed to initial speed
            if (Speed > 0) {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPointsPerSecond(PointsPerUnit * (Speed / InitialSpeed));
            } else {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPointsPerSecond(0f);
            }
        }

        public virtual void KillCharacter(PlayerCharacterIRE player) {
            // if we've specified an effect for when a life is lost, we instantiate it at the camera's position
            if (LifeLostExplosion != null) {
                GameObject explosion = Instantiate(LifeLostExplosion);
                explosion.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            }

            // we've just lost a life
            MMEventManager.TriggerEvent(new MMGameEvent("LifeLost"));
            _started = DateTime.UtcNow;
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPoints(_savedPoints);
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).LoseLives(1);

            /// If no more lives, trigger game over
            if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).CurrentLives <= 0) {
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetGameOverScreen(true);
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.GameOver);
                MMEventManager.TriggerEvent(new MMGameEvent("GameOver"));
            } else {
                player.GetComponent<PlayerCharacterIRE>().ActivateDamageInvincibility();
            }
        }

        /// <summary>
        /// Kills the character if it goes out of bounds
        /// </summary>
        /// <param name="player"></param>
        public virtual void KillCharacterOutOfBounds(PlayerCharacterIRE player) {
            SetZeroSpeed(true, false);

            // if we've specified an effect for when a life is lost, we instantiate it at the camera's position
            if (LifeLostExplosion != null) {
                GameObject explosion = Instantiate(LifeLostExplosion);
                explosion.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            }

            // we've just lost a life
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.LifeLost);
            MMEventManager.TriggerEvent(new MMGameEvent("LifeLost"));
            _started = DateTime.UtcNow;
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPoints(_savedPoints);
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).LoseLives(1);
            player.gameObject.SetActive(false);

            if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).CurrentLives <= 0) {
                (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).SetGameOverScreen(true);
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.GameOver);
                MMEventManager.TriggerEvent(new MMGameEvent("GameOver"));
            }
        }

        /// <summary>
		/// Triggered when all lives are lost and you press the main action button
		/// </summary>
		public virtual void GameOverAction() {
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).UnPause();
            GotoLevel(SceneManager.GetActiveScene().name);
        }

        public virtual void LifeLostAction() {
            SetZeroSpeed(false, true);
            Instance.CurrentPlayableCharacters[0].transform.position = StartingPosition.transform.position;
            Instance.CurrentPlayableCharacters[0].ResetCharacter();
            Instance.CurrentPlayableCharacters[0].gameObject.SetActive(true);
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetStatus(GameManagerIRE_PW.GameStatus.GameInProgress);
        }

        /// <summary>
		/// Determines if the object whose bounds are passed as a parameter has to be recycled or not.
		/// </summary>
		/// <returns><c>true</c>, if the object has to be recycled, <c>false</c> otherwise.</returns>
		/// <param name="objectBounds">Object bounds.</param>
		/// <param name="destroyDistance">The x distance after which the object will get destroyed.</param>
		public virtual bool CheckRecycleCondition(Bounds objectBounds, float destroyDistance) {
            _tmpRecycleBounds = RecycleBounds;
            _tmpRecycleBounds.extents += Vector3.one * destroyDistance;

            if (objectBounds.Intersects(_tmpRecycleBounds)) {
                return false;
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// Check if object has left death bounds
        /// </summary>
        /// <param name="objectBounds"></param>
        /// <returns></returns>
        public virtual bool CheckDeathCondition(Bounds objectBounds) {
            if (objectBounds.Intersects(DeathBounds)) {
                return false;
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// Save game and convert points to credits if applicable
        /// </summary>
        /// <param name="levelName"></param>
        /// <param name="save"></param>
        /// <param name="convert"></param>
        public virtual void GotoLevel(string levelName, bool save, bool convert) {
            PaywallLevelEndEvent.Trigger(convert);
            if (save) {
                MMGameEvent.Trigger("Save");
            }
            GotoLevel(levelName);
        }

        /// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName) {
            (GUIManagerIRE_PW.Instance as GUIManagerIRE_PW).FaderOn(true, OutroFadeDuration);
            StartCoroutine(GotoLevelCo(levelName));
        }

        /// <summary>
        /// Waits for a short time and then loads the specified level
        /// </summary>
        /// <returns>The level co.</returns>
        /// <param name="levelName">Level name.</param>
        protected virtual IEnumerator GotoLevelCo(string levelName) {
            if (Time.timeScale > 0.0f) {
                yield return new WaitForSeconds(OutroFadeDuration);
            }
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).UnPause();

            if (string.IsNullOrEmpty(levelName)) {
                MMSceneLoadingManager.LoadScene("StartScreen");
            }
            else {
                MMSceneLoadingManager.LoadScene(levelName);
            }
        }

        /// <summary>
        /// Increase current unmodified level speed by given amount
        /// </summary>
        /// <param name="amount"></param>
        public virtual void IncreaseLevelSpeed(float amount) {
            if (_tempSpeedAddedActive) {
                return;
            }
            CurrentUnmodifiedSpeed += amount;
            Speed += amount;
        }

        /// <summary>
        /// Increment speed and reset temp speed modifiers when leaving depot
        /// Only temp speed increase from supers is retained
        /// </summary>
        protected virtual void LeaveDepot() {
            StopAllCoroutines();
            _tempSpeedAddedActive = false;
            CurrentUnmodifiedSpeed += SpeedIncrement;
            Speed += SpeedIncrement;
        }

        public virtual void OnMMEvent(MMGameEvent gameEvent) {
            if (gameEvent.EventName.Equals(_enterSupplyDepotEventName)) {
                //GameManagerIRE_PW.Instance.Pause();
            }
            // When leaving supply depot, reset temp speeds (except for those from super)
            if (gameEvent.EventName.Equals("LeaveDepot")) {
                LeaveDepot();
            }
        }

        /// <summary>
	    /// Override this if needed
	    /// </summary>
	    protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
        }

        /// <summary>
        /// Override this if needed
        /// </summary>
        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<MMGameEvent>();
        }

    }
}
