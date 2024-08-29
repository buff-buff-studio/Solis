using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.UI;
using DefaultNamespace;
using Solis.Data;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public enum Emotion
{
    Happy,
    Neutral,
    Confused
}
public enum CharacterTypeEmote
{
    Human,
    Robot,
    Frog,
    None
}
[Serializable]
public struct EmotionsAndImages
{
    public Emotion emotion;
    public Sprite image;
}
[Serializable]
public struct CharacterTypeAndImages
{
    public CharacterTypeEmote characterType;
    public List<EmotionsAndImages> emotesAndImages;
}

namespace _Scripts.UI
{
    public class DialogPanel : MonoBehaviour
    {
        private static DialogPanel _instance;
        public static DialogPanel Instance => _instance ? _instance : FindFirstObjectByType<DialogPanel>();
        public TextScaler textWriterSingle;
        [SerializeField]private GameObject orderTextGameObject;
        [SerializeField] private float timePerCharacter = 0.1f;
        [SerializeField] private Image characterImage;
        [SerializeField] private List<CharacterTypeAndImages> characterTypesAndEmotions;
       
        public List<DialogData> currentDialog;
        private int _index = 0;
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.P))
                TypeWriteText(currentDialog[0]);
            
            
        }

        public void OnClickDialog()
        {
            if (textWriterSingle == null) return;

            if (textWriterSingle.isWriting)
            {
                // textWriterSingle.WriteAll();
            } 
            else
            {
                if (_index >= currentDialog.Count-1)
                    ClosePanel();
                else
                {
                    _index++;
                    TypeWriteText(currentDialog[_index]);
                }
            }
        }

        private void ClosePanel()
        {
            _index = 0;
            orderTextGameObject.SetActive(false);
        }

        private void TypeWriteText(DialogData dialogData)
        {
            characterImage.gameObject.SetActive(false);
            EnterImage(dialogData.characterType);
            textWriterSingle.SetText(dialogData.text);
            orderTextGameObject.SetActive(true);
        }

        private void EnterImage(CharacterAndEmotion characterType)
        {
            characterImage.gameObject.SetActive(true);
            var choosed = characterTypesAndEmotions.FirstOrDefault(c => c.characterType == characterType.characterType);
            var sprite = choosed.emotesAndImages.FirstOrDefault(c => c.emotion == characterType.emotion).image;
            characterImage.sprite = sprite;
        }
    }
}