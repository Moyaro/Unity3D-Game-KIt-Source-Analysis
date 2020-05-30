using UnityEngine;
using UnityEditor;


namespace Gamekit3D
{
    public class AddObjectiveSubEditor : SubEditor<ScenarioController>//增加物体的子编辑器
    {
        string newObjectiveName = "";//新物体名字
        bool showAdd = false;//Add按钮的显示和隐藏
        string addError = "";//显示错误

        public override void OnInspectorGUI(ScenarioController sc)
        {

            if (!showAdd && GUILayout.Button("Add New Objective")) //GUILayout无需设定显示区域，系统会自动帮我们计算控件显示区域，并保证不重叠和影响里面的显示效果。 https://blog.csdn.net/u013289188/article/details/29618897
            {
                showAdd = true;
                GUI.FocusControl("ObjectiveName"); //移动键盘焦点到ObjectiveName控件。
            }
            if (showAdd)
            {                      //using 用于定义一个范围，在此范围的末尾将释放对象
                using (new GUILayout.HorizontalScope()/*所有放在这里面的元素都会被水平排列放置*/)
                {
                    GUI.SetNextControlName("ObjectiveName");//设置下一个控件的名字
                    GUILayout.Label("Objective Name");//创建一个自动布局的标签。
                    newObjectiveName = EditorGUILayout.TextField(newObjectiveName).ToUpper();//TextField创建一个单行文本字段，用户可以编辑其中的字符串，toupper：将小写英文字母改为对应的大写字母
                    using (new EditorGUI.DisabledScope(newObjectiveName == ""))//如果此时还没有新物体的名字，就增加物体的名字。disabledscope:设置一个范围并创建一个组。
                    {
                        if (GUILayout.Button("Add"))
                        {
                            if (sc.AddObjective(newObjectiveName, 1))//参数：增加物体的名字和数量。如果增加成功，将所有数据进行重置
                            {
                                Close();
                            }
                            else
                            {
                                addError = "This objective name already exists.";
                            }
                        }
                    }
                }
                if (addError != "")
                {
                    EditorGUILayout.HelpBox(addError, MessageType.Error);//将错误类型设置为error，并在helpbox类打出提示信息  https://www.cnblogs.com/backlighting/p/5061576.html
                }
                if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)//如果此时在用键盘并且键盘所按键为ESC
                {
                    Close();
                }
            }
        }

        void Close()
        {
            newObjectiveName = "";
            addError = "";
            showAdd = false;
            EditorGUIUtility.editingTextField = false;
            Repaint();//重绘界面控件
        }

    } 
}


//关于GUILayout类的函数的参照表 https://blog.csdn.net/likendsl/article/details/50254827
