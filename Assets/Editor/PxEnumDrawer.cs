using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PseudoExtensibleEnum
{

    /// <summary>
    /// Property drawer for fields using PxEnum pseudo-extension of enums
    /// </summary>
    [CustomPropertyDrawer(typeof(PxEnumPropertyAttribute))]
    public class PxEnumDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PxEnumPropertyAttribute pxEnumPropertyAttribute = (PxEnumPropertyAttribute)attribute;
            Type baseType = pxEnumPropertyAttribute.BaseType;
            bool isEnum = baseType.IsEnum;
            bool isFlags = baseType.IsDefined(typeof(FlagsAttribute), false);
            List<KeyValuePair<long, string>> options = null;
            if(isEnum)
            {
                options = PxEnum.GetValueNameCollection(baseType, false);
            }

            label = EditorGUI.BeginProperty(position, label, property); // this appears to be a complete fucking nop

            var controlPosition = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();            
            var intFieldPosition = new Rect(controlPosition);
            float widthSeg = intFieldPosition.width / 3f;
            intFieldPosition.width = widthSeg;

            long newNumericValue = EditorGUI.LongField(intFieldPosition, property.longValue);

            if(EditorGUI.EndChangeCheck())
            {
                property.longValue = newNumericValue;
            }

            if (isEnum)
            {
                bool hasUnknown = false;
                int selectedIndex = options.FindIndex(v => v.Key == property.longValue);
                if(selectedIndex == -1)
                {
                    options.Insert(0, new KeyValuePair<long, string>(-1, "<UNKNOWN>"));
                    hasUnknown = true;
                    selectedIndex = 0;
                }
                string[] popupOptions = options.Select(o => o.Value).ToArray();

                EditorGUI.BeginChangeCheck();
                var popupFieldPosition = new Rect(controlPosition);
                popupFieldPosition.xMin += widthSeg;                
                int newSelectedIndex = EditorGUI.Popup(popupFieldPosition, selectedIndex, popupOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    if(!(hasUnknown && newSelectedIndex == 0)) //ignore selection of "unknown"
                    {
                        property.longValue = options[newSelectedIndex].Key;
                    }
                }
            }

            EditorGUI.EndProperty();

        }

    }
}