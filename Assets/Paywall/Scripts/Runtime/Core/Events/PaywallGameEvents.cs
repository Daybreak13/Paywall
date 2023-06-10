using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

    public enum PaywallGameEventType { Difficulty, NoneChance }

    public struct PaywallDifficultyEvent {
        public int CurrentDifficulty;

        public PaywallDifficultyEvent(int difficulty) {
            CurrentDifficulty = difficulty;
        }
        static PaywallDifficultyEvent e;
        public static void Trigger(int difficulty) {
            e.CurrentDifficulty = difficulty;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallChanceUpdateEvent {
        public float Chance;

        public PaywallChanceUpdateEvent(float chance) {
            Chance = chance;
        }
        static PaywallChanceUpdateEvent e;
        public static void Trigger(float chance) {
            e.Chance = chance;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallGameEvent {
        public int Variable;

        public PaywallGameEvent(int variable) {
            Variable = variable;
        }
        static PaywallGameEvent e;
        public static void Trigger(int variable) {
            e.Variable = variable;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallDeathEvent {
        public GameObject DeadObject;
        public bool IncreaseStreak;

        public PaywallDeathEvent(GameObject deadObject, bool increaseStreak = false) {
            DeadObject = deadObject;
            IncreaseStreak = increaseStreak;
        }
        static PaywallDeathEvent e;
        public static void Trigger(GameObject deadObject, bool increaseStreak = false) {
            e.DeadObject = deadObject;
            e.IncreaseStreak = increaseStreak;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallDamageEvent {
        public int Damage;
        GameObject Instigator;

        public PaywallDamageEvent(int damage, GameObject instigator) {
            Damage = damage;
            Instigator = instigator;
        }
        static PaywallDamageEvent e;
        public static void Trigger(int damage, GameObject instigator) {
            e.Damage = damage;
            e.Instigator = instigator;
            MMEventManager.TriggerEvent(e);
        }
    }

    public enum ChangeAmountMethods { Add, Set }

    public struct PaywallEXChargeEvent {
        public float ChargeAmount;
        public ChangeAmountMethods ChangeAmountMethod;

        public PaywallEXChargeEvent(float chargeAmount, ChangeAmountMethods changeAmountMethod) {
            ChargeAmount = chargeAmount;
            ChangeAmountMethod = changeAmountMethod;
        }
        static PaywallEXChargeEvent e;
        public static void Trigger(float chargeAmount, ChangeAmountMethods changeAmountMethod) {
            e.ChargeAmount = chargeAmount;
            e.ChangeAmountMethod = changeAmountMethod;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// Event for items picked during a run, that only apply during the run (eg powerups)
    /// </summary>
    public struct RunnerItemPickEvent {
        public PowerUpTypes PickedPowerUpType;

        public RunnerItemPickEvent(PowerUpTypes powerUpType) {
            PickedPowerUpType = powerUpType;
        }
        static RunnerItemPickEvent e;
        public static void Trigger(PowerUpTypes powerUpType) {
            e.PickedPowerUpType = powerUpType;
            MMEventManager.TriggerEvent(e);
        }
    }

}
