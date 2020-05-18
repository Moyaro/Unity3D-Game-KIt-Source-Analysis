using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class GrenadierSMBRangeAttack : SceneLinkedSMB<GrenadierBehaviour>
    {
        public float growthTime = 2.0f;

        protected float m_GrowthTimer = 0;

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateEnter(animator, stateInfo, layerIndex);//需要看scenelinkedsmb中对base变量定义

            m_MonoBehaviour.RememberTargetPosition();//记录目标位置
            m_MonoBehaviour.grenadeLauncher.LoadProjectile();//发射器开始准备弹药

            m_MonoBehaviour.grenadeLauncher.loadedProjectile.transform.up = Vector3.up;//调整boss的弹药的垂直方向
            m_MonoBehaviour.grenadeLauncher.loadedProjectile.transform.forward = m_MonoBehaviour.transform.forward;//调整boss的弹药的向前方向

            m_GrowthTimer = 0.0f;
        }

        //状态转换的中间状态
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            m_GrowthTimer = Mathf.Clamp(m_GrowthTimer + Time.deltaTime, 0.0f, growthTime);
            if (m_MonoBehaviour.grenadeLauncher.loadedProjectile != null)
                m_MonoBehaviour.grenadeLauncher.loadedProjectile.transform.localScale =
                    Vector3.one * (m_GrowthTimer / growthTime);
        }
    }
}