using Solis.Misc.Multicam;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Multicam
{
#if UNITY_EDITOR
    [Overlay(typeof(SceneView), "Cinematic Controller", true)]
    [Icon("Assets/Editor/Multicam/Icons/CinematicControllerPanelIcon.png")]
    public class CinematicControllerPanel : Overlay
    {
        private CinematicController _controller;

        //Icons
        private Texture2D _addFrameIcon, _updateFrameIcon, _bakeRollIcon, _bakeAllRollsIcon;
        private Texture2D _alignSceneCameraIcon;

        public override void OnCreated()
        {
            base.OnCreated();

            _addFrameIcon = Resources.Load<Texture2D>("Editor/CinematicController/AddFrame");
            _updateFrameIcon = Resources.Load<Texture2D>("Editor/CinematicController/UpdateFrame");
            _bakeRollIcon = Resources.Load<Texture2D>("Editor/CinematicController/BakeRoll");
            _bakeAllRollsIcon = Resources.Load<Texture2D>("Editor/CinematicController/BakeAllRolls");

            _alignSceneCameraIcon = EditorGUIUtility.IconContent("SceneViewCamera").image as Texture2D;

            _controller = Object.FindObjectsByType<CinematicController>(FindObjectsSortMode.None)[0];

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
            var dropdown = new DropdownField("", _controller.GetRollsName, _controller.currentRoll){style = { width = 150}};
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
                    unityFontStyleAndWeight = FontStyle.Normal, marginLeft = 1, marginRight = 3, marginTop = 3.5f
                }
            };
            horizontalAlign.Add(label);

            var slider = new SliderInt(0, _controller.CurrentRoll.framing.Count - 1)
            {
                value = _controller.CurrentRoll.currentFrame,
                name = "Frame Slider",
                tooltip = "Change the current frame",
                style = {marginLeft = 3, marginBottom = 5, width = 100, marginTop = 2}
            };
            slider.RegisterValueChangedCallback(evt =>
            {
                _controller.CurrentRoll.currentFrame = (int) evt.newValue;
                _controller.SetCameraToCurrentFrame();
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

            horizontalAlign = new VisualElement() {style = {flexDirection = FlexDirection.Row, justifyContent = Justify.Center}};
            button = new Button(() => { _controller.AddFrame(); })
            {
                name = "Add Frame", tooltip = "Add a new frame", style = { width = 38, height = 38, backgroundImage = _addFrameIcon}
            };
            horizontalAlign.Add(button);
            button = new Button(() => { _controller.UpdateFrame(); })
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
            root.Add(horizontalAlign);

            //TODO: Tipo de Transição, Duração do Frame, Duração da Transição

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