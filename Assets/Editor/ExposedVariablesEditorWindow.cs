using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace ToolsStudy.Editor
{
    public struct ExposedFieldInfo
    {
        public MemberInfo memberInfo;
        public ExposedFieldAttribute exposedFieldAttribute;

        public ExposedFieldInfo(MemberInfo info, ExposedFieldAttribute attribute)
        {
            memberInfo = info;
            exposedFieldAttribute = attribute;
        }
    }
    
    public class ExposedVariablesEditorWindow : EditorWindow
    {
        List<ExposedFieldInfo> exposedMembers = new List<ExposedFieldInfo>();
     
        [MenuItem("Examples/My Editor Window")]
        public static void Open()
        {
            ExposedVariablesEditorWindow window = CreateWindow<ExposedVariablesEditorWindow>();
        }

        private void OnEnable()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            exposedMembers.Clear();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ;

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
                            var attributes = member.GetCustomAttribute<ExposedFieldAttribute>();

                            if (attributes != null)
                            {
                                exposedMembers.Add(new ExposedFieldInfo(member, attributes));
                            }
                        }
                    }
                    
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Exposed Properties", EditorStyles.boldLabel);

            foreach (var member in exposedMembers)
            {

                var memberInfo = member.memberInfo;
                var attribute = member.exposedFieldAttribute;
              
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    object obj = null;
                    string value = "N/A";
                    
                   // if(GameManager.instance) obj = fieldInfo.GetValue(GameManager.instance.test);
                    if (obj != null)
                    {
                        value = obj.ToString();
                    }
                    
                    EditorGUILayout.LabelField($"{attribute.DisplayName} - {value}");
                }
                else
                {
                    EditorGUILayout.LabelField($"{member.exposedFieldAttribute.DisplayName}");
                }
                
            }
        }
    }
}