using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    //对图层掩码进行拓展
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask layers, GameObject gameObject)
        {
            return 0 != (layers.value & 1 << gameObject.layer);
        }
    }
}