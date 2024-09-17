using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace ToolsStudy
{
   // [RequireComponent(typeof(TextMeshProUGUI))]
    public class ExposedValueText : MonoBehaviour
    {
        private TextMeshProUGUI _textField;
        [TextArea]
        public string textValue;
        public ExposedValueSelector[] exposedValues;
        private Dictionary<string, FieldInfo> _fieldNameDictionary = new Dictionary<string, FieldInfo>();
        private List<string> _instancedValues;
        
        private void Awake()  
        {
            _textField = GetComponent<TextMeshProUGUI>();
            CreateDictionary();
        }

        private void Start()
        {
            _textField.text = GetFormattedString();
        }

        private string GetFormattedString()
        {
            _instancedValues = new List<string>();

            for (int i = 0; i < exposedValues.Length; i++)
            {
                var field = _fieldNameDictionary[exposedValues[i].fieldName];
                string value = GetValue(field);
                
                _instancedValues.Add(value);
            }

            return string.Format(textValue, _instancedValues.ToArray());
        }

        private string GetValue(FieldInfo field)
        {
            object obj = null;
            string value = "N/A";
                    
           // if(GameManager.instance) obj = field.GetValue(GameManager.instance.test);
            if (obj != null)
            {
                value = obj.ToString();
            }

            return value;
        }

        private void CreateDictionary()
        {
            _fieldNameDictionary = new Dictionary<string, FieldInfo>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
          
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
                                _fieldNameDictionary.Add(member.Name,(FieldInfo)member);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}