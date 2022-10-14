
using System.Collections.Generic;
using System.Linq;
using System;

namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4.Common;
	using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(EliasIntValue))]
    [CustomPropertyDrawer(typeof(EliasDoubleValue))]
    [CustomPropertyDrawer(typeof(EliasBoolValue))]
    [CustomPropertyDrawer(typeof(EliasEnumValue))]
	public class EliasValueEditor :
		PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{ 
            var prop = property.FindPropertyRelative("value");
            using (var propScope = new EditorGUI.PropertyScope(position, label, property)) {
                EditorGUI.PropertyField(position, prop, label);
            }
        }
    }
}