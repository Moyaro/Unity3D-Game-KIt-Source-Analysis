using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D
{
    public class GrenadierSMBPunch : SceneLinkedSMB<GrenadierBehaviour>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //当需要播放近攻时的音乐时就播放
            if (m_MonoBehaviour.punchAudioPlayer)
                m_MonoBehaviour.punchAudioPlayer.PlayRandomClip();
        }
    }
}