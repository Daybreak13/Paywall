using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Compares DMMultipleObjectPoolerObject objects
    /// </summary>
    public class DMPoolComparer : IComparer<DMMultipleObjectPoolerObject> {
        public int Compare(DMMultipleObjectPoolerObject x, DMMultipleObjectPoolerObject y) {
            if (x.Chance < y.Chance) {
                return 1;
            } else if (x.Chance == y.Chance) {
                return 0;
            } else {
                return -1;
            }
        }
    }
}
