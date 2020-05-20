using System.Collections;//System.Collections 命名空间包含接口和类，这些接口和类定义各种对象（如列表、队列、位数组、哈希表和字典）的集合  
using System.Collections.Generic;//System.Collections.Generic 命名空间包含定义泛型集合的接口和类，用户可以使用泛型集合来创建强类型集合，这种集合能提供比非泛型强类型集合更好的类型安全性和性能
using System.Security.Cryptography;//System.Security.Cryptography 命名空间提供加密服务，包括安全的数据编码和解码，以及许多其他操作，例如散列法、随机数字生成和消息身份验证。https://docs.microsoft.com/zh-cn/dotnet/api/system.security.cryptography?view=dotnet-plat-ext-3.1    
using UnityEngine;
using UnityEngine.AI;//https://docs.unity3d.com/ScriptReference/UnityEngine.AIModule.html

//https://docs.unity3d.com/ScriptReference/index.html
//http://docs.manew.com/Script/index.htm
namespace Gamekit3D
{
//this assure it's runned before any behaviour that may use it, as the animator need to be fecthed
    //因为要完善动画控制器，所以将该脚本执行顺序的执行顺序设置为最后一个
    [DefaultExecutionOrder(-1)]//动态设置脚本执行顺序：https://blog.csdn.net/s15100007883/article/details/101207442
    [RequireComponent(typeof(NavMeshAgent))]//导航寻路组件，https://blog.csdn.net/Windgs_YF/article/details/87856128
    //https://www.jianshu.com/p/978477d86530; https://gameinstitute.qq.com/community/detail/119040
    public class EnemyController : MonoBehaviour
    {
        public bool interpolateTurning = false;//插入旋转
        public bool applyAnimationRotation = false;//应用动画旋转

        public Animator animator { get { return m_Animator; } }
        public Vector3 externalForce { get { return m_ExternalForce; } }//外部力下的速度
        public NavMeshAgent navmeshAgent { get { return m_NavMeshAgent; } }//导航组件属性
        public bool followNavmeshAgent { get { return m_FollowNavmeshAgent; } }//是否跟随导航组件
        public bool grounded { get { return m_Grounded; } }//是否在地面上

        //防止非法访问
        protected NavMeshAgent m_NavMeshAgent;
        protected bool m_FollowNavmeshAgent;
        protected Animator m_Animator;
        protected bool m_UnderExternalForce;//是否收到外部力
        protected bool m_ExternalForceAddGravity = true;//是否给外部力添加重力？
        protected Vector3 m_ExternalForce;
        protected bool m_Grounded;

        protected Rigidbody m_Rigidbody;

        const float k_GroundedRayDistance = .8f;//地面射线距离为float型的0.8,http://www.imooc.com/wenda/detail/402457

        void OnEnable()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;//AnimatorUpdateMode .AnimatePhysics更新在物理循环中，让动画系统与物理引擎同步

            //Turn off automatic path-finding location updates
            m_NavMeshAgent.updatePosition = false;

            //Add Rigidbody component to child objects
            m_Rigidbody = GetComponentInChildren<Rigidbody>();
            if (m_Rigidbody == null)
                m_Rigidbody = gameObject.AddComponent<Rigidbody>();

            //isKinematic属性是确定刚体是否接受动力学模拟，此影响不仅包括重力感应，还包括速度、阻力、质量等的物理模拟
            //Rigidbody.interpolation允许你以固定的帧率平滑物理运行效果
            //
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            //当该脚本执行时才打开自动寻路组件
            m_FollowNavmeshAgent = true;
        }

        private void FixedUpdate()
        {
            //有任务操控则调整动画速度为1
            animator.speed = PlayerInput.Instance != null && PlayerInput.Instance.HaveControl() ? 1.0f : 0.0f;

            //检测是否在地面上
            CheckGrounded();
            //如果收到外部力作用就随之移动
            if (m_UnderExternalForce)
                ForceMovement();
        }

        void CheckGrounded()
        {
            //初始化射线捕获的物体
            //初始化射线检测（origin，direction）（0，1，0）（0，-1，0）
            //将检测结果保存到m_Grounded
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            m_Grounded = Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers,
                QueryTriggerInteraction.Ignore);//射线，如果返回ture那么hit中将保存被碰撞物体的更多信息，射线检测最大距离，图层模板指定射线检测时忽略哪些碰撞体继续前进，指定此检测时候触发触发器

        }

        //外部作用力下的移动
        void ForceMovement()
        {
            if(m_ExternalForceAddGravity)
                m_ExternalForce += Physics.gravity * Time.deltaTime;//gt

            RaycastHit hit;
            Vector3 movement = m_ExternalForce * Time.deltaTime;//外部作用力下的位移
            
            if (!m_Rigidbody.SweepTest(movement.normalized, out hit, movement.sqrMagnitude))
            {
                //将运动中的刚体移向position
                m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
            }

            //将导航控制器移动到该位置
            m_NavMeshAgent.Warp(m_Rigidbody.position);
        }

        //动画控制器的移动
        private void OnAnimatorMove()
        {
            if (m_UnderExternalForce)
                return;

            if (m_FollowNavmeshAgent)
            {
                m_NavMeshAgent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;//返回该向量的长度
                transform.position = m_NavMeshAgent.nextPosition;
            }
            else
            {
                //检测刚体在移动时是否会与其他物体发生碰撞
                //Rigidbody.SweepTest（移动点，如果返回ture那么hit中将保存被碰撞物体的更多信息，返回此向量的长度）
                RaycastHit hit;
                if (!m_Rigidbody.SweepTest(m_Animator.deltaPosition.normalized, out hit,
                    m_Animator.deltaPosition.sqrMagnitude))
                {
                    m_Rigidbody.MovePosition(m_Rigidbody.position + m_Animator.deltaPosition);
                }
            }

            //如果当前需要调整动画控制机的方向
            if (applyAnimationRotation)
            {
                transform.forward = m_Animator.deltaRotation * transform.forward;
            }
        }

        // used to disable position being set by the navmesh agent, for case where we want the animation to move the enemy instead (e.g. Chomper attack)
        //用于禁用由导航网格代理设置的位置，以便我们希望使用动画来移动敌人（例如 Chomper 攻击）
        public void SetFollowNavmeshAgent(bool follow)
        {
            //不再跟随但导航打开
            if (!follow && m_NavMeshAgent.enabled)
            {
                //清楚当前的路径
                m_NavMeshAgent.ResetPath();
            }//跟随但导航关闭
            else if(follow && !m_NavMeshAgent.enabled)
            {
                //将导航控制器移动到该位置
                m_NavMeshAgent.Warp(transform.position);
            }

            //更改导航的状态
            m_FollowNavmeshAgent = follow;
            m_NavMeshAgent.enabled = follow;
        }

        //添加一个力
        public void AddForce(Vector3 force, bool useGravity = true)
        {
            //关闭导航
            if (m_NavMeshAgent.enabled)
                m_NavMeshAgent.ResetPath();

            m_ExternalForce = force;
            m_NavMeshAgent.enabled = false;
            m_UnderExternalForce = true;
            m_ExternalForceAddGravity = useGravity;
        }

        //力的作用效果结束时消除受力状态
        public void ClearForce()
        {
            m_UnderExternalForce = false;
            m_NavMeshAgent.enabled = true;
        }

        //向前
        public void SetForward(Vector3 forward)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            //当需要旋转插入
            if (interpolateTurning)
            {
                //Quaternion.RotateTowards（从，到，旋转度）
                targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    m_NavMeshAgent.angularSpeed * Time.deltaTime);
            }

            transform.rotation = targetRotation;
        }

        //设置导航控制器寻路的目标位置
        public bool SetTarget(Vector3 position)
        {
            return m_NavMeshAgent.SetDestination(position);
        }
    }
}