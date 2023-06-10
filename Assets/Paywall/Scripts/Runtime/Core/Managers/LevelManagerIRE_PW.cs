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
    public class LevelManagerIRE_PW : Singleton_PW<LevelManagerIRE_PW> {
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
        [field: SerializeField] public List<CharacterIRE> PlayableCharacters { get; protected set; }
        /// the list of playable characters currently instantiated in the game - use this to know what characters ARE currently in your level at runtime
        [field: SerializeField] public List<CharacterIRE> CurrentPlayableCharacters { get; protected set; }
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
        /// Current speed not counting temp speed modifiers
        [field: MMReadOnly]
        [field: SerializeField] public float CurrentUnmodifiedSpeed { get; protected set; }
        /// the acceleration (per second) at which the level will go from InitialSpeed to MaximumSpeed
        [field: SerializeField] public float SpeedAcceleration { get; protected set; } = 0f;
        /// the global speed for level segments
        [field: Tooltip("the global speed for level segments")]
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

        protected bool _temporarySpeedFactorActive;
        protected float _temporarySpeedFactor;
        protected float _temporarySpeedFactorRemainingTime;
        protected float _temporarySavedSpeed;

        protected bool _retainEnemySpeed;
        protected int _coroutineCount = 0;

        public bool TempSpeedSwitchActive { get; protected set; }    // is the temp speed switch active

        /// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start() {
            Speed = InitialSpeed;
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
            //(GameManagerIRE_PW.Instance as GameManagerIRE_PW).AutoIncrementScore(true);
            MMEventManager.TriggerEvent(new MMGameEvent("GameStart"));
            TempSpeedSwitchOff();
        }

        /// <summary>
        /// Instantiates all the playable characters and feeds them to the gameManager
        /// </summary>
        protected virtual void InstantiateCharacters() {
            CurrentPlayableCharacters = new List<CharacterIRE>();
            /// we go through the list of playable characters and instantiate them while adding them to the list we'll use from any class to access the
            /// currently playable characters

            // we check if there's a stored character in the game manager we should instantiate
            //if (CharacterSelectorManager.Instance.StoredCharacter != null) {
            //    PlayableCharacter newPlayer = (CharacterIRE)Instantiate(CharacterSelectorManager.Instance.StoredCharacter, StartingPosition.transform.position, StartingPosition.transform.rotation);
            //    newPlayer.name = CharacterSelectorManager.Instance.StoredCharacter.name;
            //    newPlayer.SetInitialPosition(newPlayer.transform.position);
            //    CurrentPlayableCharacters.Add(newPlayer);
            //    MMEventManager.TriggerEvent(new MMGameEvent("PlayableCharactersInstantiated"));
            //    return;
            //}

            if (PlayableCharacters == null) {
                return;
            }

            if (PlayableCharacters.Count == 0) {
                return;
            }

            // for each character in the PlayableCharacters list
            for (int i = 0; i < PlayableCharacters.Count; i++) {
                // we instantiate the corresponding prefab
                CharacterIRE instance = (CharacterIRE)Instantiate(PlayableCharacters[i]);
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
            if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
                return;
            }

            _savedPoints = (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Points;
            _started = DateTime.UtcNow;

            // we increment the total distance traveled so far
            DistanceTraveled += Speed / 10f * Time.deltaTime;
            if (GUIManagerIRE_PW.HasInstance) {
                GUIManagerIRE_PW.Instance.RefreshDistance();
            }

            // if we can still accelerate, we apply the level's speed acceleration
            if ((Speed < MaximumSpeed) && !TempSpeedSwitchActive) {
                Speed += SpeedAcceleration * Time.deltaTime;
            }

            if (!_temporarySpeedFactorActive && !TempSpeedSwitchActive) {
                CurrentUnmodifiedSpeed = Speed;
            }

            HandleSpeedFactor();

            RunningTime += Time.deltaTime;
            if (!_retainEnemySpeed) {
                EnemySpeed = Speed;
            }
        }

        /// <summary>
        /// Temp speed multiplier with option for retaining enemy NPC speed
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyMultiplySpeed(float factor, float duration, bool retainEnemySpeed = false) {
            if (retainEnemySpeed) {
                _retainEnemySpeed = true;
            }

            _temporarySpeedFactorRemainingTime = duration;

            Speed *= factor;
            _temporarySpeedFactorActive = true;

            StartCoroutine(SpeedMultiplierCo(factor, duration));
        }

        /// <summary>
        /// Maintain speed multiplier for given duration, then turn it off
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator SpeedMultiplierCo(float factor, float duration) {
            _coroutineCount++;
            yield return new WaitForSeconds(duration);
            Speed /= factor;
            _coroutineCount--;
            if (_coroutineCount == 0) {
                _temporarySpeedFactorActive = false;
            }
        }

        /// <summary>
        /// Rather than using a duration, multiply speed until switched off
        /// Cannot activate speed switch if it is already active
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyMultiplySpeedSwitch(float factor, bool retainEnemySpeed) {
            if (TempSpeedSwitchActive) {
                return;
            }

            _retainEnemySpeed = retainEnemySpeed;

            _temporarySpeedFactor = factor;

            if (!TempSpeedSwitchActive) {
                _temporarySavedSpeed = Speed;
            }

            Speed *= _temporarySpeedFactor;
            TempSpeedSwitchActive = true;
        }

        /// <summary>
        /// Handles speed multipliers
        /// </summary>
        protected virtual void HandleSpeedFactor() {
            //if (_temporarySpeedFactorActive && _temporarySpeedFactorRemainingTime <= 0) {
            //    _retainEnemySpeed = false;
            //}

            //if (_temporarySpeedFactorActive) {
            //    if (_temporarySpeedFactorRemainingTime <= 0) {
            //        _temporarySpeedFactorActive = false;
            //        Speed = _temporarySavedSpeed;
            //    }
            //    else {
            //        _temporarySpeedFactorRemainingTime -= Time.deltaTime;
            //    }
            //}

            // Points per second increases/decreases proportionally to the ratio of the current speed to initial speed
            if (Speed > 0) {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPointsPerSecond(PointsPerUnit * (Speed / InitialSpeed));
            } else {
                (GameManagerIRE_PW.Instance as GameManagerIRE_PW).SetPointsPerSecond(0f);
            }
        }

        /// <summary>
        /// Resets the temp speed multiplier
        /// </summary>
        public virtual void TempSpeedSwitchOff() {
            if (TempSpeedSwitchActive) {
                TempSpeedSwitchActive = false;
                _retainEnemySpeed = false;
                Speed /= _temporarySpeedFactor;
            }
        }

        public virtual void KillCharacter(CharacterIRE player) {
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
                player.GetComponent<CharacterIRE>().ActivateTempInvincibility();
            }
        }

        /// <summary>
        /// Kills the character if it goes out of bounds
        /// </summary>
        /// <param name="player"></param>
        public virtual void KillCharacterOutOfBounds(CharacterIRE player) {
            TemporarilyMultiplySpeedSwitch(0f, true);

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
            TempSpeedSwitchOff();
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
	    /// Override this if needed
	    /// </summary>
	    protected virtual void OnEnable() {

        }

        /// <summary>
        /// Override this if needed
        /// </summary>
        protected virtual void OnDisable() {

        }

    }
}
