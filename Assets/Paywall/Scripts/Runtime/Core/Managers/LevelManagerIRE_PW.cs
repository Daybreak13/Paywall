using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using System;

namespace Paywall {

    public class LevelManagerIRE_PW : LevelManager {

        public float EnemySpeed { get; protected set; }

        protected bool _retainEnemySpeed;
        protected bool _tempSpeedSwitch;

        public override void Update() {
            base.Update();
            if (!_retainEnemySpeed) {
                EnemySpeed = Speed;
            }
        }

        /// <summary>
        /// On level start or respawn, reset the temp speed factor
        /// </summary>
        public override void LevelStart() {
            base.LevelStart();
            SwitchTempSpeedOff();
        }

        /// <summary>
        /// Temp speed multiplier with option for retaining enemy NPC speed
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="duration"></param>
        /// <param name="retainEnemySpeed"></param>
        public virtual void TemporarilyMultiplySpeed(float factor, float duration, bool retainEnemySpeed) {
            if (retainEnemySpeed) {
                _retainEnemySpeed = true;
            }

            TemporarilyMultiplySpeed(factor, duration);
        }

        public virtual void TemporarilyMultiplySpeedSwitch(float factor, bool retainEnemySpeed) {
            _retainEnemySpeed = retainEnemySpeed;

            _temporarySpeedFactor = factor;

            if (!_tempSpeedSwitch) {
                _temporarySavedSpeed = Speed;
            }

            Speed = _temporarySavedSpeed * _temporarySpeedFactor;
            _tempSpeedSwitch = true;
        }

        protected override void HandleSpeedFactor() {
            if (_temporarySpeedFactorActive && _temporarySpeedFactorRemainingTime <= 0) {
                _retainEnemySpeed = false;
            }
            base.HandleSpeedFactor();
        }

        /// <summary>
        /// Resets the temp speed multiplier
        /// </summary>
        public virtual void SwitchTempSpeedOff() {
            if (_tempSpeedSwitch) {
                _tempSpeedSwitch = false;
                _retainEnemySpeed = false;
                Speed = _temporarySavedSpeed;
            }
        }

        public override void KillCharacter(PlayableCharacter player) {
            // if we've specified an effect for when a life is lost, we instantiate it at the camera's position
            if (LifeLostExplosion != null) {
                GameObject explosion = Instantiate(LifeLostExplosion);
                explosion.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            }

            // we've just lost a life
            MMEventManager.TriggerEvent(new MMGameEvent("LifeLost"));
            _started = DateTime.UtcNow;
            GameManager.Instance.SetPoints(_savedPoints);
            GameManager.Instance.LoseLives(1);

            /// If no more lives, trigger game over
            if (GameManager.Instance.CurrentLives <= 0) {
                GUIManager.Instance.SetGameOverScreen(true);
                GameManager.Instance.SetStatus(GameManager.GameStatus.GameOver);
                MMEventManager.TriggerEvent(new MMGameEvent("GameOver"));
            }
        }

        /// <summary>
        /// Kills the character if it goes out of bounds
        /// </summary>
        /// <param name="player"></param>
        public virtual void KillCharacterOutOfBounds(PlayableCharacter player) {
            TemporarilyMultiplySpeedSwitch(0f, true);
            LevelManager.Instance.CurrentPlayableCharacters.Remove(player);
            player.Die();

            // if this was the last character, we trigger the all characters are dead coroutine
            if (LevelManager.Instance.CurrentPlayableCharacters.Count == 0) {
                AllCharactersAreDead();
            }
        }

        protected override void AllCharactersAreDead() {
            base.AllCharactersAreDead();
        }

        public override void LifeLostAction() {
            base.LifeLostAction();
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

    }
}
