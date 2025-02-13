using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using HotChocolate.Utils;

#if UNITY_2020_1_OR_NEWER
namespace HotChocolate.FTUE
{
    [CustomPropertyDrawer(typeof(FtueCompositeCondition))]
    public class FtueCompositeConditionDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> conditionsLists = new Dictionary<string, ReorderableList>();
        private IEnumerable<Instantiator> conditionsInstantiators;
        private int conditionTypeIndex;

        private float height = 0;
        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float listHeight = 0;
            var conditionsProperty = property.FindPropertyRelative("conditions");

            if (conditionsLists.ContainsKey(conditionsProperty.propertyPath))
            {
                listHeight = conditionsLists[conditionsProperty.propertyPath].GetHeight();
            }

            return height + spacing + listHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;

            var ftueEvent = property.serializedObject.targetObject as IFtueConditionHolder;
            conditionsInstantiators = ftueEvent.ConditionsInstantiators();

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
            height += EditorGUIUtility.singleLineHeight + spacing;

            var invertProperty = property.FindPropertyRelative("invert");
            var unionProperty = property.FindPropertyRelative("union");
            var conditionsProperty = property.FindPropertyRelative("conditions");

            EditorGUI.PropertyField(new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight), invertProperty);
            height += EditorGUIUtility.singleLineHeight + spacing;

            EditorGUI.PropertyField(new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight), unionProperty);
            height += EditorGUIUtility.singleLineHeight + spacing;

            if (!conditionsLists.ContainsKey(conditionsProperty.propertyPath))
            {
                BuildReorderableList(conditionsProperty);
            }

            conditionsLists[conditionsProperty.propertyPath].DoList(new Rect(position.x, position.y + height, position.width, TotalListHeight(conditionsProperty)));
            float listHeight = conditionsLists[conditionsProperty.propertyPath].GetHeight();

            float buttonSize = 200f;
            conditionTypeIndex = EditorGUI.Popup(new Rect(position.x, position.y + height + listHeight, position.width - buttonSize - spacing, EditorGUIUtility.singleLineHeight), conditionTypeIndex, conditionsInstantiators.Select(c => c.displayName).ToArray());

            if (GUI.Button(new Rect(position.x + position.width - buttonSize, position.y + height + listHeight, buttonSize, EditorGUIUtility.singleLineHeight), "Add"))
            {
                OnAddCallback(conditionsLists[conditionsProperty.propertyPath]);
            }

            height += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        private float TotalListHeight(SerializedProperty property)
        {
            float height = 0;
            for(int i = 0; i < property.arraySize; ++i)
            {
                height += ElementHeightCallback(property, i);
            }

            return height;
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight = EditorGUI.GetPropertyHeight(conditionsLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var instantiator = conditionsInstantiators.ElementAt(conditionTypeIndex);
            element.managedReferenceValue = instantiator.Instantiate();
        }

        private void BuildReorderableList(SerializedProperty property)
        {
            conditionsLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Conditions");
                }
            });

            conditionsLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            conditionsLists[property.propertyPath].onAddCallback += OnAddCallback;

            conditionsLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = conditionsLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
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