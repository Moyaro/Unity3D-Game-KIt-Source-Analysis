using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit3D
{
    [RequireComponent(typeof(Collider))]
    public class InteractOnCollision : MonoBehaviour//碰撞时相互作用
    {
        public LayerMask layers;//层内碰撞器
        public UnityEvent OnCollision;

        void Reset()
        {
            layers = LayerMask.NameToLayer("Everything");//碰撞器名称
        }

        void OnCollisionEnter(Collision c)//碰撞
        {
            Debug.Log(c);//打印到控制台一下
            if (0 != (layers.value & 1 << c.transform.gameObject.layer))
            {
                OnCollision.Invoke();
            }
        }

        void OnDrawGizmos()//绘制小控件
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);
        }

        void OnDrawGizmosSelected()//绘制选定的小控件
        {
            //need to inspect events and draw arrows to relevant gameObjects.
        }

    } 
}
