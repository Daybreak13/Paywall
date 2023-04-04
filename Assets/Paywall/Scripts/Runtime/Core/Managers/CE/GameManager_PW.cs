using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.ComponentModel;

namespace Paywall {

    /// <summary>
    /// Adds points on Update
    /// </summary>
    public class GameManager_PW : GameManager {
        [field: Header("Paywall")]

        [field:Range(1f, 5f)]
        [field: SerializeField] public float GameSpeed { get; set; } = 1f;

        protected float _gameSpeed = 1f;
        protected bool _levelStarted;
        protected float _points;
        protected bool _characterDead;
        protected bool _gameOver;

        protected virtual void Update() {
            if (_levelStarted && !_characterDead && !_gameOver && !Paused) {
                _points += GameSpeed * Time.deltaTime;
                SetPoints((int)_points);
            }
        }

        public override void OnMMEvent(CorgiEngineEvent engineEvent) {
            base.OnMMEvent(engineEvent);
            if (engineEvent.EventType == CorgiEngineEventTypes.LevelStart) {
                _levelStarted = true;
                //CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
                _characterDead = false;
                _gameOver = false;
            }
            if (engineEvent.EventType == CorgiEngineEventTypes.LevelEnd) {
                _levelStarted = false;
                CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
            }
            if (engineEvent.EventType == CorgiEngineEventTypes.PlayerDeath) {
                _characterDead = true;
            }
            if (engineEvent.EventType == CorgiEngineEventTypes.GameOver) {
                _gameOver = true;
            }

        }

        /// <summary>
		/// Catches CorgiEnginePointsEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="pointEvent">CorgiEnginePointsEvent event.</param>
		public override void OnMMEvent(CorgiEnginePointsEvent pointEvent) {
            switch (pointEvent.PointsMethod) {
                case PointsMethods.Set:
                    _points = pointEvent.Points;
                    SetPoints(pointEvent.Points);
                    break;

                case PointsMethods.Add:
                    _points += pointEvent.Points;
                    AddPoints(pointEvent.Points);
                    break;
            }
        }

        protected virtual void OnDestroy() {
            if (PaywallProgressManager.HasInstance) {
                //PaywallProgressManager.Instance.ResetProgress();
            }
        }

    }
}
