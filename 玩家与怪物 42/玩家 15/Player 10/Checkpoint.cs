using System.Collections;//命名空间包含接口和类，这些接口和类定义各种对象（如列表、队列、位数组、哈希表和字典）的集合。
using System.Collections.Generic;//.NET框架中的基础类库，用于实属现一些基本的类 NET的介绍 https://blog.csdn.net/weixin_38887666/article/details/80015446
using UnityEngine;

namespace Gamekit3D
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        private void Awake()
        {
            //我们确保检查点是检查点层的一部分，检查点层被设置为只与玩家层交互。 
            gameObject.layer = LayerMask.NameToLayer("Checkpoint");//LayerMask：图层面具
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController controller = other.GetComponent<PlayerController>();

            if (controller == null)
                return;

            controller.SetCheckpoint(this);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue * 0.75f;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawRay(transform.position, transform.forward * 2);
        }
    }
}
