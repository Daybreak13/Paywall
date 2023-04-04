using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using TMPro;

namespace Paywall {

    public class GUIManager_PW : GUIManager {
        [Header("TextMesh")]

        /// the points counter
		[Tooltip("the points counter")]
        public TextMeshProUGUI PointsTmp;
        /// the level display
		[Tooltip("the level display")]
        public TextMeshProUGUI LevelTmp;

        public override void SetHUDActive(bool state) {
			if (HUD != null) {
				HUD.SetActive(state);
			}
			if (PointsTmp != null) {
				PointsTmp.enabled = state;
			}
			if (LevelTmp != null) {
				LevelTmp.enabled = state;
			}
		}

        public override void RefreshPoints() {
			if (PointsTmp != null) {
				PointsTmp.text = GameManager.Instance.Points.ToString(PointsPattern);
			}
		}


    }
}
