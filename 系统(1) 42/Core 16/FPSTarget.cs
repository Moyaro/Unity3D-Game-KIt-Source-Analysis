using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class FPSTarget : MonoBehaviour//目标帧率（每秒最多调用Update函数的次数）
    {
        public int targetFPS = 60;//帧率为60

        // Use this for initialization//用于初始化
        void OnEnable()//游戏开始运行之前被调用
        {
            SetTargetFPS(targetFPS);//获得帧率
        }

        public void SetTargetFPS(int fps)
        {
            Application.targetFrameRate = fps;//将物体的帧率设置为60
        }
    } 
}
