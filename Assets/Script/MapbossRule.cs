using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class MapBoss : NetworkBehaviour
{
    public GameObject boss1Prefab; // Prefab of boss 1
    public GameObject boss2Prefab; // Prefab of boss 2
    public GameObject winpanel;
    private int currentWave = 0;

    private MonsterManage monsterManage; // Reference to MonsterManage
    private Vector3 boss1DeathPosition; // Position where boss 1 was defeated

    private void Start()
    {
        // Find the MonsterManage object in the scene
        monsterManage = FindObjectOfType<MonsterManage>();

        if (monsterManage == null)
        {
            Debug.LogError("MonsterManage not found in the scene!");
            return;
        }

        // Start spawning the waves if on server
        if (IsServer)
        {
            StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWave < 2)
        {
            currentWave++;
            monsterManage.SetCurrentWave(currentWave);

            if (currentWave == 1)
            {
                SpawnBossServerRpc(1); // Spawn boss 1 in wave 1
            }
            else if (currentWave == 2)
            {
                SpawnBossServerRpc(2, boss1DeathPosition); // Spawn boss 2 at the position of boss 1's death
            }

            // Wait until all monsters are defeated before proceeding to the next wave
            yield return new WaitUntil(() => monsterManage.GetActiveEnemiesCount() == 0);
        }

        // Check if all enemies are defeated after the last wave (including bosses)
        if (monsterManage.GetActiveEnemiesCount() == 0)
        {
            Debug.Log("Victory!");
            winpanel.SetActive(true);
        }
    }


    // ServerRpc to spawn boss
    [ServerRpc(RequireOwnership = false)]
    private void SpawnBossServerRpc(int bossType, Vector3 spawnPosition = default)
    {
        // Determine which boss prefab to use based on bossType
        GameObject bossPrefab = null;
        if (bossType == 1)
        {
            bossPrefab = boss1Prefab; // Assign your boss 1 prefab here
        }
        else if (bossType == 2)
        {
            bossPrefab = boss2Prefab; // Assign your boss 2 prefab here
        }

        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab not found!");
            return;
        }

        if (spawnPosition == default) // If no position specified, use a random position
        {
            spawnPosition = GetRandomSpawnPosition();
        }

        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = boss.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        // Add boss to active enemies list in MonsterManage
        monsterManage.AddMonsterToActiveListServerRpc(boss);

        // If this is boss 1, set up health check for when it reaches 0
        if (bossType == 1)
        {
            PumpkinBoss pumpkinBoss = boss.GetComponent<PumpkinBoss>();
            if (pumpkinBoss != null)
            {
                StartCoroutine(CheckBossHealth(pumpkinBoss));
            }
        }
    }

    // Coroutine to monitor boss health and update death position
    // Coroutine to monitor boss health and update death position
    private IEnumerator CheckBossHealth(PumpkinBoss boss)
    {
        if (boss == null)
        {
            yield break; // Exit if the boss is already destroyed or null
        }

        while (boss.health > 0)
        {
            yield return null; // Wait for the next frame
        }

        // Boss health is 0, ensure the boss is still valid before accessing position
        if (boss != null)
        {
            boss1DeathPosition = boss.transform.position; // Save the death position of Boss 1
            Debug.Log("Boss1 died at position: " + boss1DeathPosition); // Debug log to verify position
        }
    }



    private Vector3 GetRandomSpawnPosition()
    {
        float xRange = 5f; // Adjust range for bosses as needed
        float yRange = 5f;
        float randomX = Random.Range(-xRange, xRange);
        float randomY = Random.Range(-yRange, yRange);

        return new Vector3(randomX, randomY, 0f);
    }
}
