using Interface;
using Solis.Core;
using Solis.Data;
using UnityEngine.UI;

namespace Solis.Interface.Lobby
{
    /// <summary>
    /// Represents a screen that shows a list of saves.
    /// </summary>
    public class SaveListScreen : BaseListScreen<SaveListEntry>
    {
        #region Inspector Fields
        public Button refreshButton;
        #endregion

        #region Abstract Methods Implementation
        protected override void OnRefreshList()
        {
            refreshButton.interactable = false;
            Save.GetAllSnapshots(_OnFindSnapshot, _OnFinish);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a new save.
        /// </summary>
        public void CreateNewSave()
        {
            GameManager.Instance.Save.New();
            Close();
        }
        #endregion

        #region Private Methods
        private void _OnFinish()
        {
            refreshButton.interactable = true;
        }

        private void _OnFindSnapshot(SaveSnapshot obj)
        {
            var time = obj.lastModificationTime.ToString("dd/MM/yyyy HH:mm:ss");

            var entry = CreateEntry();
            entry.imagePreview.texture = obj.preview;
            entry.textName.text = obj.name;
            entry.textLastModification.text = $"Save Time: {time}";
            entry.textPlayTime.text = $"Time Played: {Save.PlayTimeToString(obj.playTime)}";

            entry.onClick += () =>
            {
                GameManager.Instance.Save.LoadData(obj);
                Close();
            };
        }
        #endregion
    }
}