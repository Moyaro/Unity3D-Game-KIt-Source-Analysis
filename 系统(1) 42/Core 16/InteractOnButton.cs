using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Gamekit3D
{
    public class InteractOnButton : InteractOnTrigger //按钮的互动
    {

        public string buttonName = "X";
        public UnityEvent OnButtonPress;//可以在里面添加各种事件

        bool canExecuteButtons = false;

        protected override void ExecuteOnEnter(Collider other)//在碰撞器内
        {
            canExecuteButtons = true;
        }

        protected override void ExecuteOnExit(Collider other)//在碰撞器外
        {
            canExecuteButtons = false;
        }

        void Update()
        {
            if (canExecuteButtons && Input.GetButtonDown(buttonName))
            {
                OnButtonPress.Invoke();//触发事件
            }
        }

    } 
}
