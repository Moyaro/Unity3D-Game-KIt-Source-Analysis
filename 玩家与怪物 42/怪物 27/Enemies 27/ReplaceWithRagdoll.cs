using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    //本类是对怪物处于某种动画状态时进行的一些处理，包括生成玩偶预制件，保存当前怪物收到的攻击的力，在某些状态下对怪物原件和生成的玩偶预制件进行某些处理，最后销毁怪物原件
    public class ReplaceWithRagdoll : MonoBehaviour//unity中的布娃娃系统：https://www.cnblogs.com/zhaoqingqing/p/4161199.html
    {
        public GameObject ragdollPrefab;

        public void Replace()
        {
            GameObject ragdollInstance = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            //need to disable it, otherwise when we copy over the hierarchy objects position/rotation, the ragdoll will try each time to 
            //"correct" the attached joint, leading to a deformed/glitched instance
            //如果不关闭，在修改布娃娃系统的骨骼时布娃娃系统自己会尝试修正，导致变形
            ragdollInstance.SetActive(false);

            EnemyController baseController = this.GetComponent<EnemyController>();

            RigidbodyDelayedForce t = ragdollInstance.AddComponent<RigidbodyDelayedForce>();//t这个刚体延迟力的作用是作为当前这个类被调用时的一个变量
            //他对当前脚本的其他变量没有影响，意义是充当一个记录被攻击的状态的中间量
            t.forceToAdd = baseController.externalForce;

            Transform ragdollCurrent = ragdollInstance.transform;//生成的怪物预制件的位置
            Transform current = transform;//最初怪物自身的位置
            bool first = true;

            while (current != null && ragdollCurrent != null)
            {
                if (first || ragdollCurrent.name == current.name)//当生成的怪物预制件和最初的怪物名称相同时，将预制件放到最初怪物所处的位置和方向
                {
                    //we only match part of the hierarchy that are named the same, except for the very first, as the 2 objects will have different name (but must have the same skeleton)
                    ragdollCurrent.rotation = current.rotation;
                    ragdollCurrent.position = current.position;
                    first = false;
                }

                if (current.childCount > 0)//？？
                {
                    // Get first child.
                    current = current.GetChild(0);//current保存怪物身上的模型
                    ragdollCurrent = ragdollCurrent.GetChild(0);//保存生成的怪物预制件身上的模型
                }
                else
                {
                    while (current != null)
                    {
                        if (current.parent == null || ragdollCurrent.parent == null)//？？最初的怪物和生成的怪物预制件都没有父物体时
                        {
                            // No more transforms to find.
                            current = null;
                            ragdollCurrent = null;
                        }
                        else if (current.GetSiblingIndex() == current.parent.childCount - 1 ||
                                 current.GetSiblingIndex() + 1 >= ragdollCurrent.parent.childCount)//?？？
                        {
                            // Need to go up one level.
                            current = current.parent;
                            ragdollCurrent = ragdollCurrent.parent;
                        }
                        else
                        {
                            // Found next sibling for next iteration.反复？？？
                            current = current.parent.GetChild(current.GetSiblingIndex() + 1);
                            ragdollCurrent = ragdollCurrent.parent.GetChild(ragdollCurrent.GetSiblingIndex() + 1);
                            break;
                        }
                    }
                }
            }


            ragdollInstance.SetActive(true);
            Destroy(gameObject);
        }
    }
}