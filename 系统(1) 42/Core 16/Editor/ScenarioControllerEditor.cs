using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gamekit3D//场景控制的编辑器
{
    [CustomEditor(typeof(ScenarioController))]//当我们需要为一个组件添加自己自定义的编辑器内容时，需要用到这个
    public class ScenarioControllerEditor : Editor
    {
        AddObjectiveSubEditor addObjectiveSubEditor = new AddObjectiveSubEditor();///实例化增加物体子编辑器

        int showEvent = -1;//显示事件

        public void OnEnable()//获取增加物体的编辑器
        {
            addObjectiveSubEditor.Init(this);
        }

        public override void OnInspectorGUI()//绘制各种按键和标签
        {
            var sc = target as ScenarioController;//将当前目标改为ScenarioController类型
            var so = new SerializedObject(sc);//对sc进行序列化存储于so中。序列化：用于传输
            so.Update();//更新序列化物体。

            addObjectiveSubEditor.OnInspectorGUI(sc);//进行初始按键设置
            EditorGUILayout.Space();//在 Inspector 面板两个元素之间空出一行。

            // var objectives = sc.GetAllObjectives();
            using (new EditorGUILayout.HorizontalScope("box"))//创建一个box型的水平区域
            {
                GUILayout.Label("Objective");//添加一个Objective标签
                if (Application.isPlaying)//在Unity编辑器中，如果处于播放模式时返回真。
                {
                    EditorGUILayout.LabelField("Current", GUILayout.Width(64));//标签字段，用于只读，64的宽度
                }
                EditorGUILayout.LabelField("Required", GUILayout.Width(64));
                EditorGUILayout.LabelField("", GUILayout.Width(64));
            }
            var property = so.FindProperty("objectives");//根据查找objectives的序列化属性
            using (var check = new EditorGUI.ChangeCheckScope()/*检查代码块中是否有任何控件被更改。当需要检查代码块中的图形用户界面设置时，将代码包装在一个ChangeCheckScope中*/)
            {                        //数组大小
                for (var i = 0; i < property.arraySize; i++)//
                {
                    var o = property.GetArrayElementAtIndex(i);//返回数组中i位置的元素。
                    GUI.backgroundColor = showEvent == i ? Color.green : Color.white;//根据事件是否发生来改变这个编辑器的颜色
                    using (new EditorGUILayout.HorizontalScope("box"))//创建一个box型的水平区域
                    {
                        if (GUILayout.Button(o.FindPropertyRelative("name").stringValue))//找到name属性的序列化值，将其变为字符串，作为button的名字。当按下该按钮时，返回true
                        {
                            showEvent = i;//将事件设为i
                        }

                        if (Application.isPlaying)//如果当前为播放模式
                        {                                                                           //不显示标签         宽度64
                            EditorGUILayout.PropertyField(o.FindPropertyRelative("currentCount"), GUIContent.none, GUILayout.Width(64));//添加currentCount属性
                        }
                        EditorGUILayout.PropertyField(o.FindPropertyRelative("requiredCount"), GUIContent.none, GUILayout.Width(64));
                        if (GUILayout.Button("Remove", GUILayout.Width(64)))//创建remove按钮，点击该按钮，返回true
                        {
                            property.DeleteArrayElementAtIndex(i);//数组数据删减 [HelpURL("http://blog.sina.com.cn/s/blog_1491e52310102wuio.html"]
                            so.ApplyModifiedProperties();//
                        }
                    }
                    if (showEvent == i)//如果展示事件等于i
                    {
                        if (o.FindPropertyRelative("requiredCount").intValue > 1)//如果requireCount属性的序列化的整数值大于1
                        {
                            EditorGUILayout.PropertyField(o.FindPropertyRelative("OnProgress"));//添加OnProgress属性在面板
                        }
                        EditorGUILayout.PropertyField(o.FindPropertyRelative("OnComplete"));//添加OnComplete属性
                    }
                    GUI.backgroundColor = Color.white;//背景改为白色
                }
            }
            EditorGUILayout.PropertyField(so.FindProperty("OnAllObjectivesComplete"));//添加OnAllObjectivesComplete属性
            so.ApplyModifiedProperties();//保存修改的属性
            //base.OnInspectorGUI();
        }
        //  [HelpURL("https://blog.csdn.net/q764424567/article/details/80908614?utm_medium=distribute.pc_relevant.none-task-blog-BlogCommendFromMachineLearnPai2-2.nonecase&depth_1-utm_source=distribute.pc_relevant.none-task-blog-BlogCommendFromMachineLearnPai2-2.nonecase")]
        //此为编辑器各种方法的详细参考
    }
}
