using System.Collections.Generic;
using UnityEngine;

namespace Paywall
{

    public class UpgradeMenuDisplay : MonoBehaviour
    {

        /// List of upgrades
        [field: Tooltip("List of upgrades")]
        [field: SerializeField] public List<ScriptableUpgrade> Upgrades { get; protected set; }

        /// Prefab to use for the buttons
        [field: Tooltip("Prefab to use for the buttons")]
        [field: SerializeField] public GameObject ButtonPrefab { get; protected set; }


    }
}
