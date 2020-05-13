using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    [DefaultExecutionOrder(9999)]//unty特性，默认情况下，不同的脚本的Awake/OnEnable/Update函数根据脚本的拖到Inspector上顺序依次调用。但是可以通过设置脚本执行顺序来调整这些函数的执行。
    //最后一个执行该脚本
    public class FixedUpdateFollow : MonoBehaviour
    {
        public Transform toFollow;

        private void FixedUpdate()
        {
            transform.position = toFollow.position;
            transform.rotation = toFollow.rotation;
        }
    } 
}
