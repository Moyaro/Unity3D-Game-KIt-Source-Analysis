using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class FootStepEffect : MonoBehaviour//脚步效应
    {

        public Transform rayStart;
        public Transform particleOffset;
        public ParticleSystem PS;

        // Use this for initialization
        //用于初始化
        void Start()
        {

        }

        // Update is called once per frame
        //每帧调用一次更新
        void Update()
        {
            RaycastHit hit;//用于存储射线碰撞到的第一个对象信息
            if (!Physics.Raycast(rayStart.position, -Vector3.up, out hit))
                //从射线的起点，沿这个方向，产生hit信息的射线
                //https://blog.csdn.net/pdw_jsp/article/details/52182464
                return;

            MeshCollider meshCollider = hit.collider as MeshCollider;//网格对撞机
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;
            //获取碰撞器所在物体的Mesh网格

            Mesh mesh = meshCollider.sharedMesh;//获取Mesh网格的所有顶点
            Vector3[] vertices = mesh.vertices; 
            //获取mesh的三角形索引，这里的索引的就是模型顶点数组的下标
            Color[] colors = mesh.colors;
            //获取mesh的顶点颜色索引
            int[] triangles = mesh.triangles;
            //然后通过hit.triangleIndex(摄像碰撞到的三角形的第一个点的索引)
            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
            Transform hitTransform = hit.collider.transform;
            //上面的三个顶点是Mesh的本地坐标，需要用模型的Transform进行转换到世界坐标
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);
            //然后设置三个小球的位置到这个三个点，方便调试
            Debug.DrawLine(p0, p1);
            Debug.DrawLine(p1, p2);
            Debug.DrawLine(p2, p0);
            //将输入值打印到控制台
            //https://www.pianshen.com/article/3847739271/

            //closest point
            //最近点
            Vector3 closest = p0;//设置最近点是p0
            int vertNum = 0;//垂直数为0
            if (Vector3.Distance(hit.point, p1) < Vector3.Distance(hit.point, closest))
                //如果碰撞到p1的距离小于到最近点的距离
            {
                closest = p1;//最近点是p1
                vertNum = 1;//垂直数为1
            }

            if (Vector3.Distance(hit.point, p2) < Vector3.Distance(hit.point, closest))
            //如果碰撞到p2的距离小于到最近点的距离
            {
                closest = p2;//最近点是p2
                vertNum = 2;//垂直数是2
            }
            var emission = PS.emission;//将粒子系统的自发光效果赋给emission

            Color col = colors[triangles[hit.triangleIndex * 3 + vertNum]];
            //获取击中点的颜色
            if (col.a < 0.5)//如果透明度小于0.5
            {
                Debug.DrawLine(closest, new Vector3(closest.x, closest.y + 1, closest.z), Color.red);
                //从最近点，到这个结束点，用红色画一条线
                particleOffset.localPosition = new Vector3(0, hit.point.y - this.transform.position.y, 0);
                emission.enabled = true;//开启粒子系统
            }
            else
            {
                emission.enabled = false;//不启用粒子系统
            }




        }
    } 
}
