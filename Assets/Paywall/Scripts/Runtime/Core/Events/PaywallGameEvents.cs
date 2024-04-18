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
        public GameObject Instigator;

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

    public struct PaywallKillEvent {
        public bool PlayerInstigated;
        public GameObject Killed;

        public PaywallKillEvent(bool playerInstigated, GameObject killed) {
            PlayerInstigated = playerInstigated;
            Killed = killed;
        }
        static PaywallKillEvent e;
        public static void Trigger(bool playerInstigated, GameObject killed) {
            e.PlayerInstigated = playerInstigated;
            e.Killed = killed;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// Event for items picked during a run, that only apply during the run (eg powerups)
    /// </summary>
    public struct RunnerItemPickEvent {
        public PowerUpTypes PickedPowerUpType;
        public int Amount;

        public RunnerItemPickEvent(PowerUpTypes powerUpType, int amount) {
            PickedPowerUpType = powerUpType;
            Amount = amount;
        }
        static RunnerItemPickEvent e;
        public static void Trigger(PowerUpTypes powerUpType, int amount) {
            e.PickedPowerUpType = powerUpType;
            e.Amount = amount;
            MMEventManager.TriggerEvent(e);
        }
    }

    public enum PauseMethods { PauseOn, PauseOff }
    public enum PauseScreenMethods { PauseScreen, SupplyDepotScreen, None }

    public struct PaywallPauseEvent {
        public PauseMethods PauseMethod;
        public PauseScreenMethods PauseScreenMethod;

        public PaywallPauseEvent(PauseMethods pauseMethod, PauseScreenMethods pauseScreenMethod = PauseScreenMethods.PauseScreen) {
            PauseMethod = pauseMethod;
            PauseScreenMethod = pauseScreenMethod;
        }
        static PaywallPauseEvent e;
        public static void Trigger(PauseMethods pauseMethod, PauseScreenMethods pauseScreenMethod = PauseScreenMethods.PauseScreen) {
            e.PauseMethod = pauseMethod;
            e.PauseScreenMethod = pauseScreenMethod;
            MMEventManager.TriggerEvent(e);
        }
    }

    public enum DialogueEventTypes { Open, Close, ForceClose }
    public struct PaywallDialogueEvent {
        public DialogueEventTypes DialogueEventType;
        public List<DialogueLine> DialogueLines;

        public PaywallDialogueEvent(DialogueEventTypes dialogueEventType, List<DialogueLine> dialogueLines) {
            DialogueEventType = dialogueEventType;
            DialogueLines = dialogueLines;
        }
        static PaywallDialogueEvent e;
        public static void Trigger(DialogueEventTypes dialogueEventType, List<DialogueLine> dialogueLines) {
            e.DialogueEventType = dialogueEventType;
            e.DialogueLines = dialogueLines;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct PaywallModuleEvent {
        public ScriptableModule Module;

        public PaywallModuleEvent(ScriptableModule module) {
            Module = module;
        }
        static PaywallModuleEvent e;
        public static void Trigger(ScriptableModule module) {
            e.Module = module;
            MMEventManager.TriggerEvent(e);
        }
    }

}
