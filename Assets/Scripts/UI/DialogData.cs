using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Scripts.UI;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace UI
{
    public enum Emojis
    {
        Lever,
        Button,
        PressurePlate,
        ZipLine,
        RightMouseBtn,
        LeftMouseButton,
        W,
        AKey,
        S,
        D,
        Enter,
        Space,
        Shift,
        Y,
        B,
        X,
        A,
        LJoyStick,
        RJoyStick,
        PalmScanner,
        RobotScanner,
        Plug1, 
        Plug2,
        Socket1,
        Socket2,
        WindTurbine,
        Container,
        Box,
        Frog,
        Nina,
        Ram,
        Fan,
        Stop,
        Alert,
        E
    }
    
    [Serializable]
    public struct CharacterAndEmotion
    {
        public CharacterTypeEmote characterType;
        public Emotion emotion;
    }
    
    [CreateAssetMenu(fileName = "Dialog", menuName = "Solis/Game/Dialog")]
    public class DialogData : ScriptableObject
    {
        public CharacterAndEmotion characterType;
        
        private TextMeshProUGUI _textField;
        [TextArea]
        public string textValue;
        public Emojis[] emojis;
        private List<string> _instancedValues;

        public string GetFormattedString()
        {
            _instancedValues = new List<string>();

            for (int i = 0; i < emojis.Length; i++)
            {
                EmojisStructure emojisStructure = DialogPanel.Instance.emojisStructure.First(c => c.emoji == emojis[i]);
                var field = emojis[i].ToString();
                string value = $"<sprite name=\"{emojisStructure.emojiNameInSpriteEditor}\"> <color=#{emojisStructure.textColor.ToHexString()}>{field}</color>";
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
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DialogData), true)]
public class DialogDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, property, label);
        
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.objectReferenceValue != null)
        {
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Salta o campo "m_Script"
            while (iterator.NextVisible(false))
            {
                position.height = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(position, iterator, true);
                position.y += position.height + spacing;
            }
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; 

        if (property.objectReferenceValue != null)
        {
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            float spacing = EditorGUIUtility.standardVerticalSpacing;

            while (iterator.NextVisible(false))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + spacing;
            }
        }

        return height;
    }
}

#endif
