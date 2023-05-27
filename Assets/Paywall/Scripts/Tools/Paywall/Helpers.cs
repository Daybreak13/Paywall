using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public enum ElasticCollisionReturns { VFinal1, VFinal2 }

    public static class Physics {
        public static float ElasticCollision(float vi1, float vi2, float m1, float m2, ElasticCollisionReturns method) {
            if (method == ElasticCollisionReturns.VFinal1) {
                float vf1 = ((m1 - m2) * vi1 + 2f * m2 * vi2) / (m1 + m2);
                return vf1;
            }
            else {
                float vf2 = ((m2 - m1) * vi2 + 2 * m1 * vi1) / (m1 + m2);
                return vf2;
            }
        }
        public static float KineticFriction(float g, float s) {
            float a = -s * g;
            return a;
        }
    }

}
