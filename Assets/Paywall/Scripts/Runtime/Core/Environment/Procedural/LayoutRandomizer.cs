using System.Collections.Generic;
using UnityEngine;
using Weighted_Randomizer;

namespace Paywall
{

    /// <summary>
    /// List of gameobjects belonging to a certain layout
    /// </summary>
    [System.Serializable]
    public class SegmentLayout
    {
        /// List of gameobjects belonging to this layout
        [field: Tooltip("List of gameobjects belonging to this layout")]
        [field: SerializeField] public List<GameObject> Layout = new();
        /// Weight of this layout
        [field: Tooltip("Weight of this layout")]
        [field: SerializeField] public int Weight;
    }

    public class LayoutRandomizer : MonoBehaviour
    {
        /// List of possible layouts and their weights
        [field: Tooltip("List of possible layouts and their weights")]
        [field: SerializeField] public List<SegmentLayout> Layouts { get; protected set; } = new();

        protected IWeightedRandomizer<int> _layoutRandomizer = new DynamicWeightedRandomizer<int>();
        protected SegmentLayout _currentLayout;

        protected virtual void Awake()
        {
            for (int i = 0; i < Layouts.Count; i++)
            {
                _layoutRandomizer.Add(i, Layouts[i].Weight);
            }
            foreach (SegmentLayout layout in Layouts)
            {
                foreach (GameObject obj in layout.Layout)
                {
                    obj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Randomly selects a layout of gameobjects to use, then sets it to active
        /// </summary>
        protected virtual void InitializeLayout()
        {
            int key = _layoutRandomizer.NextWithReplacement();
            _currentLayout = Layouts[key];
            // Activate the objects included in the layout
            foreach (GameObject obj in _currentLayout.Layout)
            {
                obj.SetActive(true);
            }
        }

        protected virtual void OnEnable()
        {
            InitializeLayout();
        }

        /// <summary>
        /// Disable the current layout
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_currentLayout == null)
            {
                return;
            }
            foreach (GameObject obj in _currentLayout.Layout)
            {
                obj.SetActive(false);
            }
        }
    }
}
