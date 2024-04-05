using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Serializable class to hold mutable module data during runtime
    /// Used by progress manager
    /// </summary>
    [Serializable]
    public class ModuleData : DepotItemData {
        /// Is this module currently activated
        [field: Tooltip("Is this module currently activated")]
        [field: SerializeField] public bool IsActive { get; set; }
        /// Is this module currently enhanced
        [field: Tooltip("Is this module currently enhanced")]
        [field: SerializeField] public bool IsEnhanced { get; set; }

        [field: NonSerialized]
        public ScriptableModule Module { get; set; }

        public ModuleData(string name, bool isActive, bool isEnhanced, bool isValid, ScriptableModule module) : base(name, isValid, module) {
            Name = name;
            IsActive = isActive;
            IsEnhanced = isEnhanced;
            IsValid = isValid;
            Module = module;
        }
    }
}
