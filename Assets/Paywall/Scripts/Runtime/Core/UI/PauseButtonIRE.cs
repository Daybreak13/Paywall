using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    public class PauseButtonIRE : MonoBehaviour {
		/// Puts the game on pause
		public virtual void PauseButtonAction() {
			StartCoroutine(PauseButtonCo());
		}

		/// <summary>
		/// A coroutine used to trigger the pause event
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PauseButtonCo() {
			yield return null;
			if (GameManagerIRE_PW.HasInstance) {
				(GameManagerIRE_PW.Instance as GameManagerIRE_PW).Pause();
            }
		}

	}
}
