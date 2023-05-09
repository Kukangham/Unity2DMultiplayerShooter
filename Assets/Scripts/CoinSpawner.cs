using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    public static CoinSpawner Instance { get; private set; }

    [SerializeField] private GameObject coinPrefab;

    private GameObject currentCoin;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0));
            spawnPosition.z = 0;
            currentCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            currentCoin.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void SpawnCoin()
    {
        SpawnCoinServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCoinServerRpc()
    {
        Vector3 playerPosition = Player.LocalInstance.transform.position;

        // Generate a random position within the screen bounds
        Vector3 randomPosition = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(randomPosition);

        // Make sure the z position of the coin is set to 0
        spawnPosition.z = 0;

        // Make sure the spawn position is not too close to the player's position
        float minDistance = 1f;
        while (Vector3.Distance(spawnPosition, playerPosition) < minDistance)
        {
            randomPosition = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
            spawnPosition = Camera.main.ViewportToWorldPoint(randomPosition);
        }

        if (currentCoin != null) Destroy(currentCoin);

        currentCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        currentCoin.GetComponent<NetworkObject>().Spawn(true);
    }
}
