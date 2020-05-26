using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamekit3D
{
    /// <summary>
    /// Little helper class that allow to display a message in the inspector for documentation on some gameobject.
    /// 允许在检查器中显示消息以提供有关某些游戏对象的文档
    /// </summary>
    public class InspectorHelpMessage : MonoBehaviour
    {
        public string message;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InspectorHelpMessage))]
    public class InspectorHelpMessageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox((target as InspectorHelpMessage).message, MessageType.Info);
        }
    }
#endif 
}