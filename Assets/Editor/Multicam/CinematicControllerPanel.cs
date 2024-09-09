using Solis.Misc.Multicam;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor.Multicam
{
#if UNITY_EDITOR
    [Overlay(typeof(SceneView), "Cinematic Controller", true)]
    [Icon("Assets/Editor/Multicam/Icons/CinematicControllerPanelIcon.png")]
    public class CinematicControllerPanel : Overlay
    {
        private CinematicController _controller;

        //Enums Values
        private CameraTransition _transition;
        private CameraMovement _movement;

        //Icons
        private Texture2D _addFrameIcon, _updateFrameIcon, _bakeRollIcon, _bakeAllRollsIcon;
        private Texture2D _alignSceneCameraIcon, _selectCinematicControllerIcon;

        public override void OnCreated()
        {
            base.OnCreated();

            _addFrameIcon = Resources.Load<Texture2D>("Editor/CinematicController/AddFrame");
            _updateFrameIcon = Resources.Load<Texture2D>("Editor/CinematicController/UpdateFrame");
            _bakeRollIcon = Resources.Load<Texture2D>("Editor/CinematicController/BakeRoll");
            _bakeAllRollsIcon = Resources.Load<Texture2D>("Editor/CinematicController/BakeAllRolls");

            _alignSceneCameraIcon = EditorGUIUtility.IconContent("Camera Icon").image as Texture2D;
            _selectCinematicControllerIcon = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;

            if (SceneManager.sceneCount != 1 ||
                SceneManager.GetActiveScene().name is not ("Menu" or "Core" or "Lobby"))
            {
                _controller = Object.FindObjectsByType<CinematicController>(FindObjectsSortMode.None)[0];

                if (_controller == null)
                    Debug.LogError("Cinematic Controller not found in the scene");
            }

            EditorApplication.hierarchyChanged += _Repaint;
            EditorApplication.playModeStateChanged += _Repaint1;
            EditorApplication.focusChanged += _Repaint2;
            EditorApplication.projectChanged += _Repaint;
            //SceneView.windowFocusChanged += _Repaint;
        }

        public override void OnWillBeDestroyed()
        {
            EditorApplication.hierarchyChanged -= _Repaint;
            EditorApplication.playModeStateChanged -= _Repaint1;
            EditorApplication.focusChanged -= _Repaint2;
            EditorApplication.projectChanged -= _Repaint;
            //SceneView.windowFocusChanged -= _Repaint;
        }

        private void _Repaint2(bool obj)
        {
            _Repaint();
        }

        private void _Repaint1(PlayModeStateChange obj)
        {
            _Repaint();
        }

        private void _Repaint()
        {
            displayed = false;
            displayed = true;
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "Cinematic Tools", tooltip = ""};

            if(_controller == null)
            {
                var error = new Label("Cinematic Controller don't\nwork in this scene.")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 1,
                        marginTop = 5, marginBottom = 5
                    }
                };
                root.Add(error);
                if (SceneManager.sceneCount != 1 ||
                    SceneManager.GetActiveScene().name is not ("Menu" or "Core" or "Lobby"))
                {
                    _controller = Object.FindObjectsByType<CinematicController>(FindObjectsSortMode.None)[0];
                }

                return root;
            }

            return Application.isPlaying ? GameScene(root) : EditorScene(root);
        }

        private VisualElement EditorScene(VisualElement root)
        {
            var horizontalAlign = new VisualElement() {style = {flexDirection = FlexDirection.Row}};
            var header = new Label("Roll:")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 1,
                    marginTop = 5, marginBottom = 5
                }
            };
            horizontalAlign.Add(header);

            //Change Camera Roll
            var dropdown = new DropdownField("", _controller.GetRollsName, _controller.currentRoll){style = { width = 170}};
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _controller.currentRoll = _controller.GetRollsName.IndexOf(evt.newValue);
                _Repaint();
            });
            horizontalAlign.Add(dropdown);
            root.Add(horizontalAlign);

            //Controller
            //Header
            var subtitle = new Label("Frame Controller:")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.BoldAndItalic, marginLeft = 1,
                    marginTop = 5, marginBottom = 5
                }
            };
            root.Add(subtitle);

            horizontalAlign = new VisualElement() {style = {flexDirection = FlexDirection.Row, marginTop = -3, marginBottom = 2}};

            var label = new Label($"Frame: {_controller.CurrentRoll.currentFrame}")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Normal, marginLeft = 1, marginRight = 3, marginTop = 3.5f, width = 55
                }
            };
            horizontalAlign.Add(label);

            var slider = new SliderInt(0, _controller.CurrentRoll.framing.Count - 1)
            {
                value = _controller.CurrentRoll.currentFrame,
                name = "Frame Slider",
                tooltip = "Change the current frame",
                style = {marginLeft = 3, marginBottom = 5, width = 118, marginTop = 2}
            };
            slider.RegisterValueChangedCallback(evt =>
            {
                _controller.CurrentRoll.currentFrame = (int) evt.newValue;
                _controller.SetCameraToCurrentFrame();
                _Repaint();
            });
            horizontalAlign.Add(slider);

            var button = new Button(() =>
            {
                SceneView.lastActiveSceneView.AlignViewToObject(_controller.Camera);
                _Repaint();
            })
            {
                name = "Align Scene Camera", tooltip = "Align the scene camera to the current frame",
                style = { backgroundImage = _alignSceneCameraIcon, width = 20, height = 20}
            };
            horizontalAlign.Add(button);
            root.Add(horizontalAlign);

            var enumField = new EnumField("Transition", _controller.CurrentRoll.CurrentFrame.transition)
            {
                style = {marginLeft = 1, marginBottom = 5}
            };
            enumField.RegisterValueChangedCallback(evt => { _transition = (CameraTransition) evt.newValue; });
            root.Add(enumField);

            enumField = new EnumField("Movement", _controller.CurrentRoll.CurrentFrame.movement)
            {
                style = {marginLeft = 1, marginBottom = 5}
            };
            enumField.RegisterValueChangedCallback(evt => { _movement = (CameraMovement) evt.newValue; });
            root.Add(enumField);

            horizontalAlign = new VisualElement() {style = {flexDirection = FlexDirection.Row, justifyContent = Justify.Center}};
            button = new Button(() => { _controller.AddFrame(); })
            {
                name = "Add Frame", tooltip = "Add a new frame", style = { width = 38, height = 38, backgroundImage = _addFrameIcon}
            };
            horizontalAlign.Add(button);
            button = new Button(() => { _controller.UpdateFrame(_transition, _movement); })
            {
                name = "Update Frame", tooltip = "Update the current frame",  style = { width = 38, height = 38, backgroundImage = _updateFrameIcon}
            };
            horizontalAlign.Add(button);
            root.Add(horizontalAlign);

            //horizontalAlign = new VisualElement() {style = {flexDirection = FlexDirection.Row, justifyContent = Justify.Center, marginTop = 5}};
            button = new Button(() => { _controller.BakeAnimation(); })
            {
                name = "Bake Roll", tooltip = "Bake the current roll", style = { width = 38, height = 38, backgroundImage = _bakeRollIcon}
            };
            horizontalAlign.Add(button);

            //Button to select the object CinematicController in the hierarchy
            button = new Button(() =>
            {
                Selection.activeGameObject = _controller.gameObject;
                _Repaint();
            })
            {
                name = "Select Cinematic Controller", tooltip = "Select the Cinematic Controller in the hierarchy", iconImage = _selectCinematicControllerIcon,
                style = {width = 38, height = 38, marginLeft = 32}
            };
            horizontalAlign.Add(button);
            root.Add(horizontalAlign);

            //TODO: Tipo de Transição, Duração do Frame, Duração da Transição

            return root;
        }

        private VisualElement GameScene(VisualElement root)
        {
            var header = new Label("Cinematic Controller")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 1,
                    marginTop = 5, marginBottom = 5
                }
            };
            root.Add(header);

            //Change Camera Roll
            var dropdown = new DropdownField("", _controller.GetRollsName, _controller.currentRoll){style = { width = 170}};
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _controller.currentRoll = _controller.GetRollsName.IndexOf(evt.newValue);
                _Repaint();
            });
            root.Add(dropdown);

            if (CinematicController.IsPlaying == false)
            {
                var button = new Button(_controller.Play)
                {
                    text = "Play", tooltip = "Play the current roll", style = { marginLeft = 1, marginTop = 3 }
                };
                root.Add(button);
            }
            else
            {
                var button = new Button(_controller.Reset)
                {
                    text = "Replay", tooltip = "Replay the current roll", style = { marginLeft = 1, marginTop = 3 }
                };
                root.Add(button);

                button = new Button(_controller.Stop)
                {
                    text = "Stop", tooltip = "Stop the current roll", style = { marginLeft = 1, marginTop = 3 }
                };
                root.Add(button);
            }

            return root;
        }

        private void _OnSceneGUI(SceneView obj)
        {
            if (!displayed)
                return;
        }
    }
#endif
}