
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
            
            GUILayout.Space(10);
            GUILayout.Label("SERVER CONTROLS", EditorStyles.boldLabel);
            
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
                    gameManager.SaveData.currentLevel = gameManager.FindActiveLevel(SolisNetworkManager.sceneToLoad);
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
            
            GUILayout.Space(10);
            GUILayout.Label("SAVE CONTROLS", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Save Data"))
            {
                gameManager.Save.SaveData(null);
            }
        }
    }
    #endif
}