using Paywall.Tools;
using UnityEditor;
using UnityEngine;

namespace Paywall.Editors {

    /// <summary>
    /// Custom editor for resizer that redraws the collider when changes to the sprite renderer are made
    /// </summary>
    [CustomEditor(typeof(EdgeColliderResizer))]
    public class EdgeColliderResizerEditor : Editor {
        private EdgeColliderResizer script {
            get {
                return (EdgeColliderResizer)target;
            }
        }

        private bool isUpdating;

        private void OnEnable() {
            isUpdating = false;
        }

        private void OnSceneGUI() {
            if (!isUpdating) {
                isUpdating = true;
                script.Resize();
                isUpdating = false;
            }
        }
    }
}
