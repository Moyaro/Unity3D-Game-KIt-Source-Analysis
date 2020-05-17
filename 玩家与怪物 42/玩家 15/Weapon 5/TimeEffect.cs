using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class TimeEffect : MonoBehaviour//当鼠标点击以后播放挥砍动画，并显示刀光
    {
        public Light staffLight;
        
        Animation m_Animation;

        void Awake()
        {
            m_Animation = GetComponent<Animation>();

            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            staffLight.enabled = true;

            if (m_Animation)
                m_Animation.Play();

            StartCoroutine(DisableAtEndOfAnimation());
        }

        IEnumerator DisableAtEndOfAnimation()
        {
            yield return new WaitForSeconds(m_Animation.clip.length);//等待一个动画的时间长度

            gameObject.SetActive(false);
            staffLight.enabled = false;
        }
    } 
}
