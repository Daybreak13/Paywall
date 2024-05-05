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
        /// If the character's simulated x movement is blocked, we slow the level speed to this value
        [field: Tooltip("If the character's simulated x movement is blocked, we slow the level speed to this value")]
        [field: SerializeField] public float BlockedSpeed { get; protected set; } = 0f;
        /// Rate at which we slow to BlockedSpeed when blocked
        [field: Tooltip("Rate at which we slow to BlockedSpeed when blocked")]
        [field: SerializeField] public float BlockedDeceleration { get; protected set; } = 20f;
        /// Rate at which we accelerate to preblock speed after unblocking
        [field: Tooltip("Rate at which we accelerate to preblock speed after unblocking")]
        [field: SerializeField] public float BlockedAcceleration { get; protected set; } = 20f;
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

        // Speed parameter seen by other components, after speed mult
        public float FinalSpeed { get { return Speed * SpeedMultiplier; } }
        public float SpeedMultiplier { get; protected set; } = 0.1f;

        protected Dictionary<Guid, float> _tempSpeeds = new();
        
        protected DateTime _started;
        protected float _savedPoints;
        protected float _recycleX;
        protected Bounds _tmpRecycleBounds;

        protected bool _tempSpeedAddedActive;
        protected float _tempSpeedSwitchFactor;
        protected float _currentAddedSpeed;

        protected int _tempActiveCount = 0;
        protected bool _charBlocking;
        protected float _preBlockSpeed;
        protected float _lastBlockTime;

        protected bool _teleporting;
        protected float _teleportStart;
        protected float _teleportDistance;
        protected float _teleportSpeed;

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

            // storage
            _savedPoints = GameManagerIRE_PW.Instance.Points;
            _started = DateTime.UtcNow;
            GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.BeforeGameStart);
            GameManagerIRE_PW.Instance.SetPointsPerSecond(PointsPerUnit);

            if (GUIManagerIRE_PW.Instance != null) {
                // set the level name in the GUI
                GUIManagerIRE_PW.Instance.SetLevelName(SceneManager.GetActiveScene().name);
                // fade in
                GUIManagerIRE_PW.Instance.FaderOn(false, IntroFadeDuration);
            }

            PrepareStart();
        }

        /// <summary>
        /// Handles everything before the actual start of the game.
        /// </summary>
        protected virtual void PrepareStart() {
            //if we're supposed to show a countdown we schedule it, otherwise we just start the level
            if (StartCountdown > 0) {
                GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.BeforeGameStart);
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
            GUIManagerIRE_PW.Instance.SetCountdownActive(true);

            // while the countdown is active, we display the current value, and wait for a second and show the next
            while (countdown > 0) {
                if (GUIManagerIRE_PW.Instance.CountdownText != null) {
                    GUIManagerIRE_PW.Instance.SetCountdownText(countdown.ToString());
                }
                countdown--;
                yield return new WaitForSeconds(1f);
            }

            // when the countdown reaches 0, and if we have a start message, we display it
            if ((countdown == 0) && (StartText != "")) {
                GUIManagerIRE_PW.Instance.SetCountdownText(StartText);
                yield return new WaitForSeconds(1f);
            }

            // we turn the countdown inactive, and start the level
            GUIManagerIRE_PW.Instance.SetCountdownActive(false);
            LevelStart();
        }

        /// <summary>
        /// Handles the start of the level : starts the autoincrementation of the score, sets the proper status and triggers the corresponding event.
        /// </summary>
        protected virtual void LevelStart() {
            GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.GameInProgress);
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
        /// Update points, increment distance traveled, accelerate level speed, handle speed factor
        /// Only execute if game is in progress
        /// </summary>
        protected virtual void Update() {
            if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
                return;
            }

            _savedPoints = GameManagerIRE_PW.Instance.Points;
            _started = DateTime.UtcNow;

            HandleCharacterBlocked();

            HandleSpeedFactor();

            RunningTime += Time.deltaTime;
            if (!_tempSpeedAddedActive && !TempSpeedSwitchActive && Speed != 0) {
                CurrentUnmodifiedSpeed = Speed;
            }
        }

        protected virtual void FixedUpdate() {
            HandleTempDist();

            DistanceTraveled += (SegmentSpeed + Speed) * SpeedMultiplier * Time.fixedDeltaTime;
            if (GUIManagerIRE_PW.HasInstance) {
                GUIManagerIRE_PW.Instance.RefreshDistance();
            }

        }

        /// <summary>
        /// Handle temporary speed mod based on distance
        /// Used by Portal
        /// </summary>
        protected virtual void HandleTempDist() {
            if (!_teleporting || Mathf.Abs(_teleportStart - DistanceTraveled) < _teleportDistance) {
                return;
            }

            if (_charBlocking) {
                _preBlockSpeed -= _teleportSpeed;
            }
            else {
                Speed -= _teleportSpeed;
            }
            _currentAddedSpeed -= _teleportSpeed;
            _tempActiveCount--;

            if (_tempActiveCount == 0) {
                _tempSpeedAddedActive = false;
            }
            _teleporting = false;
        }

        /// <summary>
        /// Handles speed multipliers
        /// </summary>
        protected virtual void HandleSpeedFactor() {
            // Points per second increases/decreases proportionally to the ratio of the current speed to initial speed
            if (Speed > 0) {
                GameManagerIRE_PW.Instance.SetPointsPerSecond(PointsPerUnit * (Speed / InitialSpeed));
            }
            else {
                GameManagerIRE_PW.Instance.SetPointsPerSecond(0f);
            }
        }

        /// <summary>
        /// What to do if the character's simulated movement is blocked (character begins to move in -x direction)
        /// </summary>
        protected virtual void HandleCharacterBlocked() {
            if (CurrentPlayableCharacters[0].transform.position.x < StartingPosition.transform.position.x && CurrentPlayableCharacters[0].CollidingRight) {
                if (_charBlocking) {
                    if (Speed > BlockedSpeed) {
                        Speed -= BlockedDeceleration * Time.deltaTime;
                        if (Speed < BlockedSpeed) {
                            Speed = BlockedSpeed;
                        }
                    }
                }
                else {
                    _charBlocking = true;
                    _preBlockSpeed = Speed;
                    _lastBlockTime = Time.time;
                }
            }
            else if (_charBlocking && !CurrentPlayableCharacters[0].CollidingRight && (Time.time - _lastBlockTime > 0.1f)) {
                _charBlocking = false;
                Speed = _preBlockSpeed;
            }
        }

        /// <summary>
        /// Resets the level : repops dead characters, sets everything up for a new game
        /// </summary>
        public virtual void ResetLevel() {
            InstantiateCharacters();
            PrepareStart();
        }

        /// <summary>
        /// Temporarily add to the current speed
        /// Used by CharacterDodge
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyAddSpeed(float factor, float duration) {
            if (!_tempSpeedAddedActive && !TempSpeedSwitchActive) {
                CurrentUnmodifiedSpeed = Speed;
            }

            _tempSpeedAddedActive = true;

            if (_charBlocking) {
                _preBlockSpeed += factor;
            }
            else {
                Speed += factor;
            }
            _currentAddedSpeed += factor;

            StartCoroutine(TemporarilyAddSpeedCo(factor, duration));
        }

        protected IEnumerator TemporarilyAddSpeedCo(float factor, float duration) {
            _tempActiveCount++;
            yield return new WaitForSeconds(duration);

            if (_charBlocking) {
                _preBlockSpeed -= factor;
            }
            else {
                Speed -= factor;
            }
            _currentAddedSpeed -= factor;

            _tempActiveCount--;
            if (_tempActiveCount == 0) {
                _tempSpeedAddedActive = false;
            }
        }

        /// <summary>
        /// Temporarily add speed, end when distance has been elapsed
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="distance"></param>
        public virtual void TemporarilyAddSpeedDist(float factor, float distance) {
            if (_teleporting) {
                Debug.Log("teleporting");
            }
            if (!_tempSpeedAddedActive && !TempSpeedSwitchActive) {
                CurrentUnmodifiedSpeed = Speed;
            }

            _tempSpeedAddedActive = true;

            if (_charBlocking) {
                _preBlockSpeed += factor;
            }
            else {
                Speed += factor;
            }
            _currentAddedSpeed += factor;

            _teleporting = true;
            _tempActiveCount++;
            _teleportStart = DistanceTraveled;
            _teleportDistance = distance;
            _teleportSpeed = factor;
            //StartCoroutine(TemporarilyAddSpeedDistCo(factor, distance));
        }

        protected IEnumerator TemporarilyAddSpeedDistCo(float factor, float distance) {
            float start = DistanceTraveled;
            while (Mathf.Abs(DistanceTraveled - start) < distance) {
                yield return null;
            }

            if (_charBlocking) {
                _preBlockSpeed -= factor;
            }
            else {
                Speed -= factor;
            }
            _currentAddedSpeed -= factor;
            _tempActiveCount--;

            if (_tempActiveCount == 0) {
                _tempSpeedAddedActive = false;
            }
        }

        /// <summary>
        /// Temporarily add to speed. Must be turned off manually.
        /// Used by CharacterSuper, Portal
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="guid"></param>
        public virtual void TemporarilyAddSpeedSwitch(float factor, Guid guid) {
            if (!_tempSpeeds.ContainsKey(guid)) {
                _tempSpeeds.Add(guid, factor);

                if (!_tempSpeedAddedActive) {
                    CurrentUnmodifiedSpeed = Speed;
                }
                _tempSpeedAddedActive = true;
                _tempActiveCount++;

                if (_charBlocking) {
                    _preBlockSpeed += factor;
                }
                else {
                    Speed += factor;
                }
                _currentAddedSpeed += factor;
            }
            else {
                if (_charBlocking) {
                    _preBlockSpeed -= factor;
                }
                else {
                    Speed -= factor;
                }
                _currentAddedSpeed -= factor;

                _tempSpeeds.Remove(guid);

                _tempActiveCount--;
                if (_tempActiveCount == 0) {
                    _tempSpeedAddedActive = false;
                }
            }
        }

        /// <summary>
        /// Sets current speed to zero, or restores it
        /// Resets all temp speed modifiers
        /// </summary>
        /// <param name="on"></param>
        public virtual void SetZeroSpeed(bool on, bool retainEnemySpeed) {
            if (on) {
                if (!_tempSpeedAddedActive) {
                    CurrentUnmodifiedSpeed = Speed;
                }
                _tempActiveCount = 0;
                _tempSpeeds.Clear();
                StopAllCoroutines();
                Speed = 0;
                _tempSpeedAddedActive = false;
            }
            else {
                Speed = CurrentUnmodifiedSpeed;
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
            GameManagerIRE_PW.Instance.SetPoints(_savedPoints);
            GameManagerIRE_PW.Instance.LoseLives(1);

            /// If no more lives, trigger game over
            if (GameManagerIRE_PW.Instance.CurrentLives <= 0) {
                GUIManagerIRE_PW.Instance.SetGameOverScreen(true);
                GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.GameOver);
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
            // if we've specified an effect for when a life is lost, we instantiate it at the camera's position
            if (LifeLostExplosion != null) {
                GameObject explosion = Instantiate(LifeLostExplosion);
                explosion.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            }

            // we've just lost a life
            GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.LifeLost);
            MMEventManager.TriggerEvent(new MMGameEvent("LifeLost"));
            _started = DateTime.UtcNow;
            GameManagerIRE_PW.Instance.SetPoints(_savedPoints);
            GameManagerIRE_PW.Instance.LoseLives(1);
            player.gameObject.SetActive(false);
            SetZeroSpeed(true, false);

            if (GameManagerIRE_PW.Instance.CurrentLives <= 0) {
                GUIManagerIRE_PW.Instance.SetGameOverScreen(true);
                GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.GameOver);
                MMEventManager.TriggerEvent(new MMGameEvent("GameOver"));
            }
        }

        /// <summary>
        /// Triggered when all lives are lost and you press the main action button
        /// </summary>
        public virtual void GameOverAction() {
            GameManagerIRE_PW.Instance.UnPause();
            GotoLevel(SceneManager.GetActiveScene().name);
        }

        public virtual void LifeLostAction() {
            SetZeroSpeed(false, true);
            Instance.CurrentPlayableCharacters[0].transform.position = StartingPosition.transform.position;
            Instance.CurrentPlayableCharacters[0].ResetCharacter();
            Instance.CurrentPlayableCharacters[0].gameObject.SetActive(true);
            GameManagerIRE_PW.Instance.SetStatus(GameManagerIRE_PW.GameStatus.GameInProgress);
        }

        /// <summary>
        /// Determines if the object whose bounds are passed as a parameter has to be recycled or not.
        /// </summary>
        /// <returns><c>true</c>, if the object has to be recycled, <c>false</c> otherwise.</returns>
        /// <param name="objectBounds">Object bounds.</param>
        /// <param name="destroyDistance">The x distance after which the object will get destroyed.</param>
        public virtual bool CheckRecycleCondition(Bounds objectBounds, float destroyDistance, OutOfBoundsTypes outOfBoundsType = OutOfBoundsTypes.Recycle) {
            if (outOfBoundsType == OutOfBoundsTypes.Recycle) {
                _tmpRecycleBounds = RecycleBounds;
            }
            else {
                _tmpRecycleBounds = DeathBounds;
            }
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
            GUIManagerIRE_PW.Instance.FaderOn(true, OutroFadeDuration);
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
            GameManagerIRE_PW.Instance.UnPause();

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
            if (TempSpeedSwitchActive) {
                Speed = CurrentUnmodifiedSpeed + SpeedIncrement + _tempSpeedSwitchFactor;
            }
            else {
                Speed += SpeedIncrement;
            }
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
