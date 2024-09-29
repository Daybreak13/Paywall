using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Add this to a tiled sprite to have its length randomly set OnEnable
    /// </summary>
    public class VariableTiledSprite : MonoBehaviour
    {
        /// Tiled sprite renderer
        [field: Tooltip("Tiled sprite renderer")]
        [field: SerializeField] public SpriteRenderer SpriteComponent { get; protected set; }
        /// Parent controller of this game object
        [field: Tooltip("Parent controller of this game object")]
        [field: SerializeField] public LevelSegmentController ParentController { get; protected set; }
        /// Minimum possible length
        [field: Tooltip("Minimum possible length")]
        [field: SerializeField] public int MinLength { get; protected set; } = 3;
        /// Maximum possible length
        [field: Tooltip("Maximum possible length")]
        [field: SerializeField] public int MaxLength { get; protected set; } = 3;
        /// Left gameobject in this segment, if applicable
        [field: Tooltip("Left gameobject in this segment")]
        [field: SerializeField] public Collider2D LeftEnd { get; protected set; }
        /// Right gameobject in this segment, if applicable
        [field: Tooltip("Right gameobject in this segment")]
        [field: SerializeField] public Collider2D RightEnd { get; protected set; }

        protected System.Random _rand;

        /// <summary>
        /// Set the size of this tile sprite, as well as adjusting the position of the gameobjects connected to its left and right
        /// </summary>
        /// <param name="len"></param>
        public virtual void SetSize(int len)
        {
            SpriteComponent.size = new Vector2(len, 1);
            if (LeftEnd != null)
            {
                LeftEnd.transform.position = new Vector2(SpriteComponent.bounds.min.x - LeftEnd.bounds.extents.x, LeftEnd.transform.position.y);
            }
            if (RightEnd != null)
            {
                RightEnd.transform.position = new Vector2(SpriteComponent.bounds.max.x + RightEnd.bounds.extents.x, RightEnd.transform.position.y);
            }
            if (ParentController != null && LeftEnd != null && RightEnd != null)
            {
                Vector2 leftBound = new(LeftEnd.bounds.min.x, LeftEnd.transform.position.y);
                Vector2 rightBound = new(RightEnd.bounds.max.x, RightEnd.transform.position.y);
                ParentController.SetBounds(ParentController.transform.InverseTransformPoint(leftBound), ParentController.transform.InverseTransformPoint(rightBound));
            }
        }

        protected virtual void OnEnable()
        {
            _rand ??= RandomManager.NewRandom();
            int len = _rand.Next(MinLength, MaxLength + 1);
            SetSize(len);
        }
    }
}
