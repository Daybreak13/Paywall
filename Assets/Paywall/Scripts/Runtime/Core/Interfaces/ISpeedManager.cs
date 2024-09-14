using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Interface for getting and setting float speed
    /// To be used by any SpeedManager classes
    /// </summary>
    public interface ISpeedManager {
        public float GetSpeed();
        public void SetSpeed(float speed);
    }
}
