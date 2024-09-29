using UnityEngine;
using UnityEngine.Events;

namespace Paywall
{

    public class GameEventListener : MonoBehaviour
    {
        public GameEvent Event;
        public UnityEvent Response;
        public void OnEventTriggered()
        {

        }
        private void OnEnable()
        {
            Event.RegisterListener(this);
        }
        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }
    }
}
