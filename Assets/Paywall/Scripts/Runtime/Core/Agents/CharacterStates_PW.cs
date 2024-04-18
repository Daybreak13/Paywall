using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class CharacterStates_PW {
        public enum ConditionStates {
            Normal,
            Dodging,
            Parrying
        }

        public enum MovementStates {
            Null,
            Running,
            Jumping,
            Falling,
            Stalling
        }
    }
}