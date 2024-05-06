using System;
using System.Collections.Generic;

namespace Paywall {

    public struct RandomGenerator {
        public Guid GUID { get; set; }
        public Random RNG { get; set; }
    }

    /// <summary>
    /// Static class containing references to rngs
    /// </summary>
    public static class RandomManager {
        public static Dictionary<Guid, Random> RandomGenerators = new();

        public static Random NewRandom() {
            Guid g = Guid.NewGuid();
            Random r = new();
            RandomGenerators.Add(g, r);
            return r;
        }

        public static Random NewRandom(int seed) {
            Guid g = Guid.NewGuid();
            Random r = new(seed);
            RandomGenerators.Add(g, r);
            return r;
        }
    }
}
