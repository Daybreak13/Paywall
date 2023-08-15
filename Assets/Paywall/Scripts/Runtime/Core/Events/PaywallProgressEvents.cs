using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

	public enum MoneyMethods { Add, Set }
	public enum MoneyTypes { Credit, Trinket }

	public struct PaywallCreditsEvent {
		public MoneyTypes MoneyType;
		public MoneyMethods MoneyMethod;
		public int Money;

		/// <summary>
		/// Initializes a new instance of the <see cref="Paywall.PaywallCreditsEvent"/> struct.
		/// </summary>
		/// <param name="moneyMethod">Credits method</param>
		/// <param name="credits">Credits</param>
		public PaywallCreditsEvent(MoneyTypes moneyType, MoneyMethods moneyMethod, int credits) {
			MoneyType = moneyType;
			MoneyMethod = moneyMethod;
			Money = credits;
		}
		static PaywallCreditsEvent e;
		public static void Trigger(MoneyTypes moneyType, MoneyMethods creditsMethod, int credits) {
			e.MoneyType = moneyType;
			e.MoneyMethod = creditsMethod;
			e.Money = credits;
			MMEventManager.TriggerEvent(e);
		}
	}

	public enum UpgradeMethods { Unlock, Lock, Error, TryUnlock }

	/// <summary>
	/// Upgrade event, includes the scriptable upgrade and its button object
	/// </summary>
	public struct PaywallUpgradeEvent {
		public UpgradeMethods UpgradeMethod;
		public ScriptableUpgrade Upgrade;
		public UpgradeButton ButtonComponent;

		public PaywallUpgradeEvent(UpgradeMethods upgradeMethod, ScriptableUpgrade upgrade = null, UpgradeButton button = null) {
			UpgradeMethod = upgradeMethod;
			Upgrade = upgrade;
			ButtonComponent = button;
		}
		static PaywallUpgradeEvent e;
		public static void Trigger(UpgradeMethods upgradeMethod, ScriptableUpgrade upgrade = null, UpgradeButton button = null) {
			e.UpgradeMethod = upgradeMethod;
			e.Upgrade = upgrade;
			e.ButtonComponent = button;
			MMEventManager.TriggerEvent(e);
		}
	}

	public struct PaywallLevelEndEvent {
		public bool convertCredits;
		public PaywallLevelEndEvent(bool convert) {
			convertCredits = convert;
		}
		static PaywallLevelEndEvent e;
		public static void Trigger(bool convert) {
			e.convertCredits = convert;
			MMEventManager.TriggerEvent(e);
		}
	}

}
