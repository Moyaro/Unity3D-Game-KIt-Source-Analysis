using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class RigidbodyDelayedForce : MonoBehaviour
    {
        public Vector3 forceToAdd;//受力方向

        private void Start()
        {
            Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();//获取子对象所有的刚体组件
            
            //
            for (int i = 0; i < rigidbodies.Length; ++i)
            {
                rigidbodies[i].maxAngularVelocity = 45;
                rigidbodies[i].angularVelocity = transform.right * -45.0f;
                rigidbodies[i].velocity = forceToAdd;

            }
        }
    } 
}
