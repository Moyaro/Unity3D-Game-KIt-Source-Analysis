using UnityEngine;

namespace Gamekit3D
{
    public class Dissolve : MonoBehaviour//溶解
    {
        public float minStartTime = 2f;//最短开始时间
        public float maxStartTime = 6f;//最长开始时间
        public float dissolveTime = 3f;//溶解时间
        public AnimationCurve curve;//动画曲线

        float m_Timer;
        float m_EmissionRate;
        Renderer[] m_Renderer;//渲染器
        MaterialPropertyBlock m_PropertyBlock;//材料特性块
        ParticleSystem m_ParticleSystem;//粒子系统
        ParticleSystem.EmissionModule m_Emission;//粒子系统的发射模块
        float m_StartTime;
        float m_EndTime;

        const string k_CutoffName = "_Cutoff";//只读数组

        void Awake()
        {

            m_PropertyBlock = new MaterialPropertyBlock();//定义一个新的材料特性快
            m_Renderer = GetComponentsInChildren<Renderer>();//渲染器获取自己的情况

            m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
            //粒子系统获取自己的情况

            m_Emission = m_ParticleSystem.emission;
            //将粒子系统的发射状态赋给粒子系统的发射模块

            m_EmissionRate = m_Emission.rateOverTime.constant;
            //发射率=粒子系统发射状态的随时间变化的速率的常数
            m_Emission.rateOverTimeMultiplier = 0;
            //粒子系统发射状态的随时间变化的比率乘数=0


            m_Timer = 0;//将计时器设置为0

            m_StartTime = Time.time + Random.Range(minStartTime, maxStartTime);
            //开始时间=系统时间+最长和最短开始时间内的一个随机值
            m_EndTime = m_StartTime + dissolveTime + m_ParticleSystem.main.startLifetime.constant;
            //结束时间=开始时间+溶解时间+粒子系统的主要开始使用寿命的常数
        }

        void Update()
        {
            if (Time.time >= m_StartTime)//如果系统运行时间大于等于开始时间
                
            {
                float cutoff = 0;//设置一个截止点为0

                for (int i = 0; i < m_Renderer.Length; i++)//当i小于渲染器的长度时
                {
                    m_Renderer[i].GetPropertyBlock(m_Prope);//渲染器第i长度获取材料特性快
                    cutoff = Mathf.Clamp01(m_Timer / dissolveTime);
                    //截止点为一个限制value在0,1之间并返回value的值
                    //如果value小于0，返回0。如果value大于1,返回1，否则返回value 
                    m_PropertyBlock.SetFloat(k_CutoffName, cutoff);
                    //设置材料特性快的浮点数值为cutoff
                    m_Renderer[i].SetPropertyBlock(m_PropertyBlock);
                    //渲染器第i长度获取的材料特性快为这个材料特性快
                }


                m_Emission.rateOverTimeMultiplier = curve.Evaluate(cutoff) * m_EmissionRate;
                //粒子系统发射模块的随时间变化的比率乘数=从动画曲线的cutoff值获得的乘数*发射率


                m_Timer += Time.deltaTime;//计时器时间每次加上增量时间
            }

            if (Time.time >= m_EndTime)//如果系统运行时间大于等于结束时间
            {
                Destroy(gameObject);//销毁这个物体
            }
        }
    }

}