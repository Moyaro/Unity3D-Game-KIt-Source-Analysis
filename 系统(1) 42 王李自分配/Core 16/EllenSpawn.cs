using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D
{
    public class EllenSpawn : MonoBehaviour//重生
    {
        [HideInInspector]
        public float effectTime;
        public Material[] EllenRespawnMaterials;
        public GameObject respawnParticles;//重生时的粒子
        Material[] EllenMaterials;

        MaterialPropertyBlock m_PropertyBlock;//不想实例化材质的颜色而修改它的办法：利用MaterialPropertyBlock
        //https://blog.csdn.net/liweizhao/article/details/81937590
        Renderer m_Renderer;//渲染控制器
        Vector4 pos;//
        Vector3 renderBounds;

        const string k_BoundsName = "_bounds";
        const string k_CutoffName = "_Cutoff";//shader里面控制透明度的内置变量？
        float m_Timer;
        float m_EndTime;

        bool m_Started = false;

        void Awake()
        {
            respawnParticles.SetActive(false);
            m_PropertyBlock = new MaterialPropertyBlock();
            m_Renderer = GetComponentInChildren<Renderer>();
            EllenMaterials = m_Renderer.materials;

            renderBounds = m_Renderer.bounds.size;//真实反应出有MeshRenderer这个组件的模型的尺寸
            pos.y = renderBounds.y;

            m_Renderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetVector(k_BoundsName, pos);//?
            m_PropertyBlock.SetFloat(k_CutoffName, 0.0001f);//?
            m_Renderer.SetPropertyBlock(m_PropertyBlock);

            pos = new Vector4(0, 0, 0, 0);

            m_Started = false;

            this.enabled = false;
        }

        void OnEnable()
        {
            m_Started = false;
            m_Renderer.materials = EllenRespawnMaterials;
            Set(0.001f);
            m_Renderer.enabled = false;
        }

        public void StartEffect()
        {
            m_Renderer.enabled = true;

            respawnParticles.SetActive(true);
            m_Started = true;
            m_Timer = 0.0f;
        }

        void Update()
        {
            if (!m_Started)
                return;

            float cutoff = Mathf.Clamp(m_Timer / effectTime, 0.01f, 1.0f);
            Set(cutoff);

            m_Timer += Time.deltaTime;

            if (cutoff >= 1.0f)
            {
                m_Renderer.materials = EllenMaterials;
                this.enabled = false;
            }
        }

        void Set(float cutoff)
        {
            renderBounds = m_Renderer.bounds.size;
            pos.y = renderBounds.y;
            m_Renderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetVector(k_BoundsName, pos);

            m_PropertyBlock.SetFloat(k_CutoffName, cutoff);
            m_Renderer.SetPropertyBlock(m_PropertyBlock);
        }

    }

}