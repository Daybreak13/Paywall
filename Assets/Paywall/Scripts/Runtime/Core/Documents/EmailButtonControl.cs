using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class EmailButtonControl : MonoBehaviour {
        public EmailItem emailItem { get; protected set; }

        public virtual void MarkAsRead() {
            if (emailItem != null) {
                emailItem.SetRead(true);
            }
        }

        public virtual void SetItem(EmailItem item) {
            emailItem = item;
        }

    }
}
