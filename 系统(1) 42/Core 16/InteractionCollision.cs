using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit3D
{
    [RequireComponent(typeof(Collider))]
    public class InteractionCollision : MonoBehaviour//碰撞互动
    {
        public LayerMask layers;//当前所在层
        public UnityEvent OnCollision;//碰撞事件

        void Reset()//重置
        {
            layers = LayerMask.NameToLayer("Everything");//将当前层设置在Everything层
        }

        void OnCollisionEnter(Collision c)//碰撞开始时
        {
            Debug.Log(c);//在控制台打印出碰撞物的信息
            if (0 != (layers.value & 1 << c.transform.gameObject.layer))//如果碰撞了当前层就将碰撞事件唤醒
            {
                OnCollision.Invoke();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);//在当前位置绘制一个图标，图标名称为InteractionTrigger
        }

        void OnDrawGizmosSelected()
        {
            //需要检查事件并画出相关游戏对象的箭头。 need to inspect events and draw arrows to relevant gameObjects.
        }

    } 
}
