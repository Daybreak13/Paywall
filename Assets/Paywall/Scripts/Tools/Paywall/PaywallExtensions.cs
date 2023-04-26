using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace Paywall.Tools {

    public static class PaywallExtensions {

        /// <summary>
        /// Add to list if parameter is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="obj">Object to add to the list</param>
        public static void AddIfNotNull<T>(this List<T> list, T obj) {
            if (obj != null && (obj.ToString() != "null")) {
                list.Add(obj);
            }
        }

        /// <summary>
        /// Returns a serialized property that has autoproperties
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static SerializedProperty FindPropertyByAutoPropertyName(this SerializedObject obj, string propName) {
            return obj.FindProperty(string.Format("<{0}>k__BackingField", propName));
        }


    }
}
