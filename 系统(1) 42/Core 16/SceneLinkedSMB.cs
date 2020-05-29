using UnityEngine;
using UnityEngine.Animations;

namespace Gamekit3D
{
    public class SceneLinkedSMB<TMonoBehaviour> : SealedSMB //模板类，不同场景之间发送信息，根据帧数来控制各种信息的发送
        where TMonoBehaviour : MonoBehaviour //泛型约束，指定monobehaviour为基类    [HelpURL("https://www.cnblogs.com/soundcode/p/5798769.html")]
    {
        protected TMonoBehaviour m_MonoBehaviour;
    
        bool m_FirstFrameHappened;//是否为第一帧 
        bool m_LastFrameHappened;//是否为最后一帧

        public static void Initialise (Animator animator, TMonoBehaviour monoBehaviour)//初始化
        {
            SceneLinkedSMB<TMonoBehaviour>[] sceneLinkedSMBs = animator.GetBehaviours<SceneLinkedSMB<TMonoBehaviour>>();//返回挂载在AnimatorController上的行为脚本StateMachineBehaviour
                                                                                                                         //通常StateMachineBehaviour之间的交互会经常用到。

            for (int i = 0; i < sceneLinkedSMBs.Length; i++)//执行对数组内各个行为脚本的初始化
            {
                sceneLinkedSMBs[i].InternalInitialise(animator, monoBehaviour);
            }
        }

        protected void InternalInitialise (Animator animator, TMonoBehaviour monoBehaviour)//内部初始化
        {
            m_MonoBehaviour = monoBehaviour;//获取当前模板类
            OnStart (animator);//开始时的事件
        }

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)//动画开启时，执行如下方法
        {
            m_FirstFrameHappened = false;//第一帧还没开始

            OnSLStateEnter(animator, stateInfo, layerIndex);
            OnSLStateEnter (animator, stateInfo, layerIndex, controller);
        }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if(!animator.gameObject.activeSelf)
                return;
        
            if (animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)//如果当前动画在转换状态并且得到当前动画控制器的下一个状态信息，如与当前动画状态是相同的，执行以下方法
            {
                OnSLTransitionToStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionToStateUpdate(animator, stateInfo, layerIndex, controller);
            }

            if (!animator.IsInTransition(layerIndex) && m_FirstFrameHappened)//如果当前动画不处于转换状态并且第一帧已开始，调用如下函数
            {
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex, controller);
            }
        
            if (animator.IsInTransition(layerIndex) && !m_LastFrameHappened && m_FirstFrameHappened)//如果动画处于转换状态并且第一帧已开始，并且并非最后一帧，调用如下方法
            {
                m_LastFrameHappened = true;//现在为最后一帧
            
                OnSLStatePreExit(animator, stateInfo, layerIndex);
                OnSLStatePreExit(animator, stateInfo, layerIndex, controller);
            }

            if (!animator.IsInTransition(layerIndex) && !m_FirstFrameHappened)//如果不是动画不在转换中并且第一帧没有开始，调用如下方法
            {
                m_FirstFrameHappened = true;//现在为第一帧

                OnSLStatePostEnter(animator, stateInfo, layerIndex);
                OnSLStatePostEnter(animator, stateInfo, layerIndex, controller);
            }

            if (animator.IsInTransition(layerIndex) && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash) //如果当前动画在转换状态并且得到目前动画状态信息，如与当前动画状态是相同的，执行以下方法
            {
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex, controller);
            }
        }
        //sealed：防止在方法所在类的派生类中对该方法的重载。
        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)//当动画处于关闭状态，执行以下方法
        {
            m_LastFrameHappened = false;//此时为最后一帧

            OnSLStateExit(animator, stateInfo, layerIndex);
            OnSLStateExit(animator, stateInfo, layerIndex, controller);
        }

        /// <summary>
        /// Called by a MonoBehaviour in the scene during its Start function.
        /// 场景中的单个行为在其开始功能期间调用。
        /// </summary>
        public virtual void OnStart(Animator animator) { }

        /// <summary>
        /// Called before Updates when execution of the state first starts (on transition to the state).
        /// 当状态的执行第一次开始时(转换到状态时)，在更新之前调用。
        /// </summary>
        public virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called after OnSLStateEnter every frame during transition to the state.
        /// 在转换到状态期间，在开始状态后调用输入每一帧。
        /// </summary>
        public virtual void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called on the first frame after the transition to the state has finished.
        /// 在转换到状态后的第一帧调用。
        /// </summary>
        public virtual void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called every frame after PostEnter when the state is not being transitioned to or from.
        /// 当状态未转换为或未转换为时，在输入后每隔一帧调用一次。
        /// </summary>
        public virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called on the first frame after the transition from the state has started.  Note that if the transition has a duration of less than a frame, this will not be called.
        /// 从状态转换开始后的第一帧调用。请注意，如果过渡的持续时间小于一帧，则不会调用它。
        /// </summary>
        public virtual void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called after OnSLStatePreExit every frame during transition to the state.
        /// 在转换到状态的过程中，在OnSLStatePreExit之后调用每个帧。
        /// </summary>
        public virtual void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called after Updates when execution of the state first finshes (after transition from the state).
        /// 第一次执行状态时在更新后调用(从状态转换后)。
        /// </summary>
        public virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called before Updates when execution of the state first starts (on transition to the state
        /// 当状态的执行第一次开始时(转换到状态时)，在更新之前调用。
        /// </summary>
        public virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after OnSLStateEnter every frame during transition to the state
        /// 在转换到状态期间，在开始状态后调用输入每一帧。
        /// </summary>
        public virtual void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called on the first frame after the transition to the state has finished.
        /// 在转换到状态后的第一帧调用。
        /// </summary>
        public virtual void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called every frame when the state is not being transitioned to or from.
        /// 当状态未转换为或未转换为时，调用每个帧。
        /// </summary>
        public virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called on the first frame after the transition from the state has started.  Note that if the transition has a duration of less than a frame, this will not be called.
        /// 从状态转换开始后的第一帧调用。请注意，如果过渡的持续时间小于一帧，则不会调用它。
        /// </summary>
        public virtual void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after OnSLStatePreExit every frame during transition to the state.
        /// 在转换到状态的过程中，在OnSLStatePreExit之后调用每个帧。
        /// </summary>
        public virtual void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after Updates when execution of the state first finshes (after transition from the state).
        /// 第一次执行状态时在更新后调用(从状态转换后)。
        /// </summary>
        public virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
    }

    //此类替换正常的StateMachineBehaviour。它增加了直接引用对象的可能性 This class repalce normal StateMachineBehaviour. It add the possibility of having direct reference to the object
    //该状态正在运行，避免了每次通过GetComponent检索它的成本。 the state is running on, avoiding the cost of retrienving it through a GetComponent every time.
    //关于更深入解释在c.f文件里。 c.f. Documentation for more in depth explainations.
    public abstract class SealedSMB : StateMachineBehaviour
    {    //sealed：防止在方法所在类的派生类中对该方法的重载。
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }//处于开启状态

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }//处于更新状态

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }//处于关闭状态
    }
}