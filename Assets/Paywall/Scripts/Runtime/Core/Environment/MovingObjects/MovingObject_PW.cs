using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    public class MovingObject_PW : MovingObject {
        /// If true, use the EnemySpeed parameter of LevelManager instead of regular Speed
        [Tooltip("If true, use the EnemySpeed parameter of LevelManager instead of regular Speed")]
        [field: SerializeField] public bool UseEnemySpeed { get; protected set; }

        public override void Move() {
			if (!LevelManagerIRE_PW.HasInstance) {
				_movement = Direction * (Speed / 10) * Time.deltaTime;
			}
			else {
				if (UseEnemySpeed) {
					_movement = Direction * (Speed / 10) * (LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).EnemySpeed * Time.deltaTime;					
				}
				else {
					_movement = Direction * (Speed / 10) * LevelManager.Instance.Speed * Time.deltaTime;
				}
			}
			transform.Translate(_movement, MovementSpace);
			//MMDebug.DebugLogTime (this.name+"movement : " + _movement);
			// We apply the acceleration to increase the speed
			Speed += Acceleration * Time.deltaTime;
		}
    }
}
