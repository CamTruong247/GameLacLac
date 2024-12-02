using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // Netcode for multiplayer
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    public GameObject[] monsterPrefabs; // Danh sách 3 prefab quái vật
    public ChallengeSettings challengeSettings; // Tham chiếu đến ChallengeSettings (ScriptableObject)

    private float waveDuration; // Thời gian giữa các wave
    private int monsterPerWave; // Số lượng quái mỗi wave
    private int numberOfWaves; // Số wave
    private int currentWave = 0;

    private MonsterManage monsterManage;

    private void Start()
    {
        // Lấy giá trị từ ChallengeSettings
        if (challengeSettings != null)
        {
            waveDuration = challengeSettings.waveDuration;
            monsterPerWave = challengeSettings.monsterPerWave;
            numberOfWaves = challengeSettings.numberOfWaves;
            
        }
        else
        {
            Debug.LogError("ChallengeSettings không được gán!");
            return;
        }

        // Tìm đối tượng MonsterManage trong scene
        monsterManage = FindObjectOfType<MonsterManage>();
        monsterManage.SetTotalWaves(numberOfWaves);
        if (monsterManage == null)
        {
            Debug.LogError("MonsterManage not found in the scene!");
            return;
        }

        // Bắt đầu spawn nếu là server
        if (IsServer)
        {
            StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWave < numberOfWaves) // Điều kiện lặp dựa vào số wave tối đa
        {
            currentWave++;
            monsterManage.SetCurrentWave(currentWave);

            Debug.Log($"Wave {currentWave} bắt đầu với {monsterPerWave} quái vật!");

            // Spawn số lượng quái vật cho wave hiện tại
            for (int i = 0; i < monsterPerWave; i++)
            {
                SpawnMonsterServerRpc(); // Spawn quái vật
                yield return new WaitForSeconds(0.3f); // Delay giữa mỗi lần spawn quái
            }

            // Chờ trước khi bắt đầu wave tiếp theo
            yield return new WaitForSeconds(waveDuration);

            // Kiểm tra nếu tất cả quái vật đã bị tiêu diệt
            if (currentWave == numberOfWaves && monsterManage.GetActiveEnemiesCount() == 0)
            {
                Debug.Log("Bạn thắng!");
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonsterServerRpc()
    {
        if (IsServer)
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        // Lấy vị trí ngẫu nhiên để spawn quái vật
        Vector3 randomPosition = GetRandomSpawnPosition();

        // Lấy ngẫu nhiên một prefab từ danh sách monsterPrefabs
        GameObject randomPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

        // Spawn quái vật
        GameObject monster = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
        NetworkObject networkObject = monster.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        // Thêm quái vật vào danh sách activeEnemies
        monsterManage.AddMonsterToActiveListServerRpc(monster);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Tạo một vị trí ngẫu nhiên trong phạm vi X và Y
        float xRange = 5f;
        float yRange = 5f;
        float randomX = Random.Range(-xRange, xRange);
        float randomY = Random.Range(-yRange, yRange);
        return new Vector3(randomX, randomY, 0f); // Z = 0 cho game 2D
    }
}
