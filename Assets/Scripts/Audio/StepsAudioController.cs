using System;
using UnityEngine;

namespace Solis.Audio
{
    public class StepsAudioController : MonoBehaviour
    {
        public void PlayStepSound()
        {
            if (Physics.Raycast(transform.parent.position, Vector3.down, out RaycastHit hitInfo, 0.1f))
            {
                var layer = hitInfo.transform.tag;

                var audioToPlay = layer switch
                {
                    "GroundLayer" => "GroundLayer",
                    "GrassLayer" => "GrassLayer",
                    _ => "GrassLayer"
                };
                //AudioSystem.Instance.PlayCharacter(audioToPlay);
            }
        }
    }
}