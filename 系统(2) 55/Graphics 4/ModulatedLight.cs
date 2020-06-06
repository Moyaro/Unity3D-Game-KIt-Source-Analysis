using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    [RequireComponent(typeof(Light))]
    //当你添加的一个用了RequireComponent组件的脚本，需要的组件将会自动被添加到game object（游戏物体）。
    //这个可以有效的避免组装错误
    public class ModulatedLight : MonoBehaviour//调光
    {

        public enum ModulationType//枚举类,调整类型
                                  //https://blog.csdn.net/hellojoy/article/details/79883671
        {
            Sine, Triangle, Perlin, Random
        }

        public bool executeInEditMode = true;//在编辑模式下执行
        public ModulationType type = ModulationType.Sine;//将枚举类中的Sine赋给type
        public float frequency = 1f;//出现频率为1
        public Color colorA = Color.red;//colorA是红色
        public Color colorB = Color.blue;//colorB是蓝色

        public new Light light;//定义灯光

        float TriangleWave(float x)//三角波
        {
            var frac = x - (int)x;
            var a = frac * 2.0f - 1.0f;
            return a > 0 ? a : -a;
        }

        void Reset()//重置
        {
            light = GetComponent<Light>();//获取灯光组件
            if (light != null) colorA = light.color;//如果灯光不为空，灯光给A
        }

        void Update()
        {
            if (light == null) light = GetComponent<Light>();//如果没有灯光，获取灯光组件
            var t = 0f;
            switch (type)//判断灯光类型
            {
                case ModulationType.Sine://如果为Sine型
                    t = Mathf.Sin(Time.time * frequency);//帧率*频率的弧度值
                    break;
                case ModulationType.Triangle://如果是Triangle型
                    t = TriangleWave(Time.time * frequency);//帧率*频率的三角波
                    break;
                case ModulationType.Perlin://如果为Perlin型
                    t = Mathf.PerlinNoise(Time.time * frequency, 0.5f);//返回一个RerlinNoise值
                    //https://blog.csdn.net/sinat_37102348/article/details/82967894
                    break;
                case ModulationType.Random://如果为Random值
                    t = Random.value;//返回一个随机数，在0.0（包括）～1.0（包括）之间
                    break;
            }
            light.color = Color.Lerp(colorA, colorB, t);
            //返回颜色a和颜色b之间的线性差值t，t是夹在0到1之间。当t为0时返回a。当t为1时返回b
        }

    }

}
