using MoreMountains.Tools;
using UnityEngine;

namespace Paywall
{

    public enum UIEventTypes { Click, Select }

    public struct PWUIEvent
    {
        public GameObject Obj;
        public UIEventTypes EventType;

        public PWUIEvent(GameObject o, UIEventTypes eventType)
        {
            Obj = o;
            EventType = eventType;
        }
        static PWUIEvent e;
        public static void Trigger(GameObject o, UIEventTypes eventType)
        {
            e.Obj = o;
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }

    public enum PWInputEventTypes { Back }

    public struct PWInputEvent
    {
        public PWInputEventTypes EventType;

        public PWInputEvent(PWInputEventTypes eventType)
        {
            EventType = eventType;
        }
        static PWInputEvent e;
        public static void Trigger(PWInputEventTypes eventType)
        {
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }

}