using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class SpitterSMBShoot : SceneLinkedSMB<SpitterBehaviour>
    {
        static int s_IdleStateHash = Animator.StringToHash("Idle");
        protected Vector3 m_AttackPosition;

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_MonoBehaviour.target == null)
            {
                //if we reached the shooting state without a target, mean the target move outside of our detection range
                //so just go back to idle.
                animator.Play(s_IdleStateHash);
                return;
            }


            m_MonoBehaviour.controller.SetFollowNavmeshAgent(false);//关闭导航器，休闲状态下的运动的目标位置受导航控制器控制

            m_MonoBehaviour.RememberTargetPosition();
            Vector3 toTarget = m_MonoBehaviour.target.transform.position - m_MonoBehaviour.transform.position;
            toTarget.y = 0;//？？

            m_MonoBehaviour.transform.forward = toTarget.normalized;//归一化，变成单位向量（1，2，3）=》（sqr(1/14)，sqr(4/14),sqr(9/14)）
            m_MonoBehaviour.controller.SetForward(m_MonoBehaviour.transform.forward);//按帧旋转

            if (m_MonoBehaviour.attackAudio != null)
                m_MonoBehaviour.attackAudio.PlayRandomClip();
        }

        //从状态转换开始后在第一帧调用。 请注意，如果过渡的持续时间少于一帧，则不会调用该过渡。
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.FindTarget();
        }
    }
}