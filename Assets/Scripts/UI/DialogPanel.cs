using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UI;
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
[Serializable]
public struct EmojisStructure
{
    public Emojis emoji;
    public string emojiNameInSpriteEditor;
    public Color textColor;
}

namespace _Scripts.UI
{
    
    public class DialogPanel : NetworkBehaviour
    {
        private static DialogPanel _instance;
        public static DialogPanel Instance => _instance ? _instance : FindFirstObjectByType<DialogPanel>();
        public TextScaler textWriterSingle;
        [SerializeField]private GameObject orderTextGameObject;
        [SerializeField] private Image characterImage;
        [SerializeField] private List<CharacterTypeAndImages> characterTypesAndEmotions;
       
        public List<DialogData> currentDialog;
        private int _index = 0;
        public BoolNetworkValue IsPlaying => textWriterSingle.isWriting;
        private CharacterTypeFilter _characterThatIsTalking;

        public List<EmojisStructure> emojisStructure = new List<EmojisStructure>();
        
        #region MonoBehaviour

        protected void OnEnable()
        {
            WithValues(IsPlaying);
            
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(OnClickDialog);
        }
        protected void OnDisable()
        {
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(OnClickDialog);
        }
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
        #endregion
        

        public void PlayDialog(List<DialogData> dialogData)
        {
            currentDialog = dialogData;
            TypeWriteText(currentDialog[0]);
        }

        public bool OnClickDialog(PlayerInteractPacket playerInteractPacket, int i)
        {
            var player = GetNetworkObject(playerInteractPacket.Id);
            
            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;

            if (_characterThatIsTalking == CharacterTypeFilter.Both)
            {
                // logic for the two players select   
            }
            else
            {
                if (!_characterThatIsTalking.Filter(controller.CharacterType))
                    return false;
            }
            
            
            if (textWriterSingle == null) return false;

            if (textWriterSingle.isWriting.Value)
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

            return true;
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
            textWriterSingle.SetText(dialogData.GetFormattedString());
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