using UnityEditor;
using UnityEngine;

namespace Gamekit3D
{
    [ExecuteInEditMode]
    //在EditMode下也可以执行脚本,加上此属性后，不运行程序，也能执行脚本
    public class GlobalShaderSettings : MonoBehaviour//全局着色器设置
    {

        [SerializeField]
        //凡是显示在Inspector 中的属性都同时具有Serialize功能
        //序列化的意思是说再次读取Unity时序列化的变量是有值的，不需要你再次去赋值，因为它已经被保存下来
        float TopScale = 1;//上标度
        [SerializeField]
        float NormalDetailScale = 1;//标准细节比例
        [SerializeField]
        float NoiseAmount = 1;//噪声量
        [SerializeField]
        float NoiseFalloff = 1;//噪声衰减
        [SerializeField]
        float NoiseScale = 1;//噪声等级
        [SerializeField]
        float FresnelAmount = 0.5f;//菲涅尔反射
        [SerializeField]
        float FresnelPower = 0.5f;//菲涅耳功率

        void Update()
        {
            //为所有着色器设置一个全局浮点数。
            Shader.SetGlobalFloat("_TopScale", TopScale);
            Shader.SetGlobalFloat("_TopNormal2Scale", NormalDetailScale);
            Shader.SetGlobalFloat("_NoiseAmount", NoiseAmount);
            Shader.SetGlobalFloat("_NoiseFallOff", NoiseFalloff);
            Shader.SetGlobalFloat("_noiseScale", NoiseScale);
            Shader.SetGlobalFloat("_FresnelAmount", FresnelAmount);
            Shader.SetGlobalFloat("_FresnelPower", FresnelPower);
        }
    } 
}
