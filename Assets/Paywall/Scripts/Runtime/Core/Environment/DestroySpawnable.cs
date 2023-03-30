using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class DestroySpawnable : MonoBehaviour {
        protected float _leftEdge;
        protected float _buffer = 2f;

        protected virtual void Start() {
            _leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - _buffer;
        }


    }
}
