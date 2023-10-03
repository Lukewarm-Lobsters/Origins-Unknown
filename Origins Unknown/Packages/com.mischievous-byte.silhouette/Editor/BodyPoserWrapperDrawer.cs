using MischievousByte.Silhouette;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MischievousByte.SilhouetteEditor
{
    /*[CustomPropertyDrawer(typeof(BodyPoserWrapper))]
    public class BodyPoserWrapperDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty settingsProperty = property.FindPropertyRelative("settings");

            return EditorGUI.GetPropertyHeight(settingsProperty);
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty assetProperty = property.FindPropertyRelative("asset");
            SerializedProperty contextProperty = property.FindPropertyRelative("context");
            SerializedProperty typeNameProperty = property.FindPropertyRelative("typeName");

            property.isExpanded = true;

            bool hasAsset = assetProperty.objectReferenceValue != null;

            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeNameProperty.stringValue)).Where(t => t != null).FirstOrDefault();

            Rect valuePosition = position;
            valuePosition.height = EditorGUIUtility.singleLineHeight;

            var obj = EditorGUI.ObjectField(valuePosition, label, assetProperty.objectReferenceValue, type, true);

            if (obj == null)
            {
                assetProperty.objectReferenceValue = null;
                return;
            }


            if (!IsContextualAsset(obj.GetType()))
                return;

            assetProperty.objectReferenceValue = obj;

            if (contextProperty.managedReferenceValue == null)
                return;

            EditorGUI.PropertyField(position, contextProperty, new GUIContent(new string(' ', label.text.Length)), true);
        }
    }*/

}
