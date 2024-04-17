using System;
using System.Collections;
using System.Collections.Generic;
using ExamplePlatformer;
using UnityEngine;
using NetBuff.Components;
using SolarBuff.Player;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance { get; private set; }
	
	public Transform humanSpawnPoint;
	public Transform robotSpawnPoint;
	public OrbitCamera cam;
	
	public Transform GetPlayerSpawnPoint(PlayerControllerCore.PlayerType playerType)
	{
		return playerType == PlayerControllerCore.PlayerType.Human ? humanSpawnPoint : robotSpawnPoint;
	}

	private void OnEnable()
	{
		if (Instance != null)
		{
			gameObject.SetActive(false);
			return;
		}else Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this) Instance = null;
	}
}
