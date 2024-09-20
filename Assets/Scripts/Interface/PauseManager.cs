using System;
using System.Collections;
using System.Collections.Generic;
using Solis.Core;
using Solis.Player;
using Solis.Settings;
using Solis.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : WindowManager
{
    public GameObject pauseVolume;
    public SolisNetworkManager networkManager;
    public GameManager gameManager;
    
    private CanvasGroup pauseMenu;
    private bool isPaused = false;
    
    public static Action<bool> OnPause;

    private void Awake()
    {
        TryGetComponent(out pauseMenu);
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        pauseMenu.alpha = visible ? 1 : 0;
        pauseMenu.interactable = visible;
        pauseMenu.blocksRaycasts = visible;
    }

    private void Update()
    {
        if(gameManager.IsOnLobby || !gameManager.isGameStarted) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseVolume.SetActive(isPaused);
        SetVisible(false);
        ChangeWindow(0);
        OnPause?.Invoke(false);
    }

    public void Pause()
    {
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnPause?.Invoke(true);
    }

    public void Resume()
    {
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
       OnPause?.Invoke(false);
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseVolume.SetActive(isPaused);
        SetVisible(true);
        ChangeWindow(0);
        OnPause?.Invoke(true);
    }
    
    public void BugReport()
    {
        Application.OpenURL("https://forms.gle/vmPGyamH8Dcc8S9J8"); 
    }
    
    public void ExitGame()
    {
        networkManager.Close();
    }
}
