using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class MultiPointSpawnerProcedural : MultiPointSpawner {
        protected override void Initialization() {
            if (!_initialized) {

                _initialized = true;
            }
        }
    }
}
