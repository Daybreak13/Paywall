using UnityEngine;

namespace Paywall
{

    public class InventoryItem_PW : MonoBehaviour
    {
        /// The item's ID
        [field: Tooltip("The item's ID")]
        [field: SerializeField] public string ItemID { get; protected set; }

        [field: Header("Basic info")]

        /// Item name
        [field: Tooltip("Item name")]
        public string ItemName;
        /// the item's short description
        [Tooltip("the item's short description")]
        [TextArea]
        public string ShortDescription;
        /// the item's long description
        [Tooltip("the item's long description")]
        [TextArea]
        public string Description;

        /// <summary>
        /// Determines if an item is null or not
        /// </summary>
        /// <returns><c>true</c> if is null the specified item; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        public static bool IsNull(InventoryItem_PW item)
        {
            if (item == null)
            {
                return true;
            }
            if (item.ItemID == null)
            {
                return true;
            }
            if (item.ItemID == string.Empty)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// What happens when the object is picked - override this to add your own behaviors
        /// </summary>
        public virtual bool Pick(string playerID) { return true; }

    }
}
