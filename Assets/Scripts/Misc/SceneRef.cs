using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Solis.Misc
{
    /// <summary>
    /// Used to reference a scene directly in the editor.
    /// </summary>
    [Serializable]
    public class SceneRef : ISerializationCallbackReceiver
    {
        #region Public Fields
        #if UNITY_EDITOR
        [SerializeField]
        private SceneAsset sceneAsset;
        #endif
        
        [SerializeField]
        // ReSharper disable once NotAccessedField.Local
        private int buildIndex = -1;
        
        [SerializeField]
        public string sceneName = string.Empty;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the name of the scene.
        /// </summary>
        public string Name
        {
            get
            {
                #if UNITY_EDITOR
                _Refresh();
                #endif
                return sceneName;
            }
        }
        #endregion
        
        #region Private Methods
        #if UNITY_EDITOR
        private void _Refresh()
        {
            if (sceneAsset == null)
            {
                sceneName = string.Empty;
                buildIndex = -1;
                return;
            }
            
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = sceneAsset.name;
            buildIndex = SceneUtility.GetBuildIndexByScenePath(path);
        }
        #endif
        #endregion
        
        #region ISerializationCallbackReceiver Methods
        public void OnBeforeSerialize()
        {
            #if UNITY_EDITOR
            _Refresh();
            #endif
        }

        public void OnAfterDeserialize()
        {
            
        }
        #endregion
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneAsset = property.FindPropertyRelative("sceneAsset");
            
            position.height = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(position, label, sceneAsset))
            {
                EditorGUI.PropertyField(position, sceneAsset, label);
            }
        }
    }
    #endif
}