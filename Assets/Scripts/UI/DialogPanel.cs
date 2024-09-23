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
    public string emojiNameDisplay;
    public string emojiNameInSpriteEditor;
    public Color textColor;
}

namespace _Scripts.UI
{
    
    public class DialogPanel : NetworkBehaviour
    {
        private static DialogPanel _instance;
        public static DialogPanel Instance => _instance ? _instance : FindFirstObjectByType<DialogPanel>();

        public static bool IsDialogPlaying;

        public TextScaler textWriterSingle;
        [SerializeField]private GameObject orderTextGameObject;
        [SerializeField] private Image characterImage;
        [SerializeField] private List<CharacterTypeAndImages> characterTypesAndEmotions;

        public NetworkBehaviourNetworkValue<DialogPlayerBase> currentDialog = new(); 
        public IntNetworkValue index;
        private CharacterTypeEmote _characterThatIsTalking;

        public List<EmojisStructure> emojisStructure = new List<EmojisStructure>();

        public IntNetworkValue charactersReady;
        [SerializeField]private List<int> hasSkipped = new List<int>();
        [SerializeField] private GameObject nextImage;
        [SerializeField] private TextMeshProUGUI playersText;
        
        
        #region MonoBehaviour

        protected void OnEnable()
        {
            WithValues(charactersReady,index, currentDialog);
            
            PacketListener.GetPacketListener<PlayerInputPackage>().AddServerListener(OnClickDialog);
            index.OnValueChanged += UpdateDialog;
            charactersReady.OnValueChanged += UpdateText;

        }
        protected void OnDisable()
        {
            PacketListener.GetPacketListener<PlayerInputPackage>().RemoveServerListener(OnClickDialog);
            index.OnValueChanged -= UpdateDialog;
            charactersReady.OnValueChanged -= UpdateText;
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
        

        public void PlayDialog(DialogPlayerBase dialogData)
        {
            currentDialog.Value = dialogData;
            index.Value = 0;
        }

        public bool OnClickDialog(PlayerInputPackage playerInputPackage, int i)
        {
            if(playerInputPackage.Key != KeyCode.Return) return false;
            if(hasSkipped.Contains(i)) return false;
            if(textWriterSingle.isWriting) return false;
            if(!orderTextGameObject.activeSelf) return false; 
            var player = GetNetworkObject(playerInputPackage.Id);
            
            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;
           
            if (index.Value != -1)
            {
                hasSkipped.Add(i);
                charactersReady.Value++;
                playersText.text = charactersReady.Value + "/2";
#if UNITY_EDITOR
                if (hasSkipped.Count == 0) return true;
#else
                if (hasSkipped.Count != 2) return true;
#endif
            }
            
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
        private void UpdateText(int oldvalue, int newvalue)
        {
            playersText.text = charactersReady.Value + "/2";
        }

        public void UpdateDialog(int oldValue, int newValue)
        {
            if (newValue == -1) ClosePanel();
            else
            {
                IsDialogPlaying = true;
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
         
            IsDialogPlaying = false;
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