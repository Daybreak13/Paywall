using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using UnityEngine.SceneManagement;

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
		public Dictionary<string, ScriptableUpgrade> Upgrades = new Dictionary<string, ScriptableUpgrade>();
		public PaywallScene[] Scenes;
	}

	/// <summary>
	/// Creates save files for game progress and handles saves and loads
	/// Every time credits are updated, save the game
	/// </summary>
	public class PaywallProgressManager : MMSingleton<PaywallProgressManager>, 
		MMEventListener<CorgiEngineEvent>, MMEventListener<PaywallCreditsEvent>, 
		MMEventListener<MMGameEvent>, MMEventListener<PaywallUpgradeEvent>, MMEventListener<PaywallLevelEndEvent> {

		public int InitialMaximumLives { get; set; }
		public int InitialCurrentLives { get; set; }

		/// the list of scenes that we'll want to consider for our game
		[Tooltip("the list of scenes that we'll want to consider for our game")]
		public PaywallScene[] Scenes;

		[MMInspectorButton("CreateSaveGame")]
		/// A button to test creating the save file
		public bool CreateSaveGameBtn;

		/// Credits, readonly, for debug purposes only. Comment out Update for final build.
		[Tooltip("Credits, readonly, for debug purposes only. Comment out Update for final build.")]
		[MMReadOnly]
		[field: SerializeField] private int displayedCredits;

		/// Credits are the "real world" currency
		[Tooltip("Credits are the \"real world\" currency")]
		[field:SerializeField] public int Credits { get; private set; }
		/// Trinkets are the "game world" currency
		[Tooltip("Trinkets are the \"game world\" currency")]
		public int Trinkets { get; private set; }
		public Dictionary<string, ScriptableUpgrade> Upgrades { get; private set; }
		public static Progress persistentProgress { get; private set; }

		protected const string _saveFolderName = "PaywallProgress";
		protected const string _saveFileName = "Progress.data";

		/// <summary>
		/// On awake, we load our progress
		/// </summary>
		protected override void Awake() {
			base.Awake();
			LoadSavedProgress();
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
		protected virtual void SaveProgress() {
			Progress progress = new Progress();
			progress.MaximumLives = GameManager.Instance.MaximumLives;
			progress.CurrentLives = GameManager.Instance.CurrentLives;
			
			progress.Scenes = Scenes;
			progress.Credits = Credits;
			progress.Trinkets = Trinkets;
			progress.Upgrades = Upgrades;

			persistentProgress = progress;

			MMSaveLoadManager.Save(progress, _saveFileName, _saveFolderName);
		}

		/// <summary>
		/// Called when the level ends.
		/// Convert points, save progress.
		/// </summary>
		protected virtual void LevelEnd() {
			ConvertPointsToCredits();
			SaveProgress();
        }

		/// <summary>
		/// Converts current points to credits
		/// </summary>
		protected virtual void ConvertPointsToCredits() {
			if (GameManager.HasInstance) {
				Credits += GameManager.Instance.Points;
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
				GameManager.Instance.MaximumLives = progress.MaximumLives;
				GameManager.Instance.CurrentLives = progress.CurrentLives;
				InitialMaximumLives = progress.InitialMaximumLives;
				InitialCurrentLives = progress.InitialCurrentLives;
				Scenes = progress.Scenes;
				Credits = progress.Credits;
				Trinkets = progress.Trinkets;
				Upgrades = progress.Upgrades;
			}
			else {
				InitialMaximumLives = GameManager.Instance.MaximumLives;
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
			if (LevelManager.HasInstance && (LevelManager.Instance.Players.Count > 0) && (Upgrades != null)) {
				foreach (KeyValuePair<string, ScriptableUpgrade> entry in Upgrades) {
					if (entry.Value.UpgradeType == UpgradeTypes.Player) {
						entry.Value.UpgradeAction(LevelManager.Instance.Players[0]);
                    }
                }
            }
        }

		/// <summary>
		/// When we grab a level complete event, we update our status, and save our progress to file
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public virtual void OnMMEvent(CorgiEngineEvent gameEvent) {
			switch (gameEvent.EventType) {
				case CorgiEngineEventTypes.LevelComplete:
					LevelComplete();
					break;
				case CorgiEngineEventTypes.GameOver:
					//LevelEnd();
					break;
				case CorgiEngineEventTypes.LevelEnd:
					//LevelEnd();
					break;
				case CorgiEngineEventTypes.LevelStart:
					//ApplyUpgradesToCharacter();
					break;
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
			if (Upgrades.TryGetValue(upgradeEvent.Upgrade.UpgradeName, out ScriptableUpgrade upgrade)) {
				if (upgrade.Unlocked) {
					return;
                }

				if (upgrade.MoneyType == MoneyTypes.Trinket) {
					if (upgrade.Cost > Trinkets) {
						return;
                    }
                } else {
					if (upgrade.Cost > Trinkets) {
						return;
					}
				}

				Upgrades[upgradeEvent.Upgrade.UpgradeName] = upgradeEvent.Upgrade;
				PaywallCreditsEvent.Trigger(upgrade.MoneyType, MoneyMethods.Add, -upgradeEvent.Upgrade.Cost);
				SaveProgress();
			}
		}

		public virtual void OnMMEvent(MMGameEvent gameEvent) {
			if (gameEvent.EventName == "Save") {
				SaveProgress();
            }
        }

		public virtual void OnMMEvent(PaywallLevelEndEvent endEvent) {
			LevelEnd();
        }

		/// <summary>
		/// OnEnable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable() {
			this.MMEventStartListening<CorgiEngineEvent>();
			this.MMEventStartListening<PaywallCreditsEvent>();
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<PaywallUpgradeEvent>();
			this.MMEventStartListening<PaywallLevelEndEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable() {
			this.MMEventStopListening<CorgiEngineEvent>();
			this.MMEventStopListening<PaywallCreditsEvent>();
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<PaywallUpgradeEvent>();
			this.MMEventStopListening<PaywallLevelEndEvent>();
		}

	}
}
