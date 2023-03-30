using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Compares SpawnableObjects
    /// </summary>
    public class SpawnableComparer : IComparer<SpawnableObject> {
        public int Compare(SpawnableObject x, SpawnableObject y) {
            if (x.spawnChance < y.spawnChance) {
                return 1;
            } else if (x.spawnChance == y.spawnChance) {
                return 0;
            } else {
                return -1;
            }
        }
    }
}
