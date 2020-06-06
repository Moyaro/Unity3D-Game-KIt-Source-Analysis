using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit3D
{
    [RequireComponent(typeof(SphereCollider))]//添加组件球体碰撞器
    public class InteractionTrigger : MonoBehaviour//交互触发器
    {
        public LayerMask layers;//定义图层蒙版，只选定Layermask层内的碰撞器，其它层内碰撞器忽略。
        public UnityEvent OnEnter, OnExit;
        new SphereCollider collider;//定义球形碰撞器

        void Reset()//重置
        {
            layers = LayerMask.NameToLayer("Everything");//将名为everything的图层蒙版赋给layers
            collider = GetComponent<SphereCollider>();//访问游戏对象的球形碰撞器赋给collider
            collider.radius = 5;//球形碰撞器半径为5
            collider.isTrigger = true;//碰撞器的触发器开启
        }

        void OnTriggerEnter(Collider other)//触发器被触发时执行
        {
            if (0 != (layers.value & 1 << other.gameObject.layer))
            //如果图层的属性与1右移这个位数之后的结果不为0
            {
                OnEnter.Invoke();//在OE里调用这个other碰撞器
            }
        }

        void OnTriggerExit(Collider other)//触发器停止时执行
        {
            if (0 != (layers.value & 1 << other.gameObject.layer))
            //如果图层的属性与1右移这个位数之后的结果不为0
            {
                OnExit.Invoke();//在OE里调用这个other碰撞器
            }
        }

        void OnDrawGizmos()//绘制小控件
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);
            //游戏场景内放置名为“InteractionTrigger”的图标
        }

        void OnDrawGizmosSelected()//选定绘制小控件时
        {
            //need to inspect events and draw arrows to relevant gameObjects.
            //需要检查事件并向相关的游戏对象绘制箭头
        }

    } 
}
