using HotChocolate.UI.Styles;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Racing.FTUE
{
    [CustomEditor(typeof(ButtonStyleConfig), true)]
    public class ButtonStyleConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

            ButtonStyleConfig config = target as ButtonStyleConfig;
            if (config.type == ButtonStyleConfig.ButtonType.BouncyTween)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenScale"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenEasing"));
            }
            else if(config.type == ButtonStyleConfig.ButtonType.UnityStandard)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("transition"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colors"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationTriggers"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteState"));
            }
    
            serializedObject.ApplyModifiedProperties();
        }
    }
}
