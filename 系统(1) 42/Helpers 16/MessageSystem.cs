using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    namespace Message
    {
        //定义玩家是出于受伤、死亡还是重生的状态
        public enum MessageType
        {
            DAMAGED,
            DEAD,
            RESPAWN,
            //Add your user defined message type after
        }

        //定义信息接收器的接口
        public interface IMessageReceiver
        {
            void OnReceiveMessage(MessageType type, object sender, object msg);
        }
    } 
}
