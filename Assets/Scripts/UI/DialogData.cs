using System;
using UI;
using UnityEditor;
using UnityEngine;

namespace UI
{
    [Serializable]
    public struct CharacterAndEmotion
    {
        public CharacterTypeEmote characterType;
        public Emotion emotion;
    }
    
    [CreateAssetMenu(fileName = "Dialog", menuName = "Solis/Game/Dialog")]
    public class DialogData : ScriptableObject
    {
        [TextArea]
        public string text;
        public CharacterAndEmotion characterType;
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
