using System;
using Solis.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Interface.Widgets
{
    /// <summary>
    /// Base class for list entries.
    /// </summary>
    public class WidgetListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public Image background;
        
        [Header("SETTINGS")]
        public Color colorNormal = Color.white;
        public Color colorHover = new(224, 224, 244);
        public Color colorClicked = new(192, 192, 192);
        public string clickSound = "click";
        #endregion

        #region Public Fields
        public Action onClick;
        #endregion

        #region Private Fields
        private bool _inside;
        private bool _clicked;
        private float _pressedTime;
        #endregion
        
        #region Unity Callbacks
        private void Update()
        {
            var color = colorNormal;

            if (_clicked)
                color = colorClicked;
            else if (_inside)
                color = colorHover;

            background.color = Color.Lerp(background.color, color, Time.deltaTime * 10);
        }
        #endregion

        #region IPointerEnterHandler Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            _inside = true;
        }
        #endregion

        #region IPointerExitHandler Implementation
        public void OnPointerExit(PointerEventData eventData)
        {
            _inside = false;
        }
        #endregion

        #region IPointerDownHandler Implementation
        public void OnPointerDown(PointerEventData eventData)
        {
            _clicked = true;
            _pressedTime = Time.time;
        }
        #endregion

        #region IPointerUpHandler Implementation
        public void OnPointerUp(PointerEventData eventData)
        {
            _clicked = false;
            if (_inside && Time.time - _pressedTime < 0.5f)
            {
                AudioSystem.PlayVfxStatic(clickSound);
                onClick?.Invoke();
            }
        }
        #endregion
    }
}