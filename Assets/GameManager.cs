using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetBuff.Components;
using SolarBuff.Player;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance { get; private set; }
	
	public Transform humanSpawnPoint;
	public Transform robotSpawnPoint;
	
	public Transform GetPlayerSpawnPoint(PlayerControllerCore.PlayerType playerType)
	{
		return playerType == PlayerControllerCore.PlayerType.Human ? humanSpawnPoint : robotSpawnPoint;
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}else Instance = this;
		
	}
}
