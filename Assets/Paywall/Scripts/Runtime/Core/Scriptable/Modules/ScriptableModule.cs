using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

    /// <summary>
    /// SO for modules
    /// Contains active and enhanced data, and module name
    /// </summary>
    [CreateAssetMenu(fileName = "Module", menuName = "Paywall/Modules/Module")]
    public class ScriptableModule : BaseScriptableDepotItem {
        [field: Header("Module")]

        /// Is this module currently activated
        [field: Tooltip("Is this module currently activated")]
        [field: SerializeField] public bool IsActive { get; protected set; }
        /// Is this module currently enhanced
        [field: Tooltip("Is this module currently enhanced")]
        [field: SerializeField] public bool IsEnhanced { get; protected set; }
        /// Shop description of the enhanced version of this module
        [field: Tooltip("Shop description of the enhanced version of this module")]
        [field: TextArea]
        [field: SerializeField] public string EnhancedDescription { get; protected set; }

        public virtual void SetModuleActive(bool active) {
            IsActive = active;
            PaywallModuleEvent.Trigger(this);
        }

        public virtual void SetModuleEnhanced(bool enhanced) {
            IsEnhanced = enhanced;
            PaywallModuleEvent.Trigger(this);
        }
    }
}
