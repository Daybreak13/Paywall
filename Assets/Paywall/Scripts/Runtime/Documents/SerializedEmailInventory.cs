using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paywall.Documents {

    [Serializable]
    public class SerializedEmailInventory {
        public Dictionary<string, EmailItem> EmailItems;
    }

}
