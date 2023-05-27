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

        public PaywallDeathEvent(GameObject deadObject) {
            DeadObject = deadObject;
        }
        static PaywallDeathEvent e;
        public static void Trigger(GameObject deadObject) {
            e.DeadObject = deadObject;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallDamageEvent {
        public int Damage;

        public PaywallDamageEvent(int damage, GameObject instigator) {
            Damage = damage;
        }
        static PaywallDamageEvent e;
        public static void Trigger(int damage) {
            e.Damage = damage;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallEXChargeEvent {
        public float ChargeAmount;

        public PaywallEXChargeEvent(float chargeAmount) {
            ChargeAmount = chargeAmount;
        }
        static PaywallEXChargeEvent e;
        public static void Trigger(float chargeAmount) {
            e.ChargeAmount = chargeAmount;
            MMEventManager.TriggerEvent(e);
        }
    }

}
