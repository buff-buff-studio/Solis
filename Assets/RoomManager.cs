using System;
using System.Collections;
using System.Collections.Generic;
using NetBuff;
using NetBuff.Components;
using NetBuff.Misc;
using SolarBuff;
using SolarBuff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance;
    
    public TextMeshProUGUI roomList;
    public GameObject playBtt;
    public IntNetworkValue levelIndex = new IntNetworkValue(-1);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void OnEnable()
    {
        WithValues(levelIndex);
        playBtt.SetActive(false);
        playBtt.SetActive(!NetworkManager.Instance.IsServerRunning);
        levelIndex.OnValueChanged += LevelIndexOnOnValueChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        var gm = FindObjectsOfType<GameManager>();
        if (gm.Length == 0 ) return;
        PlayerControllerCore.Players.ForEach(p =>
        {
            p.canMove = true;
            p.transform.position = gm[0].GetPlayerSpawnPoint(p.type).position;
            p.transform.rotation = gm[0].GetPlayerSpawnPoint(p.type).rotation;
            p.SearchCamera();
        });
    }

    private void LevelIndexOnOnValueChanged(int oldvalue, int newvalue)
    {
        /*
        gameManagers[oldvalue].cam.gameObject.SetActive(false);
        gameManagers[newvalue].cam.gameObject.SetActive(true);
        PlayerControllerCore.Players.ForEach(p =>
        {
            p.transform.position = gameManagers[newvalue].GetPlayerSpawnPoint(p.type).position;
            p.transform.rotation = gameManagers[newvalue].GetPlayerSpawnPoint(p.type).rotation;
            p.SearchCamera();
        });
        */
    }

    public void StartGame()
    {
        if(!HasAuthority) return;
        //SceneManager.LoadScene("Scenes/Puzzle_1", LoadSceneMode.Single);
        NetworkManager.Instance.LoadScene("Scenes/Puzzle_1", LoadSceneMode.Single);
    }
}
