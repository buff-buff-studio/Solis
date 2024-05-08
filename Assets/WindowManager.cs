using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Device.Application;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private int startIndex;
    [SerializeField] private int currentIndex;
    [SerializeField] private List<CanvasGroup> windows;

    #region Unity Callbacks

    protected virtual void Start()
    {
        currentIndex = startIndex;
        for (var i = 0; i < windows.Count; i++)
        {
            windows[i].gameObject.SetActive(true);
            SetWindowActive(i, i == startIndex);
        }
    }

    #if UNITY_EDITOR
    protected void OnValidate()
    {
        if (windows.Count == 0)
            return;

        startIndex = Mathf.Clamp(startIndex, 0, windows.Count - 1);
        currentIndex = Mathf.Clamp(currentIndex, 0, windows.Count - 1);

        if (!Application.isPlaying)
        {
            for (var i = 0; i < windows.Count; i++)
            {
                windows[i].gameObject.SetActive(true);
                SetWindowActive(i, i == startIndex);
            }
        }
    }
    #endif

    #endregion
    private void SetWindowActive(int index, bool active)
    {
        if (index >= windows.Count || index < 0)
            return;

        windows[index].alpha = active ? 1 : 0;
        windows[index].blocksRaycasts = active;
        windows[index].interactable = active;
    }

    public void ChangeWindow(string name)
    {
        for (var i = 0; i < windows.Count; i++)
        {
            if (windows[i].name != name) continue;
            ChangeWindow(i);
            return;
        }
    }

    public void ChangeWindow(int index)
    {
        if (index == currentIndex)
            return;

        SetWindowActive(currentIndex, false);
        SetWindowActive(index, true);
        currentIndex = index;
    }
}
