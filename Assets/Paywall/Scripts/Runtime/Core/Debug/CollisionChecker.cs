using UnityEngine;

namespace Paywall
{

    public class CollisionChecker : MonoBehaviour
    {
        protected int _counter = 0;
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                Debug.Log("Collision detected: " + ++_counter);
            }
        }
    }
}
