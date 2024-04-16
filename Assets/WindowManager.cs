using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField]
    private protected int index;
    [SerializeField]
    private int startIndex;
    [SerializeField]
    private List<CanvasWindow> windows = new List<CanvasWindow>();

    public int StartIndex
    {
        get => startIndex;
        set => startIndex = value;
    }

    public int LastIndex => windows.Count - 1;

    protected virtual void OnEnable()
    {
        if (windows.Count != 0) ShowWindow(startIndex);
    }

    public virtual void ShowWindow(int index)
    {
        this.index = Mathf.Clamp(index, 0, windows.Count - 1);

        for (var i = 0; i < windows.Count; i++) SetWindow(i, i == index);
    }

    public void SetWindow(int i, bool active)
    {
        windows[i].SetActive(active);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (windows.Count == 0) return;

        index = Mathf.Clamp(index, 0, windows.Count - 1);
        ShowWindow(index);
    }
#endif
}

[System.Serializable]
public class CanvasWindow
{
    public string name = "Window";
    public CanvasGroup canvasGroup;
    public bool IsActive => canvasGroup.alpha >= .99f;

    public void SetActive(bool visible, bool forceActive = true)
    {
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
        if(forceActive)canvasGroup.gameObject.SetActive(visible);
    }

    public CanvasWindow(CanvasGroup canvasGroup)
    {
        this.canvasGroup = canvasGroup;
        name = canvasGroup.name;
    }
}