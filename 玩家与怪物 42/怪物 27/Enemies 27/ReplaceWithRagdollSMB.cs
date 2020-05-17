using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


namespace Gamekit3D
{
    //该类的作用是充当一个保存了重载在MachineExit状态和Exit状态下进行处理的方法的类，通过初始化这个类的对象来调用重载的这两个方法
    //这两个方法都是对当前这个状态进行ReplaceWithRagdoll中的处理
    public class ReplaceWithRagdollSMB : StateMachineBehaviour
    {
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            ReplaceWithRagdoll replacer = animator.GetComponent<ReplaceWithRagdoll>();
            replacer.Replace();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ReplaceWithRagdoll replacer = animator.GetComponent<ReplaceWithRagdoll>();
            replacer.Replace();
        }
    }
}