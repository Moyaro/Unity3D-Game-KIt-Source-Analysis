using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Gamekit3D
{
    public class CameraSettings : MonoBehaviour
    {
        public enum InputChoice//枚举
        {
            KeyboardAndMouse, Controller,//这俩哥们初始时候默认分别是0和1
        }

        //使用序列化：将对象的状态保存在存储媒体中以便可以在以后重新创建出完全相同的副本
        [Serializable]
        public struct InvertSettings//颠倒设置
        {
            public bool invertX;//颠倒后的二维坐标
            public bool invertY;
        }


        public Transform follow;//跟随的
        public Transform lookAt;//注视着的
        public CinemachineFreeLook keyboardAndMouseCamera;//CinemachineFreeLookEditor的文件路径是E:\UnityFile\Completion U3D GameKit\Library\PackageCache\com.unity.cinemachine@2.3.4\Editor\Editors，他是电影控制器开发工具包的一个类
        public CinemachineFreeLook controllerCamera;//该怎么用就怎么用，暂时没必要回溯去看CinemachineFreeLook的结构
        public InputChoice inputChoice;//枚举型的数据除了有方便阅读和确保变量合法的功能外，在Inspctor中还可以作为选项进行选择
        public InvertSettings keyboardAndMouseInvertSettings;//键盘和鼠标的颠倒设置
        public InvertSettings controllerInvertSettings;//控制者的颠倒设置
        public bool allowRuntimeCameraSettingsChanges;//是否允许在游戏运行时更改相机设置

        public CinemachineFreeLook Current//该属性能返回当前控制电影机的的是鼠标键盘还是相机控制器
        {
            get { return inputChoice == InputChoice.KeyboardAndMouse ? keyboardAndMouseCamera : controllerCamera; }
        }

        void Reset()
        {
            Transform keyboardAndMouseCameraTransform = transform.Find("KeyboardAndMouseFreeLookRig");//找到鼠标键盘相机控制器
            if (keyboardAndMouseCameraTransform != null)
                keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineFreeLook>();//获取鼠标键盘相机控制器上的自由视角电影机控制器

            Transform controllerCameraTransform = transform.Find("ControllerFreeLookRig");
            if (controllerCameraTransform != null)
                controllerCamera = controllerCameraTransform.GetComponent<CinemachineFreeLook>();

            PlayerController playerController = FindObjectOfType<PlayerController>();//寻找并保存被挂载的PlayerController
            if (playerController != null && playerController.name == "Ellen")
            {
                follow = playerController.transform;//跟随艾伦

                lookAt = follow.Find("HeadTarget");//注视着艾伦的头部

                if (playerController.cameraSettings == null)//如果艾伦身上的相机没进行设置就将本类的设置进行覆盖
                    playerController.cameraSettings = this;
            }
        }

        void Awake()
        {
            UpdateCameraSettings();
        }

        void Update()
        {
            //每帧如果允许在运行时更新相机设置就更新
            if (allowRuntimeCameraSettingsChanges)
            {
                UpdateCameraSettings();
            }
        }

        void UpdateCameraSettings()
        {
            //用新建的颠倒设置更新两种电影机控制器的跟随物体、注视物体、XY轴的颠倒输入、优先级
            keyboardAndMouseCamera.Follow = follow;
            keyboardAndMouseCamera.LookAt = lookAt;
            keyboardAndMouseCamera.m_XAxis.m_InvertInput = keyboardAndMouseInvertSettings.invertX;
            keyboardAndMouseCamera.m_YAxis.m_InvertInput = keyboardAndMouseInvertSettings.invertY;

            controllerCamera.m_XAxis.m_InvertInput = controllerInvertSettings.invertX;
            controllerCamera.m_YAxis.m_InvertInput = controllerInvertSettings.invertY;
            controllerCamera.Follow = follow;
            controllerCamera.LookAt = lookAt;

            keyboardAndMouseCamera.Priority = inputChoice == InputChoice.KeyboardAndMouse ? 1 : 0;
            controllerCamera.Priority = inputChoice == InputChoice.Controller ? 1 : 0;
        }
    } 
}
