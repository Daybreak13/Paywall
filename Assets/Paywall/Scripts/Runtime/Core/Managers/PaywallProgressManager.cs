using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using Paywall.Documents;
using MoreMountains.InventoryEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {
	
	[System.Serializable]
	/// <summary>
	/// A serializable entity to store scenes, whether they've been completed, unlocked, and which ones
	/// </summary>
	public class PaywallScene {
		public string SceneName;
		public bool LevelComplete = false;
		public bool LevelUnlocked = false;
	}

	[System.Serializable]
	/// <summary>
	/// A serializable entity used to store progress : a list of scenes with their internal status (see above), how many lives are left, and how much we can have
	/// </summary>
	public class Progress {
		public int InitialMaximumLives = 0;
		public int InitialCurrentLives = 0;
		public int MaximumLives = 0;
		public int CurrentLives = 0;
		public int Credits = 0;
		public int Trinkets = 0;
		public Dictionary<string, Upgrade> Upgrades = new Dictionary<string, Upgrade>();
		public Dictionary<string, EmailItem> EmailItems = new Dictionary<string, EmailItem>();
		public SerializedInventory serializedInventory;
		public PaywallScene[] Scenes;
	}

	/// <summary>
	/// Creates save files for game progress and handles saves and loads
	/// Every time credits are updated, save the game
	/// Saves game on MMGameEvent "Save"
	/// 
	/// </summary>
	public class PaywallProgressManager : MMSingleton<PaywallProgressManager>, MMEventListener<PaywallCreditsEvent>, 
		MMEventListener<MMGameEvent>, MMEventListener<PaywallUpgradeEvent>, 
		MMEventListener<PaywallLevelEndEvent>, MMEventListener<EmailEvent> {

		public int InitialMaximumLives { get; set; }
		public int InitialCurrentLives { get; set; }

		public string PlayerID = "Player1";

		/// the list of scenes that we'll want to consider for our game
		[Tooltip("the list of scenes that we'll want to consider for our game")]
		public PaywallScene[] Scenes;

		[MMInspectorButton("CreateSaveGame")]
		/// A button to test creating the save file
		public bool CreateSaveGameBtn;

		/// Credits, readonly, for debug purposes only. Comment out Update for final build.
		[field: Tooltip("Credits, readonly, for debug purposes only. Comment out Update for final build.")]
		[field: MMReadOnly]
		[field: SerializeField] private int displayedCredits;

		/// Credits are the "real world" currency
		[field: Tooltip("Credits are the \"real world\" currency")]
		[field:SerializeField] public int Credits { get; private set; }
		/// Trinkets are the "game world" currency
		[field: Tooltip("Trinkets are the \"game world\" currency")]
		public int Trinkets { get; private set; }

		public Dictionary<string, EmailItem> EmailItems { get; protected set; }
		public Dictionary<string, Upgrade> Upgrades { get; private set; }
		public enum SaveMethods { All, Inventory }

		protected const string _saveFolderName = "PaywallProgress";
		protected const string _saveFileName = "Progress.data";

		/// <summary>
		/// On awake, we load our progress
		/// </summary>
		protected override void Awake() {
			base.Awake();
			LoadSavedProgress();
		}

		protected virtual void Start() {
			if (EmailItems != null) {
				EmailEvent.Trigger(EmailEventType.TriggerLoad, null, PlayerID, EmailItems);
			}
		}

		/// <summary>
		/// Updates credit display
		/// For debugging only. Comment out for final build.
		/// </summary>
		protected virtual void Update() {
			displayedCredits = Credits;
        }

		/// <summary>
		/// When a level is completed, we update our progress
		/// </summary>
		protected virtual void LevelComplete() {
			for (int i = 0; i < Scenes.Length; i++) {
				if (Scenes[i].SceneName == SceneManager.GetActiveScene().name) {
					Scenes[i].LevelComplete = true;
					Scenes[i].LevelUnlocked = true;
					if (i < Scenes.Length - 1) {
						Scenes[i + 1].LevelUnlocked = true;
					}
				}
			}
		}

		/// <summary>
		/// Saves the progress to a file
		/// </summary>
		protected virtual void SaveProgress(SaveMethods saveMethod = SaveMethods.All) {
			Progress progress = new Progress();

			if (saveMethod == SaveMethods.All) {
				progress.CurrentLives = GameManager.Instance.CurrentLives;

				progress.Scenes = Scenes;
				progress.Credits = Credits;
				progress.Trinkets = Trinkets;
				progress.Upgrades = Upgrades;
				progress.EmailItems = EmailItems;
			}
			if (saveMethod == SaveMethods.Inventory) {

            }

			MMSaveLoadManager.Save(progress, _saveFileName, _saveFolderName);
		}

		/// <summary>
		/// Called when the level ends.
		/// Convert points, save progress.
		/// </summary>
		protected virtual void LevelEnd(bool convert) {
			if (convert) ConvertPointsToCredits();
			SaveProgress();
        }

		/// <summary>
		/// Converts current points to credits
		/// </summary>
		protected virtual void ConvertPointsToCredits() {
			if (GameManager.HasInstance) {
				Credits += (int)GameManager.Instance.Points;
            }
        }

		/// <summary>
		/// Adds money
		/// </summary>
		/// <param name="money"></param>
		protected virtual void AddMoney(MoneyTypes MoneyType, int money) {
			if (MoneyType == MoneyTypes.Credit) {
				Credits += money;
			} else {
				Trinkets += money;
            }
        }

		/// <summary>
		/// Sets money
		/// </summary>
		/// <param name="money"></param>
		protected virtual void SetMoney(MoneyTypes MoneyType, int money) {
			if (MoneyType == MoneyTypes.Trinket) {
				Credits = money;
			} else {
				Trinkets = money;
            }
		}

		/// <summary>
		/// A test method to create a test save file at any time from the inspector
		/// </summary>
		protected virtual void CreateSaveGame() {
			SaveProgress();
		}

		/// <summary>
		/// Loads the saved progress into memory
		/// </summary>
		protected virtual void LoadSavedProgress() {
			Progress progress = (Progress)MMSaveLoadManager.Load(typeof(Progress), _saveFileName, _saveFolderName);
			if (progress != null) {
				Scenes = progress.Scenes;
				GameManager.Instance.SetLives(progress.CurrentLives);
				InitialMaximumLives = progress.InitialMaximumLives;
				InitialCurrentLives = progress.InitialCurrentLives;
				Scenes = progress.Scenes;
				Credits = progress.Credits;
				Trinkets = progress.Trinkets;
				Upgrades = progress.Upgrades;
				EmailItems = progress.EmailItems;
				if (EmailItems != null) {
					EmailEvent.Trigger(EmailEventType.TriggerLoad, null, PlayerID, EmailItems);
				}
			}
			else {
				InitialCurrentLives = GameManager.Instance.CurrentLives;
			}
		}

		/// <summary>
		/// A method used to remove all save files associated to progress
		/// </summary>
		public virtual void ResetProgress() {
			MMSaveLoadManager.DeleteSaveFolder(_saveFolderName);
		}

		protected virtual void ApplyUpgradesToCharacter() {
			if (LevelManager.HasInstance && (LevelManager.Instance.CurrentPlayableCharacters.Count > 0) && (Upgrades != null)) {
				foreach (KeyValuePair<string, Upgrade> entry in Upgrades) {
					if (entry.Value.UpgradeType == UpgradeTypes.Player) {
						entry.Value.UpgradeAction(LevelManager.Instance.CurrentPlayableCharacters[0]);
                    }
                }
            }
        }

		public virtual void OnMMEvent(PaywallCreditsEvent creditsEvent) {
			switch (creditsEvent.MoneyMethod) {
				case MoneyMethods.Add:
					AddMoney(creditsEvent.MoneyType, creditsEvent.Money);
					SaveProgress();
					break;
				case MoneyMethods.Set:
					SetMoney(creditsEvent.MoneyType, creditsEvent.Money);
					SaveProgress();
					break;
			}
		}

		/// <summary>
		/// On an upgrade event, update the upgrade data in the progress manager and save the game
		/// </summary>
		/// <param name="upgradeEvent"></param>
		public virtual void OnMMEvent(PaywallUpgradeEvent upgradeEvent) {
			if (Upgrades.TryGetValue(upgradeEvent.Upgrade.UpgradeName, out Upgrade upgrade)) {
				if (upgrade.Unlocked) {
					return;
                }

				// Do nothing if there is insufficient funds
				if (upgrade.MoneyType == MoneyTypes.Trinket) {
					if (upgrade.Cost > Trinkets) {
						return;
                    }
                } else {
					if (upgrade.Cost > Trinkets) {
						return;
					}
				}

				Upgrades[upgradeEvent.Upgrade.UpgradeName].UnlockUpgrade();
				PaywallCreditsEvent.Trigger(upgrade.MoneyType, MoneyMethods.Add, -upgradeEvent.Upgrade.Cost);
				SaveProgress();
			}
		}

		public virtual void OnMMEvent(MMGameEvent gameEvent) {
			if (gameEvent.EventName == "Save") {
				SaveProgress();
            }
        }

		public virtual void OnMMEvent(EmailEvent emailEvent) {
			if (emailEvent.EventType == EmailEventType.ContentChanged) {
				EmailItems = emailEvent.EmailItems;
				SaveProgress();
            }
        }

		public virtual void OnMMEvent(PaywallLevelEndEvent endEvent) {
			LevelEnd(endEvent.convertCredits);
        }

		/// <summary>
		/// OnEnable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable() {
			this.MMEventStartListening<PaywallCreditsEvent>();
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<PaywallUpgradeEvent>();
			this.MMEventStartListening<PaywallLevelEndEvent>();
			this.MMEventStartListening<EmailEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable() {
			this.MMEventStopListening<PaywallCreditsEvent>();
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<PaywallUpgradeEvent>();
			this.MMEventStopListening<PaywallLevelEndEvent>();
			this.MMEventStopListening<EmailEvent>();
		}

	}
}
