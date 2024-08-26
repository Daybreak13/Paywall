using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Paywall.Tools {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class FieldNullConditionAttribute : PropertyAttribute {
        public string FieldName = string.Empty;
        public bool Hidden = false;
        public bool Negative = false;

        public FieldNullConditionAttribute(string fieldName) {
            FieldName = fieldName;
            Hidden = false;
        }

        public FieldNullConditionAttribute(string fieldName, bool hideInInspector) {
            FieldName = fieldName;
            Hidden = hideInInspector;
        }

        public FieldNullConditionAttribute(string fieldName, bool hideInInspector, bool negative) {
            FieldName = fieldName;
            Hidden = hideInInspector;
            Negative = negative;
        }
    }
}
