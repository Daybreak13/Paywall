using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Generates level segments randomly
    /// </summary>
    public class ProceduralLevelGenerator : MonoBehaviour {
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public List<LevelSegment> LevelSegments { get; protected set; }



        #region Editor

        public virtual LevelSegment[] GetAttachedSegments() {
            return LevelSegments.ToArray();
        }

        public virtual string[] GetAttachedSegmentNames() {
            string[] names = new string[LevelSegments.Count];
            for (int i = 1; i < LevelSegments.Count; i++) {
                names[i] = LevelSegments[i].Label;
            }
            return names;
        }

        public virtual string[] GetAttachedSegmentNamesSorted() {
            List<string> namesList = new();
            foreach (LevelSegment segment in LevelSegments) {
                namesList.Add(segment.Label);
            }
            namesList.Sort();
            
            return namesList.ToArray();
        }

        #endregion

    }
}
