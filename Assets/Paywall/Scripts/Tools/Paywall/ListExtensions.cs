using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paywall.Tools {

    public static class ListExtensions {

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

    }
}
