using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using HotChocolate.Utils;

#if UNITY_2020_1_OR_NEWER
namespace HotChocolate.FTUE
{
    [CustomPropertyDrawer(typeof(FtueSequence))]
    public class FtueSequenceDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> stepsLists = new Dictionary<string, ReorderableList>();
        private IEnumerable<Instantiator> stepsInstantiators;
        private int stepTypeIndex;

        private float height = 0;
        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float listHeight = 0;
            var stepsProperty = property.FindPropertyRelative("steps");

            if (stepsLists.ContainsKey(stepsProperty.propertyPath))
            {
                listHeight = stepsLists[stepsProperty.propertyPath].GetHeight();
            }

            return height + spacing + listHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;

            var ftueEvent = property.serializedObject.targetObject as IFtueEvent;
            stepsInstantiators = ftueEvent.StepsInstantiators();

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Sequence");
            height += EditorGUIUtility.singleLineHeight + spacing;

            var stepsProperty = property.FindPropertyRelative("steps");

            if (!stepsLists.ContainsKey(stepsProperty.propertyPath))
            {
                BuildReorderableList(stepsProperty);
            }

            stepsLists[stepsProperty.propertyPath].DoList(new Rect(position.x, position.y + height, position.width, TotalListHeight(stepsProperty)));
            float listHeight = stepsLists[stepsProperty.propertyPath].GetHeight();

            float buttonSize = 200f;
            stepTypeIndex = EditorGUI.Popup(new Rect(position.x, position.y + height + listHeight, position.width - buttonSize - spacing, EditorGUIUtility.singleLineHeight), stepTypeIndex, stepsInstantiators.Select(s => s.displayName).ToArray());

            if (GUI.Button(new Rect(position.x + position.width - buttonSize, position.y + height + listHeight, buttonSize, EditorGUIUtility.singleLineHeight), "Add"))
            {
                OnAddCallback(stepsLists[stepsProperty.propertyPath]);
            }

            height += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        private float TotalListHeight(SerializedProperty property)
        {
            float height = 0;
            for (int i = 0; i < property.arraySize; ++i)
            {
                height += ElementHeightCallback(property, i);
            }

            return height;
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight = EditorGUI.GetPropertyHeight(stepsLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var instantiator = stepsInstantiators.ElementAt(stepTypeIndex);
            element.managedReferenceValue = instantiator.Instantiate();
        }

        private void BuildReorderableList(SerializedProperty property)
        {
            stepsLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Steps");
                }
            });

            stepsLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            stepsLists[property.propertyPath].onAddCallback += OnAddCallback;

            stepsLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = stepsLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height), prop, new GUIContent(GetManagedTypeName(prop)), true);
            };
        }

        private string GetManagedTypeName(SerializedProperty property)
        {
            var tokens = property.managedReferenceFullTypename.Split('.');
            if (tokens.Length > 0)
                return tokens[tokens.Length - 1];

            return property.managedReferenceFullTypename;
        }
    }
}
#endif