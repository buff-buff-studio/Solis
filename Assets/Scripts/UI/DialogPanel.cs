using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Packets;
using Solis.Player;
using TMPro;
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
    Human = 0,
    Robot = 1,
    Frog = 2,
    None = 3
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

        public NetworkBehaviourNetworkValue<DialogPlayer> currentDialog = new(); 
        public IntNetworkValue index;
        private CharacterTypeEmote _characterThatIsTalking;

        public List<EmojisStructure> emojisStructure = new List<EmojisStructure>();
   
        [SerializeField]private PauseManager pauseManager;
        public IntNetworkValue charactersReady;
        [SerializeField]private List<int> hasSkipped = new List<int>();
        [SerializeField] private GameObject nextImage;
        [SerializeField] private TextMeshProUGUI playersText;
        
        
        #region MonoBehaviour

        protected void OnEnable()
        {
            WithValues(charactersReady,index, currentDialog);
            
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(OnClickDialog);
            index.OnValueChanged += UpdateDialog;
                
        }
        protected void OnDisable()
        {
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(OnClickDialog);
            index.OnValueChanged -= UpdateDialog;
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
        
        #endregion
        

        public void PlayDialog(DialogPlayer dialogData)
        {
            currentDialog.Value = dialogData;
            index.Value = 0;
        }

        public bool OnClickDialog(PlayerInteractPacket playerInteractPacket, int i)
        {
            if(hasSkipped.Contains(i)) return false;
            if(textWriterSingle.isWriting) return false;
            var player = GetNetworkObject(playerInteractPacket.Id);
            
            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;
           
            if (index.Value != -1)
            {
                hasSkipped.Add(i);
                charactersReady.Value++;
                playersText.text = charactersReady.Value + "/2";
                if (hasSkipped.Count != 2) return true;
            }
            
            /*else
            {
                Debug.Log("B");
                Debug.Log(_characterThatIsTalking.ToString());
                Debug.Log(controller.CharacterType.ToString());
                if ((int)_characterThatIsTalking != (int)controller.CharacterType)
                {
                    Debug.Log("NO!");
                    return true;
                }
            }*/
            
            if(currentDialog == null) return false;
            Debug.Log("C");
            if (index.Value + 1 > currentDialog.Value.currentDialog.dialogs.Count - 1) 
                index.Value = -1;
            else
                index.Value++;

            hasSkipped.Clear();
            charactersReady.Value = 0;
            return true;
        }

        public void UpdateDialog(int oldValue, int newValue)
        {
            if (newValue == -1) ClosePanel();
            else
            {
                playersText.text = charactersReady.Value + "/2";
                pauseManager.Pause();
                _characterThatIsTalking = currentDialog.Value.currentDialog.dialogs[index.Value].characterType.characterType;
                TypeWriteText(currentDialog.Value.currentDialog.dialogs[index.Value], () => nextImage.SetActive(true));
            }
        }
        
        private void ClosePanel()
        {
            playersText.text = "0/2";
            if (IsServer)
            {
                charactersReady.Value = 0;
                hasSkipped.Clear();
            }
         
            pauseManager.Resume();
            orderTextGameObject.SetActive(false);
            nextImage.SetActive(false);
        }

        private void TypeWriteText(DialogStruct dialogData, Action callback)
        {
            characterImage.gameObject.SetActive(false);
            EnterImage(dialogData.characterType);
            textWriterSingle.SetText(dialogData.GetFormattedString(), callback);
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