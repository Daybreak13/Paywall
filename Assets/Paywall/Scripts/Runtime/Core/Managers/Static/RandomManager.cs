using System;
using System.Collections.Generic;

namespace Paywall {

    /// <summary>
    /// Static class containing references to rngs
    /// </summary>
    public static class RandomManager {
        public static Dictionary<Guid, System.Random> RandomGenerators = new();

        public static System.Random NewRandom() {
            Guid g = Guid.NewGuid();
            System.Random r = new();
            RandomGenerators.Add(g, r);
            return r;
        }

        public static System.Random NewRandom(int seed) {
            Guid g = Guid.NewGuid();
            System.Random r = new(seed);
            RandomGenerators.Add(g, r);
            return r;
        }
    }
}
