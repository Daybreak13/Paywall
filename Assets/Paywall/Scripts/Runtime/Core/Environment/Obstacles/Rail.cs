using UnityEngine;

namespace Paywall
{

    public class Rail : MonoBehaviour
    {
        /// The collider in the shape of this rail
        [field: Tooltip("The collider in the shape of this rail")]
        [field: SerializeField] public EdgeCollider2D EdgeCollider { get; protected set; }
        /// The collider used to detect collision with the player
        [field: Tooltip("The collider used to detect collision with the player")]
        [field: SerializeField] public BoxCollider2D Box { get; protected set; }

    }
}
