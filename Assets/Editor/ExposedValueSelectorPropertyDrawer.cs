using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ToolsStudy.Editor
{
    [CustomPropertyDrawer(typeof(ExposedValueSelector))]
    public class ExposedValueSelectorPropertyDrawer : PropertyDrawer
    {
        private List<FieldInfo> _fields;
        private List<string> _fieldNames = new List<string>();
        private bool gotFields;
        private int index;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!gotFields) GetFields();

            var fieldNameProperty = property.FindPropertyRelative("fieldName");

            index = GetFieldName(fieldNameProperty.stringValue);
            index = EditorGUI.Popup(position, index, _fieldNames.ToArray());
            fieldNameProperty.stringValue = _fields[index].Name;
        }

        private void GetFields()
        {
            _fields = new List<FieldInfo>();
            _fieldNames = new List<string>();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var t in types)
                {
                    MemberInfo[] members = t.GetMembers(flags);

                    foreach (MemberInfo member in members)
                    {
                        if (member.CustomAttributes.ToArray().Length > 0)
                        {
                            var attribute = member.GetCustomAttribute<ExposedFieldAttribute>();

                            if (attribute != null)
                            {
                                _fields.Add((FieldInfo)member);
                                _fieldNames.Add($"{member.ReflectedType}/{attribute.DisplayName}");
                            }
                        }
                    }

                }
            }

            gotFields = true;
        }

        private int GetFieldName(string value)
        {
            string fieldName = value;
            int count = 0;

            foreach (var member in _fields)
            {
                if (member.Name == fieldName)
                {
                    return count;
                }

                count++;
            }

            return 0;
        }
    }
}