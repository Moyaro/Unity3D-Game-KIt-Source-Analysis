using Gamekit3D.Message;//引用了本工程内置的类MessageSystem，（没必要像我这里后面说的这么详细）里面枚举了人物状态信息的三种类型受攻击、死亡、重生，定义了接受信息函数的接口IMessageReceiver
using UnityEngine;//匹配unity引擎必备的命名空间，像这种大路边的命名空间就不必说明了

namespace Gamekit3D
{
    [DefaultExecutionOrder(100)]
    public class ChomperBehavior : MonoBehaviour, IMessageReceiver
    {
        public static readonly int hashInPursuit = Animator.StringToHash("InPursuit"); //readonly只能在初始化的时候被赋值
        public static readonly int hashAttack = Animator.StringToHash("Attack");//该字符串转换到ID//从字符串生成一个参数ID。//ID是用于参数的存取器优化（setters 和 getters）。
        public static readonly int hashHit = Animator.StringToHash("Hit");
        public static readonly int hashVerticalDot = Animator.StringToHash("VerticalHitDot");
        public static readonly int hashHorizontalDot = Animator.StringToHash("HorizontalHitDot");
        public static readonly int hashThrown = Animator.StringToHash("Thrown");
        public static readonly int hashGrounded = Animator.StringToHash("Grounded");
        public static readonly int hashVerticalVelocity = Animator.StringToHash("VerticalVelocity");
        public static readonly int hashSpotted = Animator.StringToHash("Spotted");
        public static readonly int hashNearBase = Animator.StringToHash("NearBase");

        public static readonly int hashIdleState = Animator.StringToHash("ChomperIdle");

        public EnemyController controller { get { return m_Controller; } }

        public PlayerController target { get { return m_Target; } }
        public TargetDistributor.TargetFollower followerData { get { return m_FollowerInstance; } }

        public Vector3 originalPosition { get; protected set; }//原始的位置
        [System.NonSerialized]
        public float attackDistance = 3;//攻击距离

        public MeleeWeapon meleeWeapon;//武器
        public TargetScanner playerScanner;//Scanner扫描器
        [Tooltip("Time in seconde before the Chomper stop pursuing the player when the player is out of sight")]
        public float timeToStopPursuit;//距离停止追赶还有多少时间

        [Header("Audio")]
        public RandomAudioPlayer attackAudio;
        public RandomAudioPlayer frontStepAudio;
        public RandomAudioPlayer backStepAudio;
        public RandomAudioPlayer hitAudio;
        public RandomAudioPlayer gruntAudio;
        public RandomAudioPlayer deathAudio;
        public RandomAudioPlayer spottedAudio;

        protected float m_TimerSinceLostTarget = 0.0f;

        protected PlayerController m_Target = null;
        protected EnemyController m_Controller;//赋到怪兽身上的 控制器
        protected TargetDistributor.TargetFollower m_FollowerInstance = null;

        protected void OnEnable()
        {
            m_Controller = GetComponentInChildren<EnemyController>();

            originalPosition = transform.position;

            meleeWeapon.SetOwner(gameObject);

            m_Controller.animator.Play(hashIdleState, 0, Random.value);//Play（将要播放动画的名字（动画状态的哈希值），动画状态所在的层，将要播放动画状态的归一化时间）

            SceneLinkedSMB<ChomperBehavior>.Initialise(m_Controller.animator, this);//给怪兽动画赋上咀嚼表现
        }

        /// <summary>
        /// Called by animation events.//由动画事件调用..
        /// </summary>
        /// <param name="frontFoot">Has a value of 1 when it's a front foot stepping and 0 when it's a back foot.</param>//它的值是1，当它是前脚的时候，0当它是后脚的时候
        void PlayStep(int frontFoot)
        {
            if (frontStepAudio != null && frontFoot == 1)
                frontStepAudio.PlayRandomClip();
            else if (backStepAudio != null && frontFoot == 0)
                backStepAudio.PlayRandomClip();
        }

        /// <summary>
        /// Called by animation events.
        /// </summary>
        public void Grunt()//怪物发生咕噜声
        {
            if (gruntAudio != null)
                gruntAudio.PlayRandomClip();
        }

        public void Spotted()
        {
            if (spottedAudio != null)
                spottedAudio.PlayRandomClip();
        }

        protected void OnDisable()//目标跟随
        {
            if (m_FollowerInstance != null)
                m_FollowerInstance.distributor.UnregisterFollower(m_FollowerInstance);
        }

        private void FixedUpdate()
        {
            m_Controller.animator.SetBool(hashGrounded, controller.grounded);//（该参数的名称，该参数的新值）
            //setbool设置布尔值
            Vector3 toBase = originalPosition - transform.position;//原始的位置减后来的位置
            toBase.y = 0;

            m_Controller.animator.SetBool(hashNearBase, toBase.sqrMagnitude < 0.1 * 0.1f);//Vector3.sqrMagnitude 长度平方（返回这个向量的长度的平方）
            //hashNearBase基地附近
        }

        public void FindTarget()
        {
            //we ignore height difference if the target was already seen如果目标已经被看到，我们就忽略了高差
            PlayerController target = playerScanner.Detect(transform, m_Target == null);//对玩家进行扫描

            if (m_Target == null)
            {
                //we just saw the player for the first time, pick an empty spot to target around them我们只是第一次看到玩家，选择一个空位置瞄准他们周围
                if (target != null)
                {
                    m_Controller.animator.SetTrigger(hashSpotted);//Animator.SetTrigger 设置触发器（设置一个要激活的触发器参数。）

                    m_Target = target;
                    TargetDistributor distributor = target.GetComponentInChildren<TargetDistributor>();//从这个物体的第一个子物体获取组件
                    if (distributor != null)
                        m_FollowerInstance = distributor.RegisterNewFollower();//目标跟随
                }
            }
            else
            {
                //we lost the target. But chomper have a special behaviour : they only loose the player scent if they move past their detection range
                //我们失去了目标。 但是chomper有一种特殊的行为：他们只有在越过探测范围时才会释放玩家的气味
                //and they didn't see the player for a given time. Not if they move out of their detectionAngle. So we check that this is the case before removing the target
                //他们在给定的时间内没有看到球员。 如果他们离开他们的探测角度就不会。 所以我们在移除目标之前检查一下情况
                if (target == null)
                {
                    m_TimerSinceLostTarget += Time.deltaTime;

                    if (m_TimerSinceLostTarget >= timeToStopPursuit)
                    {
                        Vector3 toTarget = m_Target.transform.position - transform.position;

                        if (toTarget.sqrMagnitude > playerScanner.detectionRadius * playerScanner.detectionRadius)
                        {
                            if (m_FollowerInstance != null)
                                m_FollowerInstance.distributor.UnregisterFollower(m_FollowerInstance);

                            //the target move out of range, reset the target目标移出范围，重置目标
                            m_Target = null;
                        }
                    }
                }
                else
                {
                    if (target != m_Target)
                    {
                        if (m_FollowerInstance != null)
                            m_FollowerInstance.distributor.UnregisterFollower(m_FollowerInstance);

                        m_Target = target;

                        TargetDistributor distributor = target.GetComponentInChildren<TargetDistributor>();
                        if (distributor != null)
                            m_FollowerInstance = distributor.RegisterNewFollower();
                    }

                    m_TimerSinceLostTarget = 0.0f;
                }
            }
        }

        public void StartPursuit()
        {
            if (m_FollowerInstance != null)
            {
                m_FollowerInstance.requireSlot = true;
                RequestTargetPosition();
            }

            m_Controller.animator.SetBool(hashInPursuit, true);//追捕
        }

        public void StopPursuit()
        {
            if (m_FollowerInstance != null)
            {
                m_FollowerInstance.requireSlot = false;
            }

            m_Controller.animator.SetBool(hashInPursuit, false);
        }

        public void RequestTargetPosition()
        {
            Vector3 fromTarget = transform.position - m_Target.transform.position;
            fromTarget.y = 0;

            m_FollowerInstance.requiredPoint = m_Target.transform.position + fromTarget.normalized * attackDistance * 0.9f;//追踪目标需要的距离，
        }

        public void WalkBackToBase()
        {
            if (m_FollowerInstance != null)
                m_FollowerInstance.distributor.UnregisterFollower(m_FollowerInstance);
            m_Target = null;
            StopPursuit();
            m_Controller.SetTarget(originalPosition);//将追踪过程中最后发现目标的地方设置为初始点
            m_Controller.SetFollowNavmeshAgent(true);
        }

        public void TriggerAttack()//触发攻击
        {
            m_Controller.animator.SetTrigger(hashAttack);
        }

        public void AttackBegin()//下一次攻击
        {
            meleeWeapon.BeginAttack(false);
        }

        public void AttackEnd()//攻击结束
        {
            meleeWeapon.EndAttack();
        }

        public void OnReceiveMessage(Message.MessageType type, object sender, object msg)//？？？？
        {
            switch (type)
            {
                case Message.MessageType.DEAD:
                    Death((Damageable.DamageMessage)msg);
                    break;
                case Message.MessageType.DAMAGED:
                    ApplyDamage((Damageable.DamageMessage)msg);
                    break;
                default:
                    break;
            }
        }

        public void Death(Damageable.DamageMessage msg)
        {
            Vector3 pushForce = transform.position - msg.damageSource;

            pushForce.y = 0;

            transform.forward = -pushForce.normalized;
            controller.AddForce(pushForce.normalized * 7.0f - Physics.gravity * 0.6f);

            controller.animator.SetTrigger(hashHit);
            controller.animator.SetTrigger(hashThrown);

            //We unparent the hit source, as it would destroy it with the gameobject when it get replaced by the ragdol otherwise
            deathAudio.transform.SetParent(null, true);
            deathAudio.PlayRandomClip();
            GameObject.Destroy(deathAudio, deathAudio.clip == null ? 0.0f : deathAudio.clip.length + 0.5f);
        }

        public void ApplyDamage(Damageable.DamageMessage msg)
        {
            //TODO : make that more generic, (e.g. move it to the MeleeWeapon code with a boolean to enable shaking of camera on hit?)
            if (msg.damager.name == "Staff")
                CameraShake.Shake(0.06f, 0.1f);

            float verticalDot = Vector3.Dot(Vector3.up, msg.direction);
            float horizontalDot = Vector3.Dot(transform.right, msg.direction);

            Vector3 pushForce = transform.position - msg.damageSource;

            pushForce.y = 0;

            transform.forward = -pushForce.normalized;
            controller.AddForce(pushForce.normalized * 5.5f, false);

            controller.animator.SetFloat(hashVerticalDot, verticalDot);
            controller.animator.SetFloat(hashHorizontalDot, horizontalDot);

            controller.animator.SetTrigger(hashHit);

            hitAudio.PlayRandomClip();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            playerScanner.EditorGizmo(transform);
        }
#endif
    }
}