using UnityEngine;//匹配unity引擎必备的命名空间，像这种大路边的命名空间就不必说明了
using Gamekit3D.Message;//引用了本工程内置的类MessageSystem，（没必要像我这里后面说的这么详细）里面枚举了人物状态信息的三种类型受攻击、死亡、重生，定义了接受信息函数的接口IMessageReceiver
using System.Collections;//命名空间包含接口和类，这些接口和类定义各种对象（如列表、队列、位数组、哈希表和字典）的集合。https://docs.microsoft.com/zh-cn/dotnet/api/system.collections?view=netcore-3.1
using UnityEngine.XR.WSA;//XR模块包含VR和AR相关平台支持功能。https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UnityEngine.XRModule.html

//本脚本主要实现了人物的控制，
namespace Gamekit3D
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    //// 需要同物体上有Animator组件，但是不需要给他赋值Animator Controller
    ///
    public class PlayerController : MonoBehaviour, IMessageReceiver
    {
        protected static PlayerController s_Instance;
        public static PlayerController instance { get { return s_Instance; } }//可以在其他脚本中通过声明instance变量的这个类的名称来访问Instance对象，而不需要实例化该类。
        //访问s_Instance即可访问脚本本身？
        public bool respawning { get { return m_Respawning; } }//变量属性的get方法，
        //不允许外界直接对类的私有成员变量直接访问的，既然不能访问，那定义这些成员变量还有什么意义呢？所以C#中就要用set和get方法来访问私有成员变量
        //当我们使用属性来访问私有成员变量时就会调用里面的get方法，当我们要修改该变量时就会调用set方法，
        //当然在定义的时候可以只定义一个get方法或只定义一个set方法。
        //如果只定义get方法，那么这个相应变量就是“只读”的；如果只定义set方法，那么相应变量就是“只写”的。
        //set就像一位门卫大叔一样，只有好人才能进来。可以通过属性来控制对员变量的读写，防止对成员变量的非法赋值
        //隐藏内部实现细节，保证变量的安全，同时也可以提升数据的安全性
        public float maxForwardSpeed = 8f;        // How fast Ellen can run.
        public float gravity = 20f;               // How fast Ellen accelerates downwards when airborne.
        public float jumpSpeed = 10f;             // How fast Ellen takes off when jumping.
        public float minTurnSpeed = 400f;         // How fast Ellen turns when moving at maximum speed.
        public float maxTurnSpeed = 1200f;        // How fast Ellen turns when stationary.最大转向旋转速度
        public float idleTimeout = 5f;            // How long before Ellen starts considering random idles.等待一段时间内后播放空闲动画
        public bool canAttack;                    // Whether or not Ellen can swing her staff.
        
        public CameraSettings cameraSettings;            // Reference used to determine the camera's direction.相机的参考方向
        public MeleeWeapon meleeWeapon;                  // Reference used to (de)activate the staff when attacking. 近战攻击脚本
        public RandomAudioPlayer footstepPlayer;         // Random Audio Players used for various situations.用于各种情况的随机音频播放器。
        public RandomAudioPlayer hurtAudioPlayer;
        public RandomAudioPlayer landingPlayer;
        public RandomAudioPlayer emoteLandingPlayer;
        public RandomAudioPlayer emoteDeathPlayer;
        public RandomAudioPlayer emoteAttackPlayer;
        public RandomAudioPlayer emoteJumpPlayer;

        protected AnimatorStateInfo m_CurrentStateInfo;    // Information about the base layer of the animator cached.有关缓存的动画制作器基本层的信息。
        protected AnimatorStateInfo m_NextStateInfo;
        protected bool m_IsAnimatorTransitioning;
        protected AnimatorStateInfo m_PreviousCurrentStateInfo;    // Information about the base layer of the animator from last frame.最后一帧中有关动画制作器基础层的信息。
        protected AnimatorStateInfo m_PreviousNextStateInfo;
        protected bool m_PreviousIsAnimatorTransitioning;
        protected bool m_IsGrounded = true;            // Whether or not Ellen is currently standing on the ground.
        protected bool m_PreviouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.//上一帧是否站在地上
        protected bool m_ReadyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.当前输入状态是否允许跳跃
        protected float m_DesiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.艾伦行走的目标速度
        protected float m_ForwardSpeed;                // How fast Ellen is currently going along the ground.当前沿着地面走的速度
        protected float m_VerticalSpeed;               // How fast Ellen is currently moving up or down.当前上下速度
        protected PlayerInput m_Input;                 // Reference used to determine how Ellen should move.怎样移动
        protected CharacterController m_CharCtrl;      // Reference used to actually move Ellen.实际移动的控制器
        protected Animator m_Animator;                 // Reference used to make decisions based on Ellen's current animation and to set parameters.根据当前的动画状态进行决策并设置参数
        protected Material m_CurrentWalkingSurface;    // Reference used to make decisions about audio.是否播放音效的参考
        protected Quaternion m_TargetRotation;         // What rotation Ellen is aiming to have based on input.目标旋转状态
        protected float m_AngleDiff;                   // Angle in degrees between Ellen's current rotation and her target rotation.当前旋转度和目标旋转度之间的差值
        protected Collider[] m_OverlapResult = new Collider[8];    // Used to cache colliders that are near Ellen.艾伦附近的8个噢鞥撞期
        protected bool m_InAttack;                     // Whether Ellen is currently in the middle of a melee attack.是否处于近战攻击中
        protected bool m_InCombo;                      // Whether Ellen is currently in the middle of her melee combo.是否在混战中？
        protected Damageable m_Damageable;             // Reference used to set invulnerablity and health based on respawning.
        protected Renderer[] m_Renderers;              // References used to make sure Renderers are reset properly. 渲染器的引用
        protected Checkpoint m_CurrentCheckpoint;      // Reference used to reset Ellen to the correct position on respawn.重生时将艾伦放到正确的位置
        protected bool m_Respawning;                   // Whether Ellen is currently respawning.是否处于重生状态
        protected float m_IdleTimer;                   // Used to count up to Ellen considering a random idle.空闲时间

        // These constants are used to ensure Ellen moves and behaves properly.确保艾伦正确移动的变量
        // It is advised you don't change them without fully understanding what they do in code.建议您在不完全了解它们在代码中所做的工作之前不要对其进行更改。
        const float k_AirborneTurnSpeedProportion = 5.4f;
        const float k_GroundedRayDistance = 1f;
        const float k_JumpAbortSpeed = 10f;
        const float k_MinEnemyDotCoeff = 0.2f;
        const float k_InverseOneEighty = 1f / 180f;
        const float k_StickingGravityProportion = 0.3f;
        const float k_GroundAcceleration = 20f;
        const float k_GroundDeceleration = 25f;

        // Parameters，特性
        //只读型数据
        readonly int m_HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        readonly int m_HashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
        readonly int m_HashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
        readonly int m_HashGrounded = Animator.StringToHash("Grounded");
        readonly int m_HashInputDetected = Animator.StringToHash("InputDetected");
        readonly int m_HashMeleeAttack = Animator.StringToHash("MeleeAttack");
        readonly int m_HashHurt = Animator.StringToHash("Hurt");
        readonly int m_HashDeath = Animator.StringToHash("Death");
        readonly int m_HashRespawn = Animator.StringToHash("Respawn");
        readonly int m_HashHurtFromX = Animator.StringToHash("HurtFromX");
        readonly int m_HashHurtFromY = Animator.StringToHash("HurtFromY");
        readonly int m_HashStateTime = Animator.StringToHash("StateTime");
        readonly int m_HashFootFall = Animator.StringToHash("FootFall");

        // States，状态
        readonly int m_HashLocomotion = Animator.StringToHash("Locomotion");
        readonly int m_HashAirborne = Animator.StringToHash("Airborne");
        readonly int m_HashLanding = Animator.StringToHash("Landing");    // Also a parameter.参数
        readonly int m_HashEllenCombo1 = Animator.StringToHash("EllenCombo1");
        readonly int m_HashEllenCombo2 = Animator.StringToHash("EllenCombo2");
        readonly int m_HashEllenCombo3 = Animator.StringToHash("EllenCombo3");
        readonly int m_HashEllenCombo4 = Animator.StringToHash("EllenCombo4");
        readonly int m_HashEllenDeath = Animator.StringToHash("EllenDeath");

        // Tags，标签
        readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");

        //是否移动输入
        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(m_Input.MoveInput.sqrMagnitude, 0f); }
        }

        //当能攻击时激活攻击状态
        public void SetCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }

        // Called automatically by Unity when the script is first added to a gameobject or is reset from the context menu.
        //当脚本首次添加到游戏对象或从上下文菜单重置时，由Unity自动调用。
        void Reset()
        {
            meleeWeapon = GetComponentInChildren<MeleeWeapon>();

            Transform footStepSource = transform.Find("FootstepSource");
            if (footStepSource != null)
                footstepPlayer = footStepSource.GetComponent<RandomAudioPlayer>();

            Transform hurtSource = transform.Find("HurtSource");
            if (hurtSource != null)
                hurtAudioPlayer = hurtSource.GetComponent<RandomAudioPlayer>();

            Transform landingSource = transform.Find("LandingSource");
            if (landingSource != null)
                landingPlayer = landingSource.GetComponent<RandomAudioPlayer>();

            cameraSettings = FindObjectOfType<CameraSettings>();

            if (cameraSettings != null)
            {
                if (cameraSettings.follow == null)
                    cameraSettings.follow = transform;

                if (cameraSettings.lookAt == null)
                    cameraSettings.follow = transform.Find("HeadTarget");
            }
        }

        // Called automatically by Unity when the script first exists in the scene.
        void Awake()
        {
            m_Input = GetComponent<PlayerInput>();//获取玩家输入的脚本
            m_Animator = GetComponent<Animator>();//动画控制机
            m_CharCtrl = GetComponent<CharacterController>();//人物控制器

            meleeWeapon.SetOwner(gameObject);//？

            s_Instance = this;//用s_Instance指代脚本本身
        }

        // Called automatically by Unity after Awake whenever the script is enabled. 
        void OnEnable()
        {
            SceneLinkedSMB<PlayerController>.Initialise(m_Animator, this);//初始动画控制机

            m_Damageable = GetComponent<Damageable>();
            m_Damageable.onDamageMessageReceivers.Add(this);

            m_Damageable.isInvulnerable = true;

            EquipMeleeWeapon(false);

            m_Renderers = GetComponentsInChildren<Renderer>();
        }

        // Called automatically by Unity whenever the script is disabled.
        void OnDisable()
        {
            m_Damageable.onDamageMessageReceivers.Remove(this);

            for (int i = 0; i < m_Renderers.Length; ++i)
            {
                m_Renderers[i].enabled = true;//退出游戏时执行渲染？
            }
        }

        // Called automatically by Unity once every Physics step.
        void FixedUpdate()
        {
            CacheAnimatorState();

            UpdateInputBlocking();

            EquipMeleeWeapon(IsWeaponEquiped());

            m_Animator.SetFloat(m_HashStateTime, Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            m_Animator.ResetTrigger(m_HashMeleeAttack);

            if (m_Input.Attack && canAttack)
                m_Animator.SetTrigger(m_HashMeleeAttack);

            CalculateForwardMovement();
            CalculateVerticalMovement();

            SetTargetRotation();

            if (IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();

            PlayAudio();

            TimeoutToIdle();

            m_PreviouslyGrounded = m_IsGrounded;
        }

        // Called at the start of FixedUpdate to record the current state of the base layer of the animator.
        void CacheAnimatorState()
        {
            m_PreviousCurrentStateInfo = m_CurrentStateInfo;
            m_PreviousNextStateInfo = m_NextStateInfo;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = m_Animator.IsInTransition(0);
        }

        // Called after the animator state has been cached to determine whether this script should block user input.
        void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash == m_HashBlockInput && !m_IsAnimatorTransitioning;
            inputBlocked |= m_NextStateInfo.tagHash == m_HashBlockInput;
            m_Input.playerControllerInputBlocked = inputBlocked;
        }

        // Called after the animator state has been cached to determine whether or not the staff should be active or not.
        bool IsWeaponEquiped()
        {
            bool equipped = m_NextStateInfo.shortNameHash == m_HashEllenCombo1 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo1;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo2 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo2;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo3 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo3;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo4 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo4;

            return equipped;
        }

        // Called each physics step with a parameter based on the return value of IsWeaponEquiped.
        void EquipMeleeWeapon(bool equip)//近战装备武器
        {
            meleeWeapon.gameObject.SetActive(equip);
            m_InAttack = false;
            m_InCombo = equip;

            if (!equip)
                m_Animator.ResetTrigger(m_HashMeleeAttack);
        }

        // Called each physics step.
        void CalculateForwardMovement()
        {
            // Cache the move input and cap it's magnitude at 1.
            Vector2 moveInput = m_Input.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            // Calculate the speed intended by input.
            m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

            // Determine change to speed based on whether there is currently any move input.
            float acceleration = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            // Adjust the forward speed towards the desired speed.
            m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, m_DesiredForwardSpeed, acceleration * Time.deltaTime);

            // Set the animator parameter to control what animation is being played.
            m_Animator.SetFloat(m_HashForwardSpeed, m_ForwardSpeed);
        }

        // Called each physics step.
        void CalculateVerticalMovement()
        {
            // If jump is not currently held and Ellen is on the ground then she is ready to jump.
            if (!m_Input.JumpInput && m_IsGrounded)
                m_ReadyToJump = true;

            if (m_IsGrounded)
            {
                // When grounded we apply a slight negative vertical speed to make Ellen "stick" to the ground.
                m_VerticalSpeed = -gravity * k_StickingGravityProportion;

                // If jump is held, Ellen is ready to jump and not currently in the middle of a melee combo...
                if (m_Input.JumpInput && m_ReadyToJump && !m_InCombo)
                {
                    // ... then override the previously set vertical speed and make sure she cannot jump again.
                    m_VerticalSpeed = jumpSpeed;
                    m_IsGrounded = false;
                    m_ReadyToJump = false;
                }
            }
            else
            {
                // If Ellen is airborne, the jump button is not held and Ellen is currently moving upwards...
                if (!m_Input.JumpInput && m_VerticalSpeed > 0.0f)
                {
                    // ... decrease Ellen's vertical speed.
                    // This is what causes holding jump to jump higher that tapping jump.
                    m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                // If a jump is approximately peaking, make it absolute.
                if (Mathf.Approximately(m_VerticalSpeed, 0f))
                {
                    m_VerticalSpeed = 0f;
                }
                
                // If Ellen is airborne, apply gravity.
                m_VerticalSpeed -= gravity * Time.deltaTime;
            }
        }

        // Called each physics step to set the rotation Ellen is aiming to have.
        //调用每个物理步骤以设置Ellen希望具有的旋转度。
        void SetTargetRotation()
            //设置目标旋转
        {
            // Create three variables, move input local to the player, flattened forward direction of the camera and a local target rotation.
            //创建三个变量，将输入移动到播放器本地，将摄像机向前平展，并将目标旋转到本地
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            
            Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;
            
            // If the local movement direction is the opposite of forward then the target rotation should be towards the camera.
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                // Otherwise the rotation should be the offset of the input from the camera's forward.
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            // The desired forward direction of Ellen.
            Vector3 resultingForward = targetRotation * Vector3.forward;

            // If attacking try to orient to close enemies.
            if (m_InAttack)
            {
                // Find all the enemies in the local area.
                Vector3 centre = transform.position + transform.forward * 2.0f + transform.up;
                Vector3 halfExtents = new Vector3(3.0f, 1.0f, 2.0f);
                int layerMask = 1 << LayerMask.NameToLayer("Enemy");
                int count = Physics.OverlapBoxNonAlloc(centre, halfExtents, m_OverlapResult, targetRotation, layerMask);

                // Go through all the enemies in the local area...
                float closestDot = 0.0f;
                Vector3 closestForward = Vector3.zero;
                int closest = -1;

                for (int i = 0; i < count; ++i)
                {
                    // ... and for each get a vector from the player to the enemy.
                    Vector3 playerToEnemy = m_OverlapResult[i].transform.position - transform.position;
                    playerToEnemy.y = 0;
                    playerToEnemy.Normalize();

                    // Find the dot product between the direction the player wants to go and the direction to the enemy.
                    // This will be larger the closer to Ellen's desired direction the direction to the enemy is.
                    float d = Vector3.Dot(resultingForward, playerToEnemy);

                    // Store the closest enemy.
                    if (d > k_MinEnemyDotCoeff && d > closestDot)
                    {
                        closestForward = playerToEnemy;
                        closestDot = d;
                        closest = i;
                    }
                }

                // If there is a close enemy...
                if (closest != -1)
                {
                    // The desired forward is the direction to the closest enemy.
                    resultingForward = closestForward;
                    
                    // We also directly set the rotation, as we want snappy fight and orientation isn't updated in the UpdateOrientation function during an atatck.
                    transform.rotation = Quaternion.LookRotation(resultingForward);
                }
            }

            // Find the difference between the current rotation of the player and the desired rotation of the player in radians.
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            m_TargetRotation = targetRotation;
        }

        // Called each physics step to help determine whether Ellen can turn under player input.
        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLocomotion || m_NextStateInfo.shortNameHash == m_HashLocomotion;
            bool updateOrientationForAirborne = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashAirborne || m_NextStateInfo.shortNameHash == m_HashAirborne;
            bool updateOrientationForLanding = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLanding || m_NextStateInfo.shortNameHash == m_HashLanding;

            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || m_InCombo && !m_InAttack;
        }

        // Called each physics step after SetTargetRotation if there is move input and Ellen is in the correct animator state according to IsOrientationUpdated.
        void UpdateOrientation()
        {
            m_Animator.SetFloat(m_HashAngleDeltaRad, m_AngleDiff * Mathf.Deg2Rad);

            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);
            float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
            float actualTurnSpeed = m_IsGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, actualTurnSpeed * Time.deltaTime);

            transform.rotation = m_TargetRotation;
        }

        // Called each physics step to check if audio should be played and if so instruct the relevant random audio player to do so.
        void PlayAudio()
        {
            float footfallCurve = m_Animator.GetFloat(m_HashFootFall);

            if (footfallCurve > 0.01f && !footstepPlayer.playing && footstepPlayer.canPlay)
            {
                footstepPlayer.playing = true;
                footstepPlayer.canPlay = false;
                footstepPlayer.PlayRandomClip(m_CurrentWalkingSurface, m_ForwardSpeed < 4 ? 0 : 1);
            }
            else if (footstepPlayer.playing)
            {
                footstepPlayer.playing = false;
            }
            else if (footfallCurve < 0.01f && !footstepPlayer.canPlay)
            {
                footstepPlayer.canPlay = true;
            }

            if (m_IsGrounded && !m_PreviouslyGrounded)
            {
                landingPlayer.PlayRandomClip(m_CurrentWalkingSurface, bankId: m_ForwardSpeed < 4 ? 0 : 1);
                emoteLandingPlayer.PlayRandomClip();
            }

            if (!m_IsGrounded && m_PreviouslyGrounded && m_VerticalSpeed > 0f)
            {
                emoteJumpPlayer.PlayRandomClip();
            }

            if (m_CurrentStateInfo.shortNameHash == m_HashHurt && m_PreviousCurrentStateInfo.shortNameHash != m_HashHurt)
            {
                hurtAudioPlayer.PlayRandomClip();
            }

            if (m_CurrentStateInfo.shortNameHash == m_HashEllenDeath && m_PreviousCurrentStateInfo.shortNameHash != m_HashEllenDeath)
            {
                emoteDeathPlayer.PlayRandomClip();
            }

            if (m_CurrentStateInfo.shortNameHash == m_HashEllenCombo1 && m_PreviousCurrentStateInfo.shortNameHash != m_HashEllenCombo1 ||
                m_CurrentStateInfo.shortNameHash == m_HashEllenCombo2 && m_PreviousCurrentStateInfo.shortNameHash != m_HashEllenCombo2 ||
                m_CurrentStateInfo.shortNameHash == m_HashEllenCombo3 && m_PreviousCurrentStateInfo.shortNameHash != m_HashEllenCombo3 ||
                m_CurrentStateInfo.shortNameHash == m_HashEllenCombo4 && m_PreviousCurrentStateInfo.shortNameHash != m_HashEllenCombo4)
            {
                emoteAttackPlayer.PlayRandomClip();
            }
        }

        // Called each physics step to count up to the point where Ellen considers a random idle.
        void TimeoutToIdle()
        {
            bool inputDetected = IsMoveInput || m_Input.Attack || m_Input.JumpInput;
            if (m_IsGrounded && !inputDetected)
            {
                m_IdleTimer += Time.deltaTime;

                if (m_IdleTimer >= idleTimeout)
                {
                    m_IdleTimer = 0f;
                    m_Animator.SetTrigger(m_HashTimeoutToIdle);
                }
            }
            else
            {
                m_IdleTimer = 0f;
                m_Animator.ResetTrigger(m_HashTimeoutToIdle);
            }

            m_Animator.SetBool(m_HashInputDetected, inputDetected);
        }

        // Called each physics step (so long as the Animator component is set to Animate Physics) after FixedUpdate to override root motion.
        void OnAnimatorMove()
        {
            Vector3 movement;

            // If Ellen is on the ground...
            if (m_IsGrounded)
            {
                // ... raycast into the ground...
                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    // ... and get the movement of the root motion rotated to lie along the plane of the ground.
                    movement = Vector3.ProjectOnPlane(m_Animator.deltaPosition, hit.normal);
                    
                    // Also store the current walking surface so the correct audio is played.
                    Renderer groundRenderer = hit.collider.GetComponentInChildren<Renderer>();
                    m_CurrentWalkingSurface = groundRenderer ? groundRenderer.sharedMaterial : null;
                }
                else
                {
                    // If no ground is hit just get the movement as the root motion.
                    // Theoretically this should rarely happen as when grounded the ray should always hit.
                    movement = m_Animator.deltaPosition;
                    m_CurrentWalkingSurface = null;
                }
            }
            else
            {
                // If not grounded the movement is just in the forward direction.
                movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
            }

            // Rotate the transform of the character controller by the animation's root rotation.
            m_CharCtrl.transform.rotation *= m_Animator.deltaRotation;

            // Add to the movement with the calculated vertical speed.
            movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

            // Move the character controller.
            m_CharCtrl.Move(movement);

            // After the movement store whether or not the character controller is grounded.
            m_IsGrounded = m_CharCtrl.isGrounded;

            // If Ellen is not on the ground then send the vertical speed to the animator.
            // This is so the vertical speed is kept when landing so the correct landing animation is played.
            if (!m_IsGrounded)
                m_Animator.SetFloat(m_HashAirborneVerticalSpeed, m_VerticalSpeed);

            // Send whether or not Ellen is on the ground to the animator.
            m_Animator.SetBool(m_HashGrounded, m_IsGrounded);
        }
        
        // This is called by an animation event when Ellen swings her staff.
        public void MeleeAttackStart(int throwing = 0)
        {
            meleeWeapon.BeginAttack(throwing != 0);
            m_InAttack = true;
        }

        // This is called by an animation event when Ellen finishes swinging her staff.
        public void MeleeAttackEnd()
        {
            meleeWeapon.EndAttack();
            m_InAttack = false;
        }

        // This is called by Checkpoints to make sure Ellen respawns correctly.
        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint != null)
                m_CurrentCheckpoint = checkpoint;
        }

        // This is usually called by a state machine behaviour on the animator controller but can be called from anywhere.
        public void Respawn()
        {
            StartCoroutine(RespawnRoutine());
        }
        
        protected IEnumerator RespawnRoutine()
        {
            // Wait for the animator to be transitioning from the EllenDeath state.
            while (m_CurrentStateInfo.shortNameHash != m_HashEllenDeath || !m_IsAnimatorTransitioning)
            {
                yield return null;
            }
            
            // Wait for the screen to fade out.
            yield return StartCoroutine(ScreenFader.FadeSceneOut());
            while (ScreenFader.IsFading)
            {
                yield return null;
            }

            // Enable spawning.
            EllenSpawn spawn = GetComponentInChildren<EllenSpawn>();
            spawn.enabled = true;

            // If there is a checkpoint, move Ellen to it.
            if (m_CurrentCheckpoint != null)
            {
                transform.position = m_CurrentCheckpoint.transform.position;
                transform.rotation = m_CurrentCheckpoint.transform.rotation;
            }
            else
            {
                Debug.LogError("There is no Checkpoint set, there should always be a checkpoint set. Did you add a checkpoint at the spawn?");
            }
            
            // Set the Respawn parameter of the animator.
            m_Animator.SetTrigger(m_HashRespawn);
            
            // Start the respawn graphic effects.
            spawn.StartEffect();
            
            // Wait for the screen to fade in.
            // Currently it is not important to yield here but should some changes occur that require waiting until a respawn has finished this will be required.
            yield return StartCoroutine(ScreenFader.FadeSceneIn());
            
            m_Damageable.ResetDamage();
        }

        // Called by a state machine behaviour on Ellen's animator controller.
        public void RespawnFinished()
        {
            m_Respawning = false;
            
            //we set the damageable invincible so we can't get hurt just after being respawned (feel like a double punitive)
            m_Damageable.isInvulnerable = false;
        }

        // Called by Ellen's Damageable when she is hurt.
        public void OnReceiveMessage(MessageType type, object sender, object data)
        {
            switch (type)
            {
                case MessageType.DAMAGED:
                    {
                        Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;
                        Damaged(damageData);
                    }
                    break;
                case MessageType.DEAD:
                    {
                        Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;
                        Die(damageData);
                    }
                    break;
            }
        }

        // Called by OnReceiveMessage.
        void Damaged(Damageable.DamageMessage damageMessage)
        {
            // Set the Hurt parameter of the animator.
            m_Animator.SetTrigger(m_HashHurt);

            // Find the direction of the damage.
            Vector3 forward = damageMessage.damageSource - transform.position;
            forward.y = 0f;

            Vector3 localHurt = transform.InverseTransformDirection(forward);

            // Set the HurtFromX and HurtFromY parameters of the animator based on the direction of the damage.
            m_Animator.SetFloat(m_HashHurtFromX, localHurt.x);
            m_Animator.SetFloat(m_HashHurtFromY, localHurt.z);

            // Shake the camera.
            CameraShake.Shake(CameraShake.k_PlayerHitShakeAmount, CameraShake.k_PlayerHitShakeTime);

            // Play an audio clip of being hurt.
            if (hurtAudioPlayer != null)
            {
                hurtAudioPlayer.PlayRandomClip();
            }
        }

        // Called by OnReceiveMessage and by DeathVolumes in the scene.
        public void Die(Damageable.DamageMessage damageMessage)
        {
            m_Animator.SetTrigger(m_HashDeath);
            m_ForwardSpeed = 0f;
            m_VerticalSpeed = 0f;
            m_Respawning = true;
            m_Damageable.isInvulnerable = true;
        }
    }
}