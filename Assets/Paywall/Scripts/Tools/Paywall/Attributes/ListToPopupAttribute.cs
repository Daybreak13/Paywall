using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall;
using UnityEditor;
using System;

namespace Paywall.Tools {

    public class ListToPopupAttribute : PropertyAttribute {
        public Type myType;
        public string propertyName;

        public ListToPopupAttribute(Type type, string propName) {
            myType = type;
            propertyName = propName;
        }
    }

}
