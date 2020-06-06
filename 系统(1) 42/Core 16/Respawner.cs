using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D
{
    public class Respawner : MonoBehaviour//怪物刷新
    {
        [System.Serializable]//序列化
        public class SaveState//存档
        {
            public Vector3 position;//定义向量位置
            public Quaternion rotation;//定义四元数旋转角度
        }

        public GameObject player;
        public float savePeriod = 5;//将保存周期设置为5

        public List<SaveState> savedStates = new List<SaveState>();//转化成List形式

        float lastCheck = 0f;//最后读取
        bool paused = false;//暂停

        void Start()
        {
            lastCheck = Time.time - savePeriod;//最后读取进度为从游戏开始减去保存周期
        }

        public void Pause()//暂停
        {
            paused = true;
        }

        public void Resume()//继续
        {
            paused = false;
        }

        public void RestoreLast()//最后还原
        {
            if (savedStates.Count > 0)//List的大小
            {
                var ss = savedStates[savedStates.Count - 1];//定义var型变量
                //https://mbd.baidu.com/ma/landingpage?t=smartapp_share&scenario=share&appid=fjESu3W8LB8fsE3tG3xUoMXSvvDjawbn&url=%2Fpages%2Fnote%2Findex%3Fslug%3D628e855842df%26origin%3Dshare
                savedStates.RemoveAt(savedStates.Count - 1);//移除索引元素
                player.transform.position = ss.position;//物体坐标更改
                player.transform.rotation = ss.rotation;//物体变换的旋转角度更改
            }
        }

        void Update()
        {
            if (!paused && Time.time - lastCheck > savePeriod)//如果不是暂停中，游戏运行时间减去最后一次保存时间大于保存周期
            {
                lastCheck = Time.time;//最后一次保存时间为游戏运行时间
                savedStates.Add(new SaveState() //添加新的存档点
                { 
                    position = player.transform.position, rotation = player.transform.rotation //位置和旋转角度进行更新
                });
                savedStates.RemoveRange(0, Mathf.Max(0, savedStates.Count - 8)/*取两个值之间的最大值*/);//批量删除指定长度的内容
                //https://www.cnblogs.com/zsznh/p/10599734.html
            }
        }
    }

}