using System.Collections;
using System.Collections.Generic;
using Solis.Audio;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    public void Play(string audio)
    {
        Debug.Log("PlayAudio");
        AudioSystem.Instance.PlayVfx(audio);
    }
}
