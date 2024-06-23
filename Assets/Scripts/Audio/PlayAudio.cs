using System.Collections;
using System.Collections.Generic;
using Solis.Audio;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    [SerializeField] private string _audioName;
    
    public void Play(string audio)
    {
        Debug.Log("PlayAudio");
        AudioSystem.Instance.PlayVfx(audio);
    }
    
    public void PlayAudioName()
    {
        AudioSystem.PlayVfxStatic(_audioName);
    }
}
