using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit3D
{
    public class InventoryController : MonoBehaviour//库存控制器
    {
        [System.Serializable]//序列化，指将对象实例的状态存储到存储媒体
        public class InventoryEvent//库存事件
        {
            public string key;
            public UnityEvent OnAdd, OnRemove;
        }
        [System.Serializable]//序列化
        public class InventoryChecker//库存校对
        {

            public string[] inventoryItems;//库存项目
            public UnityEvent OnHasItem, OnDoesNotHaveItem;

            public bool CheckInventory(InventoryController inventory)//校对库存（带入库存控制器的一个“库存”）
            {

                if (inventory != null)//如果这个量不为空
                {
                    for (var i = 0; i < inventoryItems.Length; i++)//当i<库存项目的长度时
//var可以理解为匿名类型，它是一个声明变量的占位符。它主要用于在声明变量时，无法确定数据类型时使用
                    {
                        if (!inventory.HasItem(inventoryItems[i]))//如果库存项目中的i对应的与控制器中的不同
                        {
                            OnDoesNotHaveItem.Invoke();//执行ODNHI
                            return false;//返回false
                        }
                    }
                    OnHasItem.Invoke();//如果一样，执行OHI
                    return true;//返回true
                }
                return false;//如果这个量为空，返回false
            }
        }


        public InventoryEvent[] inventoryEvents;//定义库存事件的一个数组

        HashSet<string> inventoryItems = new HashSet<string>();
        //HashSet实现Set接口，由哈希表（实际上是一个HashMap实例）支持
        //创建一个新的库存项目
        //https://www.iteye.com/blog/aoyouzi-2295245
        public void AddItem(string key)//添加项目
        {
            if (!inventoryItems.Contains(key))//如果没有从库存项目中找到关键字
            {
                var ev = GetInventoryEvent(key);//将从库存事件中获取的关键字赋给ev
                if (ev != null) ev.OnAdd.Invoke();//如果关键字不为空，就调用添加这个关键字
                inventoryItems.Add(key);//在库存项目中添加上这个关键字
            }
        }

        public void RemoveItem(string key)//移除项目
        {
            if (inventoryItems.Contains(key))//如果从库存项目中找到关键字
            {
                var ev = GetInventoryEvent(key);//将从库存事件中获取的关键字赋给ev
                if (ev != null) ev.OnRemove.Invoke();//如果关键字不为空，就调用删除这个关键字
                inventoryItems.Remove(key);//在库存项目中删除这个关键字
            }
        }

        public bool HasItem(string key)//有项目
        {
            return inventoryItems.Contains(key);//返回库存项目中的关键字
        }

        public void Clear()//清除
        {
            inventoryItems.Clear();//清除这个数组中的数据
        }

        InventoryEvent GetInventoryEvent(string key)//获取库存项目
        {
            foreach (var iv in inventoryEvents)//依次调用数组中的每一个元素
            {
                if (iv.key == key) return iv;//如果iv的关键字与原定关键字相同，返回iv
            }
            return null;//如果都不相同，返回空
        }

    }

}