using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		}
	}

	public enum UpgradeMethods { Unlock, Lock }

	public struct PaywallUpgradeEvent {
		public UpgradeMethods UpgradeMethod;
		public ScriptableUpgrade Upgrade;

		public PaywallUpgradeEvent(UpgradeMethods upgradeMethod, ScriptableUpgrade upgrade) {
			UpgradeMethod = upgradeMethod;
			Upgrade = upgrade;
		}
		static PaywallUpgradeEvent e;
		public static void Trigger(UpgradeMethods upgradeMethod, ScriptableUpgrade upgrade) {
			e.UpgradeMethod = upgradeMethod;
			e.Upgrade = upgrade;
		}

	}

}
