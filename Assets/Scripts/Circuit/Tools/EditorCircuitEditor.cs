#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SolarBuff.Circuit.Tools
{
    public class EditorCircuitEditor : EditorWindow
    {
        public const string PREFABS_PATH = "Assets/Prefabs/Circuit";
        public const int PREFAB_SIZE = 80;
        
        public enum Action
        {
            Idle,
            CreatingConnection
        }
        
        private static bool _isOnCircuitMode = true;
        public static Action action = Action.Idle;
        
        public CircuitPlug currentPlug;
        private GameObject _mouseOverObject = null;
        private Vector2 _scrollPos;
      
        
        [MenuItem ("Solar Buff/Circuit Editor")]
        public static void ShowWindow () 
        {
            var inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            GetWindow<EditorCircuitEditor>("Circuit Editor", inspectorType);
        }
        
        private void OnGUI()
        {

            #region Prefabs
            var prefabFiles = AssetDatabase.FindAssets("t:Prefab", new[] {PREFABS_PATH});
            
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(position.width));
            EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            var size = 5;
            
            var width = (position.width - 5);
            var columnCount = Mathf.Max(1, (int) (width / (PREFAB_SIZE + 5)));
            
            var n = 0;
            while (n < prefabFiles.Length)
            {
                EditorGUILayout.BeginHorizontal();
                for (var i = 0; i < columnCount; i++)
                {
                    if (n >= prefabFiles.Length)
                        break;
                    
                    var path = AssetDatabase.GUIDToAssetPath(prefabFiles[n]);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var preview = AssetPreview.GetAssetPreview(asset);
                    
                    //begin vertical with the image and name bellow
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(preview, GUILayout.Width(PREFAB_SIZE), GUILayout.Height(PREFAB_SIZE));
                    GUILayout.Label(asset.name, GUILayout.Width(PREFAB_SIZE));
                    EditorGUILayout.EndVertical();
                    
                    if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new Object[] {asset};
                        DragAndDrop.StartDrag("Prefab");
                        Event.current.Use();
                    }

                    n++;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            #endregion
        }

        
        void OnFocus()
        {
#pragma warning disable CS0618
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#pragma warning restore CS0618

            _isOnCircuitMode = true;
        }
        
        void OnDestroy()
        {
#pragma warning disable CS0618
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#pragma warning restore CS0618
            
            _isOnCircuitMode = false;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!_isOnCircuitMode)
                return;
            
            CircuitPlug mouseOverPlug = null;
            
            foreach (var plug in FindObjectsByType<CircuitPlug>(FindObjectsSortMode.None))
            {
                var pos = plug.transform.position;
                
                Handles.color = plug.type == CircuitPlug.Type.Input ? Color.green : Color.red;
               
                if (HandleUtility.DistanceToCircle(pos, 0.15f) < 0.25f)
                {
                    Handles.color = Color.yellow;
                    mouseOverPlug = plug;
                }

                if (Event.current.type == EventType.Repaint)
                    Handles.SphereHandleCap(0, plug.transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
            }
            
            if (Event.current.type is EventType.MouseMove or EventType.MouseDown or EventType.MouseDrag
                or EventType.MouseUp)
            {
                _mouseOverObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            }
            
            switch (action)
            {
                case Action.Idle:
                    if (mouseOverPlug == null)
                    {
                        if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con))
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                var controlPoints = con.GetControlPoints();
                                
                                for(var i = 0; i < controlPoints.Length; i++)
                                {
                                    Handles.color = Color.yellow;
                                    if(i > 0)
                                        Handles.DrawLine(controlPoints[i - 1], controlPoints[i]);
                                }
                            }
                        }
                    }
                    
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (mouseOverPlug != null)
                        {
                            currentPlug = mouseOverPlug;
                            
                            if(currentPlug.Connection != null)
                                DestroyImmediate(currentPlug.Connection.gameObject);
                            
                            action = Action.CreatingConnection;
                            Event.current.Use();
                        }
                    }

                    break;
                
                case Action.CreatingConnection:
                    if (currentPlug == null)
                        action = Action.Idle;
                    else switch (Event.current.type)
                    {
                        case EventType.Repaint:
                        {
                            if (mouseOverPlug != null && mouseOverPlug.type != currentPlug.type)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(currentPlug.transform.position, mouseOverPlug.transform.position);
                            }
                            else
                            {
                                Handles.color = Color.white;
                                Handles.DrawLine(currentPlug.transform.position, HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin);
                            }

                            break;
                        }
                        case EventType.MouseDown when Event.current.button == 0:
                        {
                            if (mouseOverPlug != null && mouseOverPlug.type != currentPlug.type)
                            {
                                CreateConnection(currentPlug, mouseOverPlug);
                            }
                        
                            action = Action.Idle;
                            Event.current.Use();
                            break;
                        }
                    }
                    break;
            }
            
            sceneView.Repaint();
        }


        public CircuitConnection CreateConnection(CircuitPlug a, CircuitPlug b)
        {
            var go = new GameObject("Connection");
            go.SetActive(false);
            var con = go.AddComponent<CircuitConnection>();
            con.a = a;
            con.b = b;
            go.transform.parent = a.transform;
            go.SetActive(true);
            return con;
        }
    }
}
#endif