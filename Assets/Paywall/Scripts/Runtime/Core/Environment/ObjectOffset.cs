using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// If an object has this component, the spawner will spawn it at the given offset
    /// </summary>
    public class ObjectOffset : MonoBehaviour {
        [field: SerializeField] public Vector3 SpawnOffset { get; protected set; }
    }
}
