using System.Collections.Generic;
using NetBuff.Misc;
using Solis.Misc.Multicam;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace Solis.Circuit.Gates
{
    /// <summary>
    /// Basic gate component that can be used to create simple logic circuits.
    /// </summary>
    public class CircuitCinematicGate : CircuitComponent
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug input;
        public CircuitPlug output;

        [Header("SETTINGS")]
        public int cinematicRoll = 1;
        public bool playOnPower = true;
        public int frameEvent;
        public bool invisibleOnPlay = false;
        #endregion

        #region Private Fields

        private bool _cinematicPlayed;
        private BoolNetworkValue _cinematicCallback = new(false);

        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(_cinematicCallback);
            if(invisibleOnPlay)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            var count = input.Connections.Length;
            var result = 0;
            for(var i = 0; i < count; i++)
            {
                if(input.ReadOutput(i).power > 0)
                    result++;
            }
            if (result > 0)
            {
                if (!_cinematicPlayed && playOnPower)
                {
                    _cinematicPlayed = true;
                    CinematicController.Instance.Play(cinematicRoll);
                    Debug.Log("Playing cinematic roll " + cinematicRoll);
                }
                return new CircuitData(_cinematicCallback.Value ? result : 0);
            }
            else
            {
                if(_cinematicPlayed) _cinematicPlayed = false;
                if (HasAuthority) _cinematicCallback.Value = false;
                return new CircuitData(0);
            }

        }

        protected override void OnRefresh()
        {
            if (output.Connections.Length > 0) return;

            var count = input.Connections.Length;
            var result = 0;
            for(var i = 0; i < count; i++)
            {
                if(input.ReadOutput(i).power > 0)
                    result++;
            }
            if (result > 0)
            {
                if (!_cinematicPlayed && playOnPower)
                {
                    _cinematicPlayed = true;
                    CinematicController.Instance.Play(cinematicRoll);
                    Debug.Log("Playing cinematic roll " + cinematicRoll);
                }
            }
            else
            {
                if(_cinematicPlayed) _cinematicPlayed = false;
                if (HasAuthority) _cinematicCallback.Value = false;
            }
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return input;
            yield return output;
        }
        #endregion

        #region Public Methods
        public void CinematicCallback()
        {
            if (HasAuthority) _cinematicCallback.Value = true;
            Refresh();
        }
        #endregion
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CircuitCinematicGate))]
    public class CircuitCinematicGateEditor : Editor
    {
        private CircuitCinematicGate _gate;
        private CinematicController _cinematicController;

        private SerializedProperty _input;
        private SerializedProperty _output;
        private SerializedProperty _cinematicRoll;
        private SerializedProperty _playOnPower;
        private SerializedProperty _frameEvent;
        private SerializedProperty _invisibleOnPlay;

        private void OnEnable()
        {
            _gate = target as CircuitCinematicGate;
            _input = serializedObject.FindProperty("input");
            _output = serializedObject.FindProperty("output");
            _cinematicRoll = serializedObject.FindProperty("cinematicRoll");
            _playOnPower = serializedObject.FindProperty("playOnPower");
            _frameEvent = serializedObject.FindProperty("frameEvent");
            _invisibleOnPlay = serializedObject.FindProperty("invisibleOnPlay");

            _cinematicController = GameObject.FindObjectOfType<CinematicController>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_input);
            EditorGUILayout.PropertyField(_output);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("If you added a frame in the middle of the roll, you need to verify all the Cinematic Gates frames.", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
            if (_cinematicController == null)
            {
                EditorGUILayout.HelpBox("No Cinematic Controller found in the scene.\nMaybe you are in the prefab mode.", MessageType.Error);
                EditorGUILayout.PropertyField(_cinematicRoll);
            }
            else _cinematicRoll.intValue = EditorGUILayout.Popup("Cinematic Roll", _cinematicRoll.intValue, _cinematicController.GetRollsName.ToArray());
            EditorGUILayout.PropertyField(_playOnPower);
            EditorGUI.BeginChangeCheck();
            var frameEvent = _frameEvent.intValue;
            EditorGUILayout.PropertyField(_frameEvent);
            if (EditorGUI.EndChangeCheck())
            {
                _frameEvent.intValue = Mathf.Clamp(_frameEvent.intValue, 0, _cinematicController.rolls[_cinematicRoll.intValue].framing.Count - 1);
                if (_frameEvent.intValue != frameEvent)
                {
                    UnityEventTools.RemovePersistentListener(
                        _cinematicController.rolls[_cinematicRoll.intValue].framing[frameEvent].onFrameShow, _gate.CinematicCallback);
                    var uEvent = _cinematicController.rolls[_cinematicRoll.intValue].framing[_frameEvent.intValue].onFrameShow;
                    if (uEvent.GetPersistentEventCount() == 0)
                    {
                        UnityEventTools.AddPersistentListener(uEvent, _gate.CinematicCallback);
                        Debug.LogWarning($"CinematicCallback added to the event on frame {_frameEvent.intValue}, and removed from frame {frameEvent}", _cinematicController);
                    }
                    else
                    {
                        var alreadyAdded = false;
                        for (int i = 0; i < uEvent.GetPersistentEventCount(); i++)
                        {
                            if (uEvent.GetPersistentTarget(i) == _gate && uEvent.GetPersistentMethodName(i) == "CinematicCallback")
                            {
                                Debug.LogWarning($"CinematicCallback already added to the event on frame {_frameEvent.intValue}, but removed from frame {frameEvent}", _cinematicController);
                                alreadyAdded = true;
                                break;
                            }
                        }
                        if(!alreadyAdded)
                        {
                            UnityEventTools.AddPersistentListener(uEvent, _gate.CinematicCallback);
                            Debug.LogWarning($"CinematicCallback added to the event on frame {_frameEvent.intValue}, and removed from frame {frameEvent}", _cinematicController);
                        }
                    }
                }
            }
            EditorGUILayout.PropertyField(_invisibleOnPlay);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Verify if exist another frame with this CinematicCallback"))
            {
                Debug.Log("Starting verification...");
                foreach (var r in _cinematicController.rolls)
                {
                    for (var i = 0; i < r.framing.Count; i++)
                    {
                        var f = r.framing[i];
                        if (f == _cinematicController.rolls[_cinematicRoll.intValue].framing[_frameEvent.intValue]) continue;
                        for (var j = 0; j < f.onFrameShow.GetPersistentEventCount(); j++)
                        {
                            if (f.onFrameShow.GetPersistentTarget(j) != _gate || f.onFrameShow.GetPersistentMethodName(j) != "CinematicCallback") continue;
                        
                            UnityEventTools.RemovePersistentListener(f.onFrameShow, _gate.CinematicCallback);
                            Debug.Log($"CinematicCallback removed from frame {i} in roll {r.name}", _cinematicController);
                        }
                    }
                }
                Debug.Log("Verification finished.");
            }
        }
    }
#endif
}