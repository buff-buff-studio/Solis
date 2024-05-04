using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interface.Widgets
{
    /// <summary>
    /// Base class for all widgets. Used to implement ui navigation.
    /// </summary>
    public abstract class Widget : MonoBehaviour
    {
        #region Public Static Properties
        public static IReadOnlyList<Widget> Widgets => _Widgets;
        #endregion

        #region Private Static Properties
        private static readonly List<Widget> _Widgets = new();
        #endregion
        
        #region Types
        [Serializable]
        public enum ActionType
        {
            None,
            
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            
            Confirm,
            Cancel,
            
            Context, //Square
            View, //Triangle
            
            TabBefore,
            TabAfter,
            
            PageBefore,
            PageAfter,
        }
        #endregion

        #region Unity Callbacks
        public virtual void OnEnable()
        {
            _Widgets.Add(this);
        }
        
        public virtual void OnDisable()
        {
            _Widgets.Remove(this);
        }
        #endregion

        #region Public Methods
        public virtual bool PerformAction(bool selected, ActionType type)
        {
            return false;
        }
        
        public virtual string GetActionDescription(bool selected, ActionType type)
        {
            return null;
        }
        #endregion
    }
}