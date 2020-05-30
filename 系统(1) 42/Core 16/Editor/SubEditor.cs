using UnityEditor;

namespace Gamekit3D
{
	//该类主要用于对画面的重置对现有委托进行使用并清除
        //抽象类：子编辑器，不能生成实例  关于abstract的详细说明https://blog.csdn.net/yk_lin/article/details/83469094
        //泛型<T> https://www.sogou.com/link?url=hedJjaC291OB0PrGj_c3jJzmXqp0xreSchXwq6KZroyroHKd7_5kXCbc-iPH6wf8jzYWkh2yY3ynXTW0b-n0YQ..
    public abstract class SubEditor<T>
    {
        public abstract void OnInspectorGUI(T instance);

        public void Init(Editor editor)//获得当前编辑器
        {
            this.editor = editor;
        }

        public void Update()
        {
            if (defer != null) defer();//如果当前委托不为空，就运行当前委托，然后再次设为空
            defer = null;
        }

        protected void Defer(System.Action fn)//在委托中添加要执行的方法
        {
            defer += fn;
        }

        protected void Repaint()//用来重绘界面所有的控件
        {
            editor.Repaint();
        }

        Editor editor;
        System.Action defer;//一种特殊委托  https://blog.csdn.net/qq_38187606/article/details/78928876
    } 
}
