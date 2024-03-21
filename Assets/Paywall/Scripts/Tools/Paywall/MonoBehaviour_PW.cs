using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall.Tools {

    public class MonoBehaviour_PW : MonoBehaviour {
        protected virtual void OnDisable() {
            StopAllCoroutines();
        }

        protected virtual void OnDestroy() {
            StopAllCoroutines();
        }
    }
}
