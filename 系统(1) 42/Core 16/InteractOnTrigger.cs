using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit3D
{
    [RequireComponent(typeof(Collider))]//添加collider到游戏物体
    public class InteractOnTrigger : MonoBehaviour
    {
        public LayerMask layers;//LayerMask实际上是位掩码，在Unity3d中Layers一共有32层，这个是不能增加或者减少的:LayerMask实际上是用Int32的32个位来表示每个层级，当这个位为1时表示使用这个层，为0时表示不用这个层。
        public UnityEvent OnEnter, OnExit;
        new Collider collider;
        public InventoryController.InventoryChecker[] inventoryChecks;//清单检查类的对象数组

        void Reset()//重置
        {
            layers = LayerMask.NameToLayer("Everything");//返回的该名字所定义的层的层索引
            collider = GetComponent<Collider>();//碰撞器的获取
            collider.isTrigger = true;//碰撞检测开启
        }

        void OnTriggerEnter(Collider other)//在碰撞器内
        {
            if (0 != (layers.value & 1 << other.gameObject.layer))//表示开启此游戏对象的层，如果此时层的状态与该物体层开启时的层状态相同，就对该对象内的所有道具进行逐一检测
            {                                                          //按位与&：一一对应的两个二进制位为1时结果位才为1，否则为0
                ExecuteOnEnter(other);//对碰撞器内对象进行逐一检测
            }
        }

        protected virtual void ExecuteOnEnter(Collider other)
        {
            OnEnter.Invoke();//唤醒事件
            for (var i = 0; i < inventoryChecks.Length; i++)
            {
                inventoryChecks[i].CheckInventory(other.GetComponentInChildren<InventoryController>());//碰撞物体的子物体上的inventorycontroller组件，函数当清单内的所有道具都存在时，返回true,否则返回false
            }
        }

        void OnTriggerExit(Collider other)//在碰撞器外
        {
            if (0 != (layers.value & 1 << other.gameObject.layer))
            {
                ExecuteOnExit(other);
            }
        }

        protected virtual void ExecuteOnExit(Collider other)
        {
            OnExit.Invoke();
        }

        void OnDrawGizmos()// OnDrawGizmos在每一帧都被调用。所有在OnDrawGizmos内部渲染的Gizmos都是可见的。
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);//  在物体位置处绘制图标，图片要放在Gizmos文件夹下，用来辨识不可见的空物体。设定为不允许缩放
        }

        void OnDrawGizmosSelected()
        {
            //need to inspect events and draw arrows to relevant gameObjects.
        }

    } 
}
