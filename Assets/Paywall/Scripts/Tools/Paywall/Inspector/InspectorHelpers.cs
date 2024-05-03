using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Paywall.Tools {

    public class InspectorHelpers {
        [MenuItem("GameObject/Sort Children by Name", false, 10)]
        static void SortChildrenByName() {
            // Get all selected GameObjects
            Transform[] selectedTransforms = Selection.transforms;
            if (selectedTransforms == null || selectedTransforms.Length == 0) {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            // Create an undo group for the operation
            Undo.SetCurrentGroupName("Sort Children by Name");
            int group = Undo.GetCurrentGroup();

            foreach (Transform selectedTransform in selectedTransforms) {
                // Get all child GameObjects of the selected GameObject
                Transform[] childTransforms = selectedTransform.Cast<Transform>().ToArray();

                // Sort child GameObjects by name
                childTransforms = childTransforms.OrderBy(t => t.name).ToArray();

                // Record initial sibling indices for undo
                int[] initialSiblingIndices = childTransforms.Select(t => t.GetSiblingIndex()).ToArray();

                // Reorder the child GameObjects based on the sorted array
                for (int i = 0; i < childTransforms.Length; i++) {
                    //Undo.SetTransformParent(childTransforms[i], selectedTransform, "Sort Children by Name");
                    Undo.RegisterCompleteObjectUndo(childTransforms[i], "Sort Children by Name");
                    Undo.RecordObject(childTransforms[i], "Sort Children by Name");
                    childTransforms[i].SetSiblingIndex(i);
                }

                // Register undo operation to revert changes
                //for (int i = 0; i < childTransforms.Length; i++) {
                //    Undo.RegisterCompleteObjectUndo(childTransforms[i], "Sort Children by Name");
                //    Undo.RecordObject(childTransforms[i], "Sort Children by Name");
                //    childTransforms[i].SetSiblingIndex(initialSiblingIndices[i]);
                //}
            }

            // End the undo group
            Undo.CollapseUndoOperations(group);
        }

    }
}
