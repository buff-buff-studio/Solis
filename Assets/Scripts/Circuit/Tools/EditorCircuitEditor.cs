#if UNITY_EDITOR
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SolarBuff.Circuit.Tools
{
    public class EditorCircuitEditor : EditorWindow
    {
        public const string PREFABS_PATH = "Assets/Prefabs/Circuit";
        public const int PREFAB_SIZE = 80;
        
        private enum Action
        {
            Idle,
            CreatingConnection,
            MovingControlPoint,
            EditingConnection
        }

        #region Internal
        private static bool _isOnCircuitMode = true;
        private static Action _action = Action.Idle;
        private CircuitPlug _currentPlug;
        private CircuitConnection _currentConnection;
        private int _currentControlPointIndex = -1;
        private GameObject _mouseOverObject = null;
        private Vector2 _scrollPos;
        #endregion

        [Header("Settings")] 
        public float gridUnit = 0.25f;


        [MenuItem ("Solar Buff/Circuit Editor")]
        public static void ShowWindow () 
        {
            var inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            GetWindow<EditorCircuitEditor>("Circuit Editor", inspectorType);
        }
        
        private void OnGUI()
        {
            //create margin to the left and right
            EditorGUILayout.BeginVertical(new GUIStyle {padding = new RectOffset(10, 10, 0, 0)});
            #region Fields
            var so = new SerializedObject(this);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("gridUnit"));
            so.ApplyModifiedProperties();
            #endregion

            #region Current Selection Options
            if (_action is Action.EditingConnection or Action.MovingControlPoint)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Connection Tool", EditorStyles.boldLabel);
                
                //Create info box showing the controls
                EditorGUILayout.EndVertical();

                if (_currentControlPointIndex > 0 && _currentConnection != null && _currentConnection.controlPoints.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Control Point", EditorStyles.boldLabel);
                    
                    var point = _currentConnection.controlPoints[_currentControlPointIndex - 1];
                    
                    var oldPos = point.position;
                    var oldLeft = point.leftHandle + point.position;
                    var oldRight = point.rightHandle + point.position;
                    
                    var newPos = EditorGUILayout.Vector3Field("Position", oldPos);
                    var newLeft = EditorGUILayout.Vector3Field("Left Handle", oldLeft);
                    var newRight = EditorGUILayout.Vector3Field("Right Handle", oldRight);

                    if (GUILayout.Button("Flat Handles"))
                    {
                        var points = _currentConnection.GetControlPoints();
                        var prev = points[_currentControlPointIndex - 1];
                        var next = points[_currentControlPointIndex + 1];
                        var dirLeft = (prev.position - point.position).normalized;
                        var dirRight = (next.position - point.position).normalized;
                        
                        var plane = -Vector3.Cross(dirLeft, dirRight).normalized;

                        newLeft = GetPointOnPlane(newLeft, point.position, plane);
                        newRight = GetPointOnPlane(newRight, point.position, plane);
                       
                        var so2 = new SerializedObject(_currentConnection);
                        so2.Update();
                        Undo.RecordObject(_currentConnection, "Flat Handles");
                        point.leftHandle = newLeft - point.position;
                        point.rightHandle = newRight - point.position;
                        so2.ApplyModifiedProperties();
                        _currentConnection.UpdateVisual(true);
                    }
                    else if (GUILayout.Button("Recalculate Handles"))
                    {
                        var points = _currentConnection.GetControlPoints();
                        var prev = points[_currentControlPointIndex - 1];
                        var next = points[_currentControlPointIndex + 1];
                        var dirLeft = (prev.position - point.position).normalized;
                        var dirRight = (next.position - point.position).normalized;
                        
                        newLeft = point.position + dirLeft;
                        newRight = point.position + dirRight;
                        
                        var so2 = new SerializedObject(_currentConnection);
                        so2.Update();
                        Undo.RecordObject(_currentConnection, "Rectangle Handles");
                        point.leftHandle = dirLeft;
                        point.rightHandle = dirRight;
                        so2.ApplyModifiedProperties();
                        _currentConnection.UpdateVisual(true);
                    }
                    else
                    {
                        if (newPos != oldPos)
                        {
                            var so2 = new SerializedObject(_currentConnection);
                            so2.Update();
                            Undo.RecordObject(_currentConnection, "Move Control Point");
                            point.position = Snap(newPos);
                            so2.ApplyModifiedProperties();
                            _currentConnection.UpdateVisual(true);
                        }
                        else if (newLeft != oldLeft)
                        {
                            var so2 = new SerializedObject(_currentConnection);
                            so2.Update();
                            Undo.RecordObject(_currentConnection, "Move Left Handle");
                            point.leftHandle = newLeft - point.position;
                            so2.ApplyModifiedProperties();
                            _currentConnection.UpdateVisual(true);
                        }
                        else if (newRight != oldRight)
                        {
                            var so2 = new SerializedObject(_currentConnection);
                            so2.Update();
                            Undo.RecordObject(_currentConnection, "Move Right Handle");
                            point.rightHandle = newRight - point.position;
                            so2.ApplyModifiedProperties();
                            _currentConnection.UpdateVisual(true);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.HelpBox("Hold Shift to show move handles\nPress F to focus on the selected point\nPress backspace to delete the selected control point\nHold Ctrl to snap to grid", MessageType.Info);
                }
            }
            #endregion
            
            EditorGUILayout.Space();
            
            #region Prefabs
            var prefabFiles = AssetDatabase.FindAssets("t:Prefab", new[] {PREFABS_PATH});
            EditorGUILayout.LabelField("Components (Drag n' drop)", EditorStyles.boldLabel);
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
            #endregion
            EditorGUILayout.EndVertical();
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
            
            switch (_action)
            {
                case Action.Idle:
                    if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con2))
                    {
                        var cps = con2.GetControlPoints();
                        for(var i = 1; i < cps.Length; i++)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.white;
                                Handles.DrawLine(cps[i - 1].position, cps[i].position);
                            }
                        }
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (mouseOverPlug != null)
                        {
                            _currentPlug = mouseOverPlug;

                            if (_currentPlug.Connection != null)
                            {
                                Undo.DestroyObjectImmediate(_currentPlug.Connection.gameObject);
                            }
                            
                            _action = Action.CreatingConnection;
                            Event.current.Use();
                        }
                        else
                        {
                            if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con))
                            {
                                Selection.activeGameObject = _mouseOverObject;
                                UnityEditor.Tools.current = Tool.None;
                                
                                _action = Action.EditingConnection;
                                _currentControlPointIndex = -1;
                                _currentConnection = con;
                                Event.current.Use();
                            }
                        }
                    }

                    break;
                
                case Action.CreatingConnection:
                    if (_currentPlug == null)
                        _action = Action.Idle;
                    else switch (Event.current.type)
                    {
                        case EventType.Repaint:
                        {
                            if (mouseOverPlug != null && mouseOverPlug.type != _currentPlug.type)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(_currentPlug.transform.position, mouseOverPlug.transform.position);
                            }
                            else
                            {
                                Handles.color = Color.white;
                                Handles.DrawLine(_currentPlug.transform.position, HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin);
                            }

                            break;
                        }
                        case EventType.MouseDown when Event.current.button == 0:
                        {
                            if (mouseOverPlug != null && mouseOverPlug.type != _currentPlug.type)
                            {
                                CreateConnection(_currentPlug, mouseOverPlug);
                            }
                        
                            _action = Action.Idle;
                            Event.current.Use();
                            break;
                        }
                    }
                    break;
            
                case Action.EditingConnection:
                    if (_currentConnection == null)
                    {
                        _action = Action.Idle;
                        break;
                    }
                    
                    #region Current Connection Handles
                    var controlPoints = _currentConnection.GetControlPoints();
                    
                    if (_currentControlPointIndex <= 0)
                        _currentControlPointIndex = -1;

                    var onHandle = false;
                        
                    if (_currentControlPointIndex != -1)
                    {
                        var point = controlPoints[_currentControlPointIndex];
                        
                        var leftHandlePos = point.position + point.leftHandle;
                        var rightHandlePos = point.position + point.rightHandle;
                        
                        Handles.color = Color.white;
                        Handles.DrawDottedLine(leftHandlePos, rightHandlePos, 0.5f);
                        
                        Handles.color = Color.blue;
                        var newLeftHandlePosition = Handles.FreeMoveHandle(leftHandlePos, 0.15f, Vector3.zero, Handles.CircleHandleCap);
                        var newRightHandlePosition = Handles.FreeMoveHandle(rightHandlePos, 0.15f, Vector3.zero, Handles.CircleHandleCap);
                        
                        if (newLeftHandlePosition != leftHandlePos)
                        {
                            var so = new SerializedObject(_currentConnection);
                            so.Update();
                            Undo.RecordObject(_currentConnection, "Move Left Handle");
                            
                            point.leftHandle = newLeftHandlePosition - point.position;
                        }
                        else if (newRightHandlePosition != rightHandlePos)
                        {
                            var so = new SerializedObject(_currentConnection);
                            so.Update();
                            Undo.RecordObject(_currentConnection, "Move Right Handle");
                            
                            point.rightHandle = newRightHandlePosition - point.position;
                        }
                        
                        if (HandleUtility.DistanceToCircle(leftHandlePos, 0.15f) < 0.25f || HandleUtility.DistanceToCircle(rightHandlePos, 0.15f) < 0.25f)
                        {
                            onHandle = true;
                        }
                    }
                    #endregion

                    #region Show If Hovering Other Cable
                    if (!onHandle && _mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con3))
                    {
                        var cps = con3.GetControlPoints();
                        for(var i = 1; i < cps.Length; i++)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.white;
                                Handles.DrawLine(cps[i - 1].position, cps[i].position);
                            }
                        }
                    }
                    #endregion

                    #region Current Cable Editing
                    
                    var mousePos = RaycastPosition();

                    var closestPosition = Vector3.zero;
                    var closestSegment = 0;
                    var closestDistance = float.MaxValue;
                    var mouseOverControlPoint = -1;

                    for(var i = 0; i < controlPoints.Length; i++)
                    {
                        if (i > 0)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(controlPoints[i - 1].position, controlPoints[i].position);
                            }
                            
                            var p = ClosestPointOnLineSegment(controlPoints[i - 1].position, controlPoints[i].position, mousePos); 
                            var d = Vector3.Distance(p, mousePos);
                            if (d < closestDistance)
                            {
                                closestPosition = p;
                                closestSegment = i - 1;
                                closestDistance = d;
                            }
                            
                            if (i < controlPoints.Length - 1)
                            {
                                if (HandleUtility.DistanceToCircle(controlPoints[i].position, 0.15f) < 0.25f)
                                {
                                    mouseOverControlPoint = i;

                                    if (Event.current.type == EventType.Repaint)
                                    {
                                        Handles.color = Color.red;
                                        Handles.SphereHandleCap(0, controlPoints[i].position, Quaternion.identity, 0.15f, EventType.Repaint);
                                        Handles.color = Color.yellow;
                                    }
                                }
                                else if (Event.current.type == EventType.Repaint)
                                {
                                    if(_currentControlPointIndex == i)
                                        Handles.color = Color.green;
                                    else
                                        Handles.color = Color.yellow;

                                    Handles.SphereHandleCap(0, controlPoints[i].position, Quaternion.identity, 0.15f, EventType.Repaint);
                                }
                            }
                        }
                        
                        //Hold shift to see handles
                        if (Event.current.shift)
                        {
                            var pos = Handles.PositionHandle(controlPoints[i].position, Quaternion.identity);
                            if (pos != controlPoints[i].position)
                            {
                                var so = new SerializedObject(_currentConnection);
                                so.Update();
                                Undo.RecordObject(_currentConnection, "Move Control Point");
                                _currentConnection.controlPoints[i - 1].position = Snap(pos);
                            }
                        }
                    }

                    if (!onHandle)
                    {
                        if (mouseOverControlPoint == -1)
                        {
                            Handles.color = Color.white;
                            if (Event.current.type == EventType.Repaint)
                                Handles.SphereHandleCap(0, closestPosition, Quaternion.identity, 0.25f,
                                    EventType.Repaint);

                            if (HandleUtility.DistanceToCircle(closestPosition, 0.25f) < 0.25f)
                            {
                                onHandle = true;
                                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                {
                                    //add undo
                                    var so = new SerializedObject(_currentConnection);
                                    so.Update();
                                    Undo.RecordObject(_currentConnection, "Add Control Point");
                                    
                                    var dir = controlPoints[closestSegment].position - controlPoints[closestSegment + 1].position;
                                
                                    var point = new CircuitConnection.ControlPoint { position = closestPosition, leftHandle = dir.normalized, rightHandle = -dir.normalized };
                                    _currentConnection.controlPoints.Insert(closestSegment, point);
                                    _action = Action.MovingControlPoint;
                                    _currentControlPointIndex = closestSegment + 1;
                                    so.ApplyModifiedProperties();
                                    _currentConnection.UpdateVisual(true);
                                    Event.current.Use();
                                }
                            }
                        }
                        else
                        {
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                            {
                                _currentControlPointIndex = mouseOverControlPoint;
                                Event.current.Use();
                            }
                            else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                            {
                                _action = Action.MovingControlPoint;
                                _currentControlPointIndex = mouseOverControlPoint;
                                Event.current.Use();
                            }
                        }
                    }

                    if (Event.current.type == EventType.KeyDown && _currentControlPointIndex != -1)
                    {
                        if (Event.current.keyCode == KeyCode.Backspace)
                        {
                            var so = new SerializedObject(_currentConnection);
                            so.Update();
                            Undo.RecordObject(_currentConnection, "Delete Control Point");
                            _currentConnection.controlPoints.RemoveAt(_currentControlPointIndex - 1);
                            _currentControlPointIndex--;
                            so.ApplyModifiedProperties();
                            Event.current.Use();
                            _currentConnection.UpdateVisual(true);
                        }
                        //if press f, focus onto it
                        if (Event.current.keyCode == KeyCode.F)
                        {
                            var pos = controlPoints[_currentControlPointIndex].position;
                            SceneView.lastActiveSceneView.LookAt(pos);
                            Event.current.Use();
                        }
                    }
                    #endregion
                    
                    #region Change Selected
                    if (!onHandle && Event.current.type == EventType.MouseDown && Event.current.button == 0 && _mouseOverObject != _currentConnection.gameObject)
                    {
                        if (_mouseOverObject != null && _mouseOverObject.TryGetComponent<CircuitConnection>(out var con))
                        {
                            Selection.activeGameObject = _mouseOverObject;
                            UnityEditor.Tools.current = Tool.None;
                            _action = Action.EditingConnection;
                            _currentControlPointIndex = -1;
                            _currentConnection = con;
                            Event.current.Use();
                        }
                        else
                            _action = Action.Idle;
                        break;
                    }
                    #endregion
                    break;

                case Action.MovingControlPoint:
                    if (_currentConnection == null)
                        _action = Action.Idle;
                    else
                    {
                        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                            var so = new SerializedObject(_currentConnection);
                            so.Update();
                            Undo.RecordObject(_currentConnection, "Move Control Point");
                            _currentConnection.controlPoints[_currentControlPointIndex - 1].position = Snap(RaycastPosition());
                            _currentConnection.UpdateVisual(true);
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            _action = Action.EditingConnection;
                        }
                    }
                    break;
            }
            
            sceneView.Repaint();
            Repaint();
        }

        private Vector3 Snap(Vector3 point)
        {
            //if holding control, snap to gridUnit
            if (Event.current.control)
            {
                point.x = Mathf.Round(point.x / gridUnit) * gridUnit;
                point.y = Mathf.Round(point.y / gridUnit) * gridUnit;
                point.z = Mathf.Round(point.z / gridUnit) * gridUnit;
            }
            
            return point;
        }

        private Vector3 GetPointOnPlane(Vector3 point, Vector3 planePos, Vector3 planeNormal)
        {
            var d = Vector3.Dot(planeNormal, planePos);
            var t = (d - Vector3.Dot(planeNormal, point)) / Vector3.Dot(planeNormal, planeNormal);
            return point + t * planeNormal;
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
            //Undo create
            var go = new GameObject("Connection");
            go.SetActive(false);
            var con = go.AddComponent<CircuitConnection>();
            con.a = a;
            con.b = b;
            go.transform.parent = a.transform;
            go.SetActive(true);
            Undo.RegisterCreatedObjectUndo(go, "Create Connection");
            return con;
        }
    }
}
#endif