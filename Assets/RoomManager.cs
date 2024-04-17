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
    public GameManager[] gameManagers;
    public IntNetworkValue levelIndex = new IntNetworkValue(-1);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameManagers = FindObjectsOfType<GameManager>();
        foreach (var gm in gameManagers) gm.gameObject.SetActive(false);
        gameManagers[0].SetActive(true);
    }
    
    private void OnEnable()
    {
        WithValues(levelIndex);
        levelIndex.OnValueChanged += LevelIndexOnOnValueChanged;
    }

    private void LevelIndexOnOnValueChanged(int oldvalue, int newvalue)
    {
        gameManagers[oldvalue].cam.gameObject.SetActive(false);
        gameManagers[newvalue].cam.gameObject.SetActive(true);
        PlayerControllerCore.Players.ForEach(p =>
        {
            p.transform.position = gameManagers[newvalue].GetPlayerSpawnPoint(p.type).position;
            p.transform.rotation = gameManagers[newvalue].GetPlayerSpawnPoint(p.type).rotation;
            p.SearchCamera();
        });
    }

    public void StartGame()
    {
        if(HasAuthority) SceneManager.LoadScene("Scenes/LevelTest2", LoadSceneMode.Single);
        NetworkManager.Instance.LoadScene("Scenes/LevelTest2", LoadSceneMode.Single);
    }
}
