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

        /// <summary>
        /// Sets active state of gameobject if it is not null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="active"></param>
        public static void SetActiveIfNotNull(this GameObject obj, bool active) {
            if (obj != null) {
                obj.SetActive(active);
            }
        }

        /// <summary>
        /// Sets a transform's position without placing it inside another collider
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="destination"></param>
        /// <param name="layerMask"></param>
        public static void SafeSetTransformPosition(this Transform transform, Vector2 destination, LayerMask layerMask) {
            BoxCollider2D boxCollider = transform.GetComponent<BoxCollider2D>();

            // we do a first test to see if there's room enough to move to the destination
            Collider2D hit = Physics2D.OverlapBox(destination, boxCollider.size, transform.rotation.eulerAngles.z, layerMask);

            // If we found empty space, move the transform there
            if (hit == null) {
                transform.position = destination;
            } 
            // Otherwise, search for a safe location
            else {
                Vector2 newDestination = new Vector2(destination.x, destination.y + 0.1f);
                while ((newDestination.y - destination.y) <= 1f) {
                    hit = Physics2D.OverlapBox(newDestination, boxCollider.size, transform.rotation.eulerAngles.z, layerMask);
                    // If we found empty space, move the transform there
                    if (hit == null) {
                        transform.position = newDestination;
                        return;
                    }
                    newDestination = new Vector2(destination.x, destination.y + 0.1f);
                }
                // If we couldn't find any safe position, just set the transform position to the originally provided position
                transform.position = destination;
            }

        }


    }
}
