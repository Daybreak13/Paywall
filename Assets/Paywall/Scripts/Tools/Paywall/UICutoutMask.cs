using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Paywall.Tools {

    public class UICutoutMask : Image {
        [field: SerializeField] public bool InvertMask { get; protected set; } = true;

        public override Material materialForRendering {
            get {
                if (!InvertMask) {
                    return base.materialForRendering;
                }
                Material material = new Material(base.materialForRendering);
                material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return material;
            }
        }
    }
}
