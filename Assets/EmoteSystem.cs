using System.Collections;
using System.Collections.Generic;
using NetBuff.Components;
using SolarBuff.Player;
using UnityEngine;
using UnityEngine.UI;

public class EmoteSystem : NetworkBehaviour
{
    public Image emoteImage;
    public GameObject emoteObject;
    public float timeToHide = 2;

    [System.Serializable]
    public struct Emote {
        public string name;
        public Sprite humanEmote;
        public Sprite robotEmote;
        public bool canUseInGameplay;

        public Sprite this[PlayerControllerCore.PlayerType t] => t switch {
            PlayerControllerCore.PlayerType.Human => humanEmote != null ? humanEmote : throw new System.ArgumentException($"O Humano não contem o emote: \"{name}\"", nameof(t)),
            PlayerControllerCore.PlayerType.Robot => robotEmote!= null ? robotEmote : throw new System.ArgumentException($"O Robo não contem o emote: \"{name}\"", nameof(t)),
            _ => throw new System.ArgumentException("Player Type Invalido", nameof(t))
        };
    }

    public List<string> emoteSlots = new List<string>();
    public List<Emote> emotes = new List<Emote>();

    public void Start() {
        emoteObject.SetActive(false);
    }

    public void ShowEmote(int emoteIndex, PlayerControllerCore.PlayerType playerType)
    {
        if (!HasAuthority) return;
        
        ShowEmote(emoteSlots[emoteIndex], playerType);
    }

    public void ShowEmote(string emoteName, PlayerControllerCore.PlayerType playerType) {
        if(emoteObject.activeSelf) return;

        Emote emote = emotes.Find(e => e.name == emoteName);
        if (emote.canUseInGameplay) {
            emoteObject.SetActive(true);
            emoteImage.sprite = emote[playerType];
            Invoke(nameof(HideEmote), timeToHide);
        }
    }

    public void HideEmote() {
        emoteObject.SetActive(false);
    }
}
