using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class Testing : MonoBehaviour {

        protected virtual void Start() {
            int t = GetComponent<Tester>().Test;
            t = 10;
        }
    }
}
