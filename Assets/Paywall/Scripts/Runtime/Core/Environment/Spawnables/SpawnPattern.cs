using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall {

    /// <summary>
    /// Represents a spawn pattern
    /// Draws gizmos indicating location of pattern transforms
    /// </summary>
    public class SpawnPattern : MonoBehaviour {

        protected virtual void OnDrawGizmos() {
            if ((Selection.activeGameObject == null) || ((Selection.activeGameObject != transform.gameObject) && (!Selection.activeGameObject.transform.IsChildOf(transform)))) {
                return;
            }
            Gizmos.color = Color.red;
            foreach (Transform child in transform) {
                Gizmos.DrawWireSphere(child.position, 0.2f);
            }
        }

    }
}
