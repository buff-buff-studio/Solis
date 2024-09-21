﻿using System;
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
    private bool _foldout;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
        serializedObject.Update();
      // Desenha o campo 'characterType'
      
        // Desenha o campo de referência ao ScriptableObject
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, property, label);

        if (property.objectReferenceValue != null)
        {
            // Adiciona um espaço para o toggle logo abaixo da propriedade do ScriptableObject
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Desenha o toggle para expandir/recolher
            _foldout = EditorGUI.Foldout(position, _foldout, "Details", true);

            if (_foldout)
            {
           
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Desenha o campo de texto 'textValue'
                SerializedProperty textValueProperty = serializedObject.FindProperty("textValue");
                position.height = EditorGUI.GetPropertyHeight(textValueProperty);
                EditorGUI.PropertyField(position, textValueProperty, new GUIContent("Text Value"));
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                // Desenha o array de Enums 'Emojis'
                SerializedProperty emojisProperty = serializedObject.FindProperty("emojis");
                DrawEmojisArray(position, emojisProperty);

                SerializedProperty characterTypeProperty = serializedObject.FindProperty("characterType");
        
                if (characterTypeProperty != null)
                {
                    EditorGUILayout.PropertyField(characterTypeProperty, new GUIContent("Character Type"));
                }
                serializedObject.ApplyModifiedProperties();
                
            }
        }

        EditorGUI.EndProperty();
    }

    private void DrawEmojisArray(Rect position, SerializedProperty property)
    {
        if (property.isArray)
        {
            // Calcula a largura do campo de rótulo e campo de edição
            float labelWidth = 40f; // Reduz o tamanho do rótulo
            float fieldWidth = position.width - labelWidth;

            Rect labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(position.x + labelWidth, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);

            // Desenha o tamanho do array
            EditorGUI.LabelField(labelRect, "Size");
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("Array.size"), GUIContent.none);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Desenha cada elemento do array
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty emojiElement = property.GetArrayElementAtIndex(i);
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, emojiElement, new GUIContent($"Emoji {i}"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.objectReferenceValue != null)
        {
            // Adiciona espaço para o toggle
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (_foldout)
            {
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                SerializedProperty textValueProperty = serializedObject.FindProperty("textValue");
                SerializedProperty emojisProperty = serializedObject.FindProperty("emojis");

                height += EditorGUI.GetPropertyHeight(textValueProperty) + EditorGUIUtility.standardVerticalSpacing;

                if (emojisProperty.isArray)
                {
                    height += EditorGUIUtility.singleLineHeight * (emojisProperty.arraySize + 1) + EditorGUIUtility.standardVerticalSpacing * (emojisProperty.arraySize + 1);
                }
            }
        }

        return height;
    }
}
#endif