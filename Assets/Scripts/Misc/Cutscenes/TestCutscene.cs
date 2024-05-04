using UnityEngine;

namespace Solis.Misc.Cutscenes
{
    /// <summary>
    /// Just a test cutscene to show how to mark the cutscene as finished.
    /// </summary>
    public class TestCutscene : MonoBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public Animator animator;
        public CutsceneManager manager;
        
        #endregion

        private void Update()
        {
            var hasEnded = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0);
            
            if (hasEnded)
            {
                manager.MarkFinished();
                enabled = false;
            }
        }
    }
}