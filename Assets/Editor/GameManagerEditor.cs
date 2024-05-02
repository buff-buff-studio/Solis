
using Solis.Core;
using UnityEngine;
#if UNITY_EDITOR
using NetBuff;
using UnityEditor;
#endif
namespace Editor
{
    //GameManagerEditor
    #if UNITY_EDITOR
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var gameManager = (GameManager)target;

            if (NetworkManager.Instance == null)
                return;
            
            if (!NetworkManager.Instance.IsServerRunning)
                return;
            
            if (gameManager.IsOnLobby)
            {
                if (GUILayout.Button("Start Game"))
                {
                    gameManager.StartGame();
                }
            }
            else
            {
                if (GUILayout.Button("Return to Lobby"))
                {
                    gameManager.ReturnToLobby();
                }
                
                if (GUILayout.Button("Reload Level"))
                {
                    gameManager.LoadLevel();
                }

                if (GUILayout.Button("Next Level"))
                {
                    gameManager.SaveData.currentLevel++;
                    gameManager.LoadLevel();
                }
                
                if (GUILayout.Button("Prev Level"))
                {
                    gameManager.SaveData.currentLevel--;
                    gameManager.LoadLevel();
                }
            }
        }
    }
    #endif
}