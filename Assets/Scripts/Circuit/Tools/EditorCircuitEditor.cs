#if UNITY_EDITOR
using System;
using System.Linq;
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
            CreatingConnection,
            MovingControlPoint,
            EditingConnection
        }
        
        private static bool _isOnCircuitMode = true;
        public static Action action = Action.Idle;
        
        public CircuitPlug currentPlug;
        public CircuitConnection currentConnection;
        public int currentControlPointIndex = -1;

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
                    if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con2))
                    {
                        var cps = con2.GetControlPoints().ToArray();
                        for(var i = 1; i < cps.Length; i++)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(cps[i - 1], cps[i]);
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
                        else
                        {
                            if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con))
                            {
                                Selection.activeGameObject = _mouseOverObject;
                                action = Action.EditingConnection;
                                currentControlPointIndex = -1;
                                currentConnection = con;
                                Event.current.Use();
                            }
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
            
                case Action.EditingConnection:
                    if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con3))
                    {
                        var cps = con3.GetControlPoints().ToArray();
                        for(var i = 1; i < cps.Length; i++)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(cps[i - 1], cps[i]);
                            }
                        }
                    }

                    if (currentConnection == null || (Event.current.type == EventType.MouseDown && Event.current.button == 0 && _mouseOverObject != currentConnection.gameObject))
                    {
                        if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con))
                        {
                            Selection.activeGameObject = _mouseOverObject;
                            action = Action.EditingConnection;
                            currentControlPointIndex = -1;
                            currentConnection = con;
                            Event.current.Use();
                        }
                        else
                            action = Action.Idle;
                        break;
                    }

                    var controlPoints = currentConnection.GetControlPoints().ToArray();
                    var mousePos = RaycastPosition();

                    var segmentIndex = 0;
                    var closest = controlPoints[0];
                    var closestDistance = float.MaxValue;
                    var closestControlPoint = -1;
                
                    for(var i = 0; i < controlPoints.Length; i++)
                    {
                        if(i > 0)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(controlPoints[i - 1], controlPoints[i]);
                            }

                            var p = ClosestPointOnLineSegment(controlPoints[i - 1], controlPoints[i], mousePos); 
                            var d = Vector3.Distance(p, mousePos);
                            if (d < closestDistance)
                            {
                                closest = p;
                                closestDistance = d;
                                segmentIndex = i - 1;
                            }
                        }

                        if (HandleUtility.DistanceToCircle(controlPoints[i], 0.15f) < 0.25f)
                        {
                            closestControlPoint = i;

                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.red;
                                Handles.SphereHandleCap(0, controlPoints[i], Quaternion.identity, 0.15f, EventType.Repaint);
                                Handles.color = Color.yellow;
                            }
                        }
                        else if (Event.current.type == EventType.Repaint)
                        {
                            if(currentControlPointIndex == i)
                                Handles.color = Color.green;
                            else
                                Handles.color = Color.yellow;

                            Handles.SphereHandleCap(0, controlPoints[i], Quaternion.identity, 0.15f, EventType.Repaint);
                        }

                    }

                    if(closestControlPoint == -1)
                    {
                        Handles.color = Color.white;
                        if (Event.current.type == EventType.Repaint)
                            Handles.SphereHandleCap(0, closest, Quaternion.identity, 0.25f, EventType.Repaint);
                    
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            //add undo
                            var so = new SerializedObject(currentConnection);
                            so.Update();
                            Undo.RecordObject(currentConnection, "Add Control Point");
                            var point = new CircuitConnection.ControlPoint{ position = closest };
                            currentConnection.controlPoints.Insert(segmentIndex, point);
                            //action = Action.MovingControlPoint;
                            //currentControlPointIndex = segmentIndex + 1;
                            so.ApplyModifiedProperties();
                            Event.current.Use();
                        }
                    }
                    else
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            currentControlPointIndex = closestControlPoint;
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                            var so = new SerializedObject(currentConnection);
                            so.Update();
                            Undo.RecordObject(currentConnection, "Move Control Point");
                            action = Action.MovingControlPoint;
                            currentControlPointIndex = closestControlPoint;
                            so.ApplyModifiedProperties();
                            Event.current.Use();
                        }
                    }

                    if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Backspace && currentControlPointIndex != -1)
                    {
                        var so = new SerializedObject(currentConnection);
                        so.Update();
                        Undo.RecordObject(currentConnection, "Delete Control Point");
                        currentConnection.controlPoints.RemoveAt(currentControlPointIndex - 1);
                        currentControlPointIndex = -1;
                        so.ApplyModifiedProperties();
                        Event.current.Use();
                        currentConnection.UpdateVisual();
                    }

                    break;

                case Action.MovingControlPoint:
                    if (currentConnection == null)
                        action = Action.Idle;
                    else
                    {
                        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                            var mousePos2 = RaycastPosition();
                            currentConnection.controlPoints[currentControlPointIndex - 1] = new CircuitConnection.ControlPoint{ position = mousePos2 };
                            currentConnection.UpdateVisual();
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            action = Action.EditingConnection;
                        }
                    }
                    break;
            }
            
            sceneView.Repaint();
        }

        private Vector3 RaycastPosition()
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out var hit))
                return hit.point;
            return ray.origin + ray.direction * 10;
        }

        private Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            var ap = p - a;
            var ab = b - a;
            var magnitude = ab.sqrMagnitude;
            var abap = Vector3.Dot(ap, ab);
            var t = abap / magnitude;
            if (t < 0)
                return a;
            if (t > 1)
                return b;
            return a + ab * t;
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