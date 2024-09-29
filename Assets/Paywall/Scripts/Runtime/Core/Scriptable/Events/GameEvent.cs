using System.Collections.Generic;
using UnityEngine;

namespace Paywall
{

    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> listeners = new();

        public void Trigger()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventTriggered();
            }
        }
        public void RegisterListener(GameEventListener listener)
        {

        }
        public void UnregisterListener(GameEventListener listener)
        {

        }
    }
}
