﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private GameData gameData;
    public GameObject bottomBound;
    public GameObject leftBound;
    public GameObject rightBound;

    [Header("Managers")]
    public InputManager inputManager;
    public ScenesManager sceneManager;
    public SpawnManager spawnManager;
    private Dictionary<int, TankManager> tanks;

    [Header("Tank Generation")]
    public int maxPlayer = 8;
    public List<GameObject> persos;
    public Transform minLocation;
    public Transform maxLocation;

    private void Awake() {
        tanks = new Dictionary<int, TankManager>();
    }
    // Use this for initialization
    void Start() {
        gameData = FindObjectOfType<GameData>();

        shufflePersos();

        for (int i = 1; i <= gameData.numberPlayer; i++) {
            GameObject tank = GameObject.Instantiate(persos[i % persos.Count]);

            tank.transform.position = spawnManager.allocateSpawnPoint(i);

            TankManager tankManager = tank.GetComponent<TankManager>();
            tankManager.setGameManager(this);
            tankManager.PlayerNumber = i;

            tanks.Add(i, tankManager);
        }

        inputManager.setKeys(gameData.playerKeys);
    }

    public void updateKeyState(int player, bool state) {
        TankManager tank;
        tanks.TryGetValue(player, out tank);
        tank.setIsLoading(state);
    }
   
    private void shufflePersos() {
        List<int> indexes = new List<int>(persos.Count);
        List<GameObject> newPersos = new List<GameObject>(persos.Count);

        for (int i = 0; i < persos.Count; i++) {
            indexes.Add(i);
        }

        int size = indexes.Count;
        for (int i = 0; i < size; i++) {
            int index = Random.Range(0, indexes.Count);
            newPersos.Add(persos[indexes[index]]);
            indexes.RemoveAt(index);
        }
        persos = newPersos;
    }

    public void OnPlayerKill(int killerId, int victimId) {
        Player killer = gameData.players[killerId];
        Debug.LogFormat("P{0} killed P{1}", killerId, victimId);
        killer.kill += 1;

        Player victim = gameData.players[victimId];
        victim.death += 1;
        spawnManager.freeSpawnPoint(victimId);
        TankManager victimTank = tanks[victimId];
        if (victim.death < gameData.maxLives) {
            victimTank.Health = victimTank.maxHealth;
            victimTank.transform.position = spawnManager.allocateSpawnPoint(victimId);
        } else {
            Destroy(victimTank.gameObject);
        }
    }
}
