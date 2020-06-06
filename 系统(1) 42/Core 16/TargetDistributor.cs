using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    // This class allow to distribute arc around a target, used for "crowding" by ennemis, so they all
    // come at the player (or any target) from different direction.
    //这个类允许在一个目标周围分布弧线，用于ennemis的“crowing”，所以它们都来自不同的方向。
    [DefaultExecutionOrder(-1)]
    //调用通过设置脚本执行顺序来调整脚本的Awake/OnEnable/Update函数的顺序依次运行
    public class TargetDistributor : MonoBehaviour//目标分配器
    {
        //Use as a mean to communicate between this target and the followers
        //用于目标和跟随者之间的通信手段
        public class TargetFollower//目标跟随
        {
            //target should set that to true when they require the system to give them a position
            //当目标需要系统给他们一个位置时，应该将其设置为true
            public bool requireSlot;//需要位置
            //will be -1 if none is currently assigned
            //如果当前未分配，则将为-1
            public int assignedSlot;//分配位置
            //the position the follower want to reach for the target.
            //跟随者想要到达目标的位置
            public Vector3 requiredPoint;//定义向量

            public TargetDistributor distributor;//定义一个分配器

            public TargetFollower(TargetDistributor owner)//目标跟随
            {
                distributor = owner;//将owner赋给分配器
                requiredPoint = Vector3.zero;//给需求点赋值
                requireSlot = false;//关闭需要位置
                assignedSlot = -1;//给分配位置赋值
            }
        }

        public int arcsCount;//弧数

        protected Vector3[] m_WorldDirection;//定义我的世界位置向量坐标

        protected bool[] m_FreeArcs;//自由弧
        protected float arcDegree;//弧度

        protected List<TargetFollower> m_Followers;//定义目标跟随的一个数组

        public void OnEnable()//启用时
        {
            m_WorldDirection = new Vector3[arcsCount];//我的世界位置坐标为新的弧数坐标
            m_FreeArcs = new bool[arcsCount];//自由弧为是否有新的弧数

            m_Followers = new List<TargetFollower>();//定义一个新的数组

            arcDegree = 360.0f / arcsCount;//弧度为360/弧数
            Quaternion rotation = Quaternion.Euler(0, -arcDegree, 0);
            //返回一个绕y轴旋转负的弧度的四元数
            Vector3 currentDirection = Vector3.forward;//给当前方向赋值
            for (int i = 0; i < arcsCount; ++i)//当i比弧度小的时候
            {
                m_FreeArcs[i] = true;//生成一个自由弧
                m_WorldDirection[i] = currentDirection;//我的世界位置坐标为当前设定位置
                currentDirection = rotation * currentDirection;//重新计算当前位置
            }
        }

        public TargetFollower RegisterNewFollower()//设置新的跟随者
        {
            TargetFollower follower = new TargetFollower(this);//目标跟随为当前这个目标
            m_Followers.Add(follower);//目标跟随添加一个新跟随者
            return follower;//返回新跟随者
        }

        public void UnregisterFollower(TargetFollower follower)//注销跟随者
        {
            if (follower.assignedSlot != -1)//如果跟随者的分配位置不等于-1
            {
                m_FreeArcs[follower.assignedSlot] = true;//启用跟随者的分配位置变为自由弧
            }
        //如果等于-1

            m_Followers.Remove(follower);//移除这个跟随者
        }

        //at the end of the frame, we distribute target position to all follower that asked for one.
        //在框架的最后，我们将目标位置分配给所有要求的跟随者
        private void LateUpdate()//最后更新
        {
            for (int i = 0; i < m_Followers.Count; ++i)//当i<目标跟随的数时
            {
                var follower = m_Followers[i];//跟随者为i对应的跟随者

        
                if (follower.assignedSlot != -1)//如果跟随者的分配位置不等于-1
                {
                    m_FreeArcs[follower.assignedSlot] = true;//跟随者的分配位置变为自由弧
                 //we free whatever arc this follower may already have. 
                 //我们释放这个跟随者可能已经拥有的任何弧线
                }

                if (follower.requireSlot)//如果跟随者的需求点存在
                {
                    follower.assignedSlot = GetFreeArcIndex(follower);
                    //将获取的自由弧索引赋给跟随者的分配位置
                }
                //If it still need it, it will be picked again next lines.
                //如果它还需要它，它将被再次执行下一行
            }
        }

        public Vector3 GetDirection(int index)//获取位置
        {
            return m_WorldDirection[index];//将这个位置带入我的世界位置并返回
        }
        //if it changed position the new one will be picked.
        //如果它改变了位置，新的将被选中

        public int GetFreeArcIndex(TargetFollower follower)//获取跟随者的自由弧索引
        {
            bool found = false;

            Vector3 wanted = follower.requiredPoint - transform.position;
            //预计位置=需要点-当前位置
            Vector3 rayCastPosition = transform.position + Vector3.up * 0.4f;
            //射线投射位置=当前位置+上方位置的0.4倍

            wanted.y = 0;//将预计位置的y值设置为0
            float wantedDistance = wanted.magnitude;//预计距离为预计的长度

            wanted.Normalize();//用来将wanted中字符的不同表示方法统一为同样的形式

            float angle = Vector3.SignedAngle(wanted, Vector3.forward, Vector3.up);//？？？
            if (angle < 0)//如果计算结果小于0
                angle = 360 + angle;//结果加360

            int wantedIndex = Mathf.RoundToInt(angle / arcDegree);//结果四舍五入赋给需要索引
            if (wantedIndex >= m_WorldDirection.Length)//如果大于等于我的世界位置长度
                wantedIndex -= m_WorldDirection.Length;//减去我的世界位置

            int choosenIndex = wantedIndex;//选择索引=需要索引

            RaycastHit hit;//定义一个射线
            if (!Physics.Raycast(rayCastPosition, GetDirection(choosenIndex), out hit, wantedDistance))
                //如果没有从射线投射位置到预计距离，沿选择索引的方向这样的射线
                found = m_FreeArcs[choosenIndex];//found为选择索引的自由弧的bool值

            if (!found)//如果没有found值
            {//we are going to test left right with increasing offset
                //我们要测试左向右偏移
                int offset = 1;
                int halfCount = arcsCount / 2;
                while (offset <= halfCount)//当offset小于等于弧数的一半时
                {
                    int leftIndex = wantedIndex - offset;//向左偏移
                    int rightIndex = wantedIndex + offset;//向右偏移

                    if (leftIndex < 0) leftIndex += arcsCount;//如果向左偏移索引量小于0，重新计算索引量
                    if (rightIndex >= arcsCount) rightIndex -= arcsCount;//如果向右偏移索引大于等于弧数，重新计算
                    

                    if (!Physics.Raycast(rayCastPosition, GetDirection(leftIndex), wantedDistance) &&
                        m_FreeArcs[leftIndex])
                    //如果没有从射线投射位置到预计位置向左偏移，沿左偏移选择索引的方向这样的射线
                    {
                        choosenIndex = leftIndex;//将左偏移索引赋给选择索引
                        found = true;//found调整为true
                        break;
                    }

                    if (!Physics.Raycast(rayCastPosition, GetDirection(rightIndex), wantedDistance) &&
                        m_FreeArcs[rightIndex])
                    //如果没有从射线投射位置到预计位置向右偏移，沿右偏移选择索引的方向这样的射线
                    {
                        choosenIndex = rightIndex;//将右偏移索引赋给选择索引
                        found = true;//found调整为true
                        break;
                    }

                    offset += 1;//offset次数+1
                }
            }

            if (!found)//如果没有found值
            {//we couldn't find a free direction, return -1 to tell the caller there is no free space
                //找不到空闲方向，返回-1告诉呼叫方没有空闲空间
                return -1;//返回-1
            }

            m_FreeArcs[choosenIndex] = false;//关闭选择索引的自由弧
            return choosenIndex;//返回选择索引
        }

        public void FreeIndex(int index)//自由索引
        {
            m_FreeArcs[index] = true;//启用自由索引
        }
    }

}