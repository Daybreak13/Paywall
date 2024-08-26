using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Paywall.Tools {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class FieldConditionAttribute : PropertyAttribute {
		public string ConditionBoolean = string.Empty;
		public bool Hidden = false;
		public bool Negative = false;

		public FieldConditionAttribute(string conditionBoolean) {
			this.ConditionBoolean = conditionBoolean;
			this.Hidden = false;
		}

		public FieldConditionAttribute(string conditionBoolean, bool hideInInspector) {
			this.ConditionBoolean = conditionBoolean;
			this.Hidden = hideInInspector;
			this.Negative = false;
		}

		public FieldConditionAttribute(string conditionBoolean, bool hideInInspector, bool negative) {
			this.ConditionBoolean = conditionBoolean;
			this.Hidden = hideInInspector;
			this.Negative = negative;
		}
	}
}
