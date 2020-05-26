using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    //批处理
    public class BatchProcessor : MonoBehaviour
    {
        public delegate void BatchProcessing();//委托的使用

        static protected BatchProcessor s_Instance;
        static protected List<BatchProcessing> s_ProcessList;

        //这个脚本的四个方法都是静态方法,难道是因为这四个方法在调用时与该类的实例对象？无关使用时注意以下三点
        //静态方法只能访问类的静态成员，不能访问类的非静态成员；
        //非静态方法可以访问类的静态成员，也可以访问类的非静态成员；
        //静态方法既可以用实例来调用，也可以用类名来调用。
        static BatchProcessor()//静态方法
        {
            s_ProcessList = new List<BatchProcessing>();//
        }

        static public void RegisterBatchFunction(BatchProcessing function)
        {
            s_ProcessList.Add(function);
        }

        //注销批量操作？
        static public void UnregisterBatchFunction(BatchProcessing function)
        {
            s_ProcessList.Remove(function);
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < s_ProcessList.Count; ++i)
            {
                s_ProcessList[i]();//?
            }
        }

        //Before –> Awake –> OnEnable –> After –> RuntimeMethodLoad –> Start
        [RuntimeInitializeOnLoadMethod]//在游戏初始化之前做一些额外的初始化工作 https://www.cnblogs.com/meteoric_cry/p/7602122.html
        //Init会在Update之前调用
        static void Init()//与一般的static方法有什么区别？
        {
            if (s_Instance != null)
                return;

            GameObject obj = new GameObject("BatchProcessor");
            DontDestroyOnLoad(obj);
            s_Instance = obj.AddComponent<BatchProcessor>();
        }
    }

}