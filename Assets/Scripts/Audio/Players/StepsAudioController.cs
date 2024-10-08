using UnityEngine;

namespace Solis.Audio.Players
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
                
                //Debug.Log("step");
                // AudioSystem.Instance.PlayVfx(audioToPlay).At(hitInfo.transform.position);
            }
        }
    }
}