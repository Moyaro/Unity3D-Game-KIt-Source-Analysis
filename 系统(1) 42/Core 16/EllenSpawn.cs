using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D
{
    public class EllenSpawn : MonoBehaviour//重生
    {
        [HideInInspector]//在inspector内隐藏该effecttime变量
        public float effectTime;//重生时间
        public Material[] EllenRespawnMaterials;//重生的各种光效材质
        public GameObject respawnParticles;//重生时的粒子
        Material[] EllenMaterials;//ellen的材质

        MaterialPropertyBlock m_PropertyBlock;//不想实例化材质的颜色而修改它的办法：利用MaterialPropertyBlock
        //https://blog.csdn.net/liweizhao/article/details/81937590
        Renderer m_Renderer;//渲染控制器
        Vector4 pos;//范围
        Vector3 renderBounds;//渲染范围

        const string k_BoundsName = "_bounds";//边界
        const string k_CutoffName = "_Cutoff";//  截断效果，也就是渐变
        float m_Timer;//报时器
        float m_EndTime;//结束时间

        bool m_Started = false;//是否开始

        void Awake()
        {
            respawnParticles.SetActive(false);//将重生的粒子特效开启
            m_PropertyBlock = new MaterialPropertyBlock();//属性块
            m_Renderer = GetComponentInChildren<Renderer>();//得到渲染控制器
            EllenMaterials = m_Renderer.materials;//将渲染器材质设为ellen的材质

            renderBounds = m_Renderer.bounds.size;//真实反应出有MeshRenderer这个组件的模型的尺寸
            pos.y = renderBounds.y;//范围的高度设为模型高度

            m_Renderer.GetPropertyBlock(m_PropertyBlock);//获取渲染器的每个材质的属性块，检索到的属性存储在通过“属性”传入的属性块中。如果未设置属性，则清除属性块。无论哪种情况，您传入的属性块都将被完全覆盖。
            m_PropertyBlock.SetVector(k_BoundsName, pos);//设置材质球的着色器_bounds属性的初始值为pos
            m_PropertyBlock.SetFloat(k_CutoffName, 0.0001f);//设置着色器_Cutoff属性的初始值为0.0001f
            m_Renderer.SetPropertyBlock(m_PropertyBlock);//设置材质的属性块

            pos = new Vector4(0, 0, 0, 0);//给位置设置初始值

            m_Started = false;//处于未开始的状态

            this.enabled = false;//该脚本被禁用【只是暂停了此脚本的Update和FixedUpdate等函数，而脚本中开启的协程（StartCoroutine）和用Invoke唤起的函数依然在执行不受影响】
        }

        void OnEnable()//使用此脚本时
        {
            m_Started = false;//未开始
            m_Renderer.materials = EllenRespawnMaterials;//将ellen材质设为渲染器材质
            Set(0.001f);//设置着色器_Cutoff属性的值为0.001f
            m_Renderer.enabled = false;//将渲染器关闭
        }

        public void StartEffect()//开始重生
        {
            m_Renderer.enabled = true;//渲染器开启

            respawnParticles.SetActive(true);//粒子特效打开
            m_Started = true;//设为开启状态
            m_Timer = 0.0f;//当前时间为0
        }

        void Update()
        {
            if (!m_Started)//如果是开启状态，就直接返回
                return;

            float cutoff = Mathf.Clamp(m_Timer / effectTime, 0.01f, 1.0f);//截断效果为当前时间与重生时间的比值，范围被限制在0.01f到1.0f
            Set(cutoff);//设置截断效果

            m_Timer += Time.deltaTime;//时间增加

            if (cutoff >= 1.0f)//当时间大于重生时间
            {
                m_Renderer.materials = EllenMaterials;//渲染器的材质设为ellen的材质
                this.enabled = false;//脚本关闭
            }
        }

        void Set(float cutoff)//设置截断效果
        {
            renderBounds = m_Renderer.bounds.size;//根据模型设置范围
            pos.y = renderBounds.y;//范围的高度为模型高度
            m_Renderer.GetPropertyBlock(m_PropertyBlock);//获取材质属性块
            m_PropertyBlock.SetVector(k_BoundsName, pos);//设置材质球的着色器_bounds属性的值为pos

            m_PropertyBlock.SetFloat(k_CutoffName, cutoff);//设置着色器_Cutoff属性的值为cutoff
            m_Renderer.SetPropertyBlock(m_PropertyBlock);//在脚本内设置材质属性块
        }

    }

}