using UnityEngine;

namespace Interface
{
    /// <summary>
    /// Base class for all list screens.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseListScreen<T> : Screen where T : MonoBehaviour
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public RectTransform content;

        public GameObject prefabEntry;
        public GameObject prefabEmpty;
        #endregion

        #region Unity Callbacks
        protected virtual void OnEnable()
        {
            RefreshList();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clear all entries in the list.
        /// </summary>
        public void ClearEntries()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }

        /// <summary>
        /// Create a new entry in the list.
        /// </summary>
        /// <returns></returns>
        public T CreateEntry()
        {
            var entry = Instantiate(prefabEntry, content);
            return entry.GetComponent<T>();
        }

        /// <summary>
        /// Show the empty message.
        /// </summary>
        public void ShowEmpty()
        {
            if (prefabEmpty == null)
                return;
            Instantiate(prefabEmpty, content);
        }
            
        /// <summary>
        /// Refresh the list.
        /// </summary>
        public void RefreshList()
        {
            ClearEntries();
            OnRefreshList();
        }
        
        /// <summary>
        /// Close the screen.
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract void OnRefreshList();
        #endregion
    }
}