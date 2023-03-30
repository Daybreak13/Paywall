using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall.Tools {

    public class FindChild : MonoBehaviour {
        /// <summary>
        ///  Gets the first active button in the display
        /// </summary>
        /// <returns></returns>
        public static GameObject GetFirstActiveButton(Transform tform) {
            for (int i = 0; i < tform.childCount; i++) {

                if (tform.GetChild(i).gameObject.activeSelf) {
                    // Since the child i is the button's container, we return the first child of the container (which is the associated button)
                    return tform.GetChild(i).GetChild(0).gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively traverses children with a BFS to find a child
        /// </summary>
        /// <param name="aParent"></param>
        /// <param name="aName"></param>
        /// <returns></returns>
        public static Transform FindDeepChild(Transform aParent, string aName) {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0) {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}
