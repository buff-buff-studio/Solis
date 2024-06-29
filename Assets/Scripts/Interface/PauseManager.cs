using System;
using System.Collections;
using System.Collections.Generic;
using Solis.Core;
using Solis.Player;
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
            Debug.Log(SceneManager.GetActiveScene().name);
            isPaused = !isPaused;
            Cursor.visible = isPaused;
            Cursor.lockState = !isPaused ? CursorLockMode.Locked : CursorLockMode.None;
            pauseVolume.SetActive(isPaused);
            SetVisible(isPaused);
            OnPause?.Invoke(isPaused);
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Cursor.visible = isPaused;
        Cursor.lockState = !isPaused ? CursorLockMode.Locked : CursorLockMode.None;
        pauseVolume.SetActive(isPaused);
        SetVisible(false);
        OnPause?.Invoke(isPaused);
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
