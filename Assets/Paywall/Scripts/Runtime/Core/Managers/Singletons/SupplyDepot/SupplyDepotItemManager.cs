using MoreMountains.Tools;
using Paywall.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Class which contains actions associated with buying depot items
    /// BaseScriptableDepotItem.BuyAction() will call the corresponding action here
    /// </summary>
    public class SupplyDepotItemManager : Singleton_PW<SupplyDepotItemManager>, MMEventListener<MMGameEvent> {

        protected bool _ammoRitual;
        protected bool _bigTrinket;
        protected int _ammoGain;

        #region Shop Items

        public virtual void AmmoItem(int ammoGain) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.Ammo, ammoGain);
        }

        public virtual void EXItem(int ex) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.EX, ex);
        }

        public virtual void HealthItem(int healthGain) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.Health, healthGain);
        }

        /// <summary>
        /// Increase chance of big trinket spawn for one level
        /// </summary>
        public virtual void BigTrinketChance() {
            _bigTrinket = true;
        }

        #endregion

        #region Rituals

        /// <summary>
        /// If no damage is taken next level, gain 1 Ammo
        /// </summary>
        /// <param name="ammoGain"></param>
        public virtual void AmmoRitual(int ammoGain) {
            _ammoRitual = true;
            _ammoGain = ammoGain;
        }

        /// <summary>
        /// Increase level speed, gain 1 Health fragment
        /// </summary>
        public virtual void HealthRitual(int healthGain, float speedIncrease) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.Health, healthGain);
            LevelManagerIRE_PW.Instance.IncreaseLevelSpeed(speedIncrease);
        }

        #endregion

        public virtual void OnMMEvent(MMGameEvent gameEvent) {
            if (gameEvent.EventName.Equals("LifeLost")) {
                _ammoRitual = false;
            }

            if (gameEvent.EventName.Equals("EnterDepot")) {
                if (_ammoRitual) {
                    _ammoRitual = false;
                    RunnerItemPickEvent.Trigger(PowerUpTypes.Ammo, _ammoGain);
                }
                if (_bigTrinket) {
                    _bigTrinket = false;
                }
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
