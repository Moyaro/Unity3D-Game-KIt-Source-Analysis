using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    [RequireComponent(typeof(Collider))]//添加组件碰撞器
    public class InventoryItem : MonoBehaviour, IDataPersister//库存项目
    {
        public string inventoryKey = "";//设置库存关键词
        public LayerMask layers;//定义图层蒙版
        public bool disableOnEnter = false;//关闭启动时禁用

        [HideInInspector]//Unity在解析时会将inspector中的对应的调整框隐藏
        new public Collider collider;//控制器

        public AudioClip clip;//音频剪辑
        public DataSettings dataSettings;//数据设置

        void OnEnable()//启用时
        {
            collider = GetComponent<Collider>();//访问游戏对象中的控制器赋给collider
            PersistentDataManager.RegisterPersister(this);//缓存此时的游戏数据
        }

        void OnDisable()//禁用时
        {
            PersistentDataManager.UnregisterPersister(this);
        //缓存此时的游戏数据
        //https://www.dazhuanlan.com/2019/11/22/5dd6c9509fefd/
        }

        void Reset()//重置
        {
            layers = LayerMask.NameToLayer("Everything");//将名为everything的图层蒙版赋给layers
            collider = GetComponent<Collider>();//访问游戏对象中的控制器赋给collider
            collider.isTrigger = true;//碰撞器的触发器开启
            dataSettings = new DataSettings();//更新数据设置
        }

        void OnTriggerEnter(Collider other)//触发器被触发时执行
        {
            if (layers.Contains(other.gameObject))//访问图层蒙版中的物体，如果访问到
            {
                var ic = other.GetComponent<InventoryController>();
                //访问游戏对象other的库存控制器赋给ic
                ic.AddItem(inventoryKey);//在ic中添加库存关键字项目
                if (disableOnEnter)//如果设置为启动时禁用
                {
                    gameObject.SetActive(false);//禁用这个物体
                    Save();//保存当前状态
                }
                
                if (clip)//如果要音频剪辑时
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                //在此处播放一个clip的3D音效

            }
        }

        public void Save()//保存
        {
            PersistentDataManager.SetDirty(this);
            //在需要的时候调用接口方法来缓存和载入数据，主动缓存数据
        }

        void OnDrawGizmos()//绘制小控件
        {
            Gizmos.DrawIcon(transform.position, "InventoryItem", false);
            //游戏场景内在此处放置名为“InventoryItem”的图标
        }

        public DataSettings GetDataSettings()//获取数据设置
        {
            return dataSettings;//返回更新数据设置
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
            //设置数据设置
        {
            dataSettings.dataTag = dataTag;//给数据设置一个标签
            dataSettings.persistenceType = persistenceType;//给数据设置的缓存类型设置标签
        }

        public Data SaveData()//缓存数据
        {
            return new Data<bool>(gameObject.activeSelf);
            //返回一个物体自身运动的bool值
        }

        public void LoadData(Data data)//读取数据
        {
            Data<bool> inventoryItemData = (Data<bool>)data;
            gameObject.SetActive(inventoryItemData.value);//物体获取库存项目数据的value值
        }
    }
}
