
using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Helpers;
using _Scripts.UI;
using Solis.Data;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
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
        public TextWriterSingle textWriterSingle;
        [SerializeField]private GameObject orderTextGameObject;
        [SerializeField] private TextMeshProUGUI orderText;
        [SerializeField] private float timePerCharacter = 0.1f;
        [SerializeField] private Image characterImage;
        [SerializeField] private List<CharacterTypeAndImages> characterTypesAndEmotions;
       
        public List<DialogData> dialogs;
     
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
                TypeWriteText(dialogs[0], null);
        }


        public void OnClickDialog()
        {
            if (textWriterSingle != null && textWriterSingle.IsActive())
                textWriterSingle.WriteAllAndDestroy();
        }
        
        private void TypeWriteText(DialogData dialogData, Action onFinishWriting = null)
        {
            characterImage.gameObject.SetActive(false);
            orderTextGameObject.SetActive(true);
            orderText.text = "";
            textWriterSingle = WriterText.Instance.AddWriter(orderText, dialogData.text, timePerCharacter,true, true);
            EnterImage(dialogData.characterType);
            
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