using System;
using UnityEngine;

namespace Solis.Audio
{
    public class StepsAudioController : MonoBehaviour
    {
        
        
        public void PlayStepSound()
        {
            if (Physics.Raycast(transform.parent.position, Vector3.down, out RaycastHit hitInfo, 1000))
            {
                var layer = hitInfo.transform.tag;

                var audioToPlay = layer switch
                {
                    "GroundLayer" => "GroundLayer",
                    "GrassLayer" => "GrassLayer",
                    _ => "GrassLayer"
                };
                AudioSystem.Instance.PlayVfx(audioToPlay);
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }
        }

        private void OnDrawGizmos()
        {
            Debug.DrawRay(transform.position, Vector3.down * 1000);
        }
    }
}