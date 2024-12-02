using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : NetworkBehaviour
{
    public GameObject slimePrefab;
    public GameObject werewolfPrefab;
    public GameObject slimeKingPrefab;
    public GameObject golemBossPrefab;
    public float waveDuration = 30f; 
    public int monstersPerWave = 12; 
    private int currentWave = 0; 

    private int slimeKingCount = 0; 
    private int golemCount = 0;     
    private bool isWaveInProgress = false;
    private MonsterManage monsterManage;


    private void Start()
    {
        monsterManage = FindObjectOfType<MonsterManage>();
        if (IsServer)
        {
            StartCoroutine(SpawnWave()); // Server bắt đầu spawn
        }
    }

    private IEnumerator SpawnWave()
    {
        while (true)
        {
            if (!isWaveInProgress)
            {
                isWaveInProgress = true;
                currentWave++;

                // Gọi RPC để spawn quái vật trên server và client
                SpawnMonstersServerRpc(currentWave);

                // Đợi trong 30 giây cho mỗi lượt
                yield return new WaitForSeconds(waveDuration);

                isWaveInProgress = false;
            }

            yield return null;
        }
    }

    // ServerRPC để spawn quái vật trên server
    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonstersServerRpc(int wave)
    {
        SpawnMonsters(wave);
    }

    // ClientRpc để spawn quái vật trên tất cả client
    /* [ClientRpc]
     private void SpawnMonstersClientRpc(int wave)
     {
         SpawnMonsters(wave);
     }*/

    private void SpawnMonsters(int wave)
    {
        Debug.Log("Monsters Per Wave: " + monstersPerWave);
        if (monstersPerWave == 0)
        {
            Debug.LogError("Number of monsters per wave is zero!");
            return;
        }

        for (int i = 0; i < monstersPerWave; i++)
        {
            // Chọn quái vật theo độ khó
            GameObject monsterToSpawn = GetMonsterByDifficulty(wave);

            // Kiểm tra nếu là Golem Boss
            Vector3 spawnPosition;
            if (monsterToSpawn == golemBossPrefab)
            {
                // Golem Boss xuất hiện ở vị trí trung tâm
                spawnPosition = Vector3.zero;
            }
            else
            {
                // Các quái vật khác xuất hiện tại vị trí ngẫu nhiên
                spawnPosition = GetRandomSpawnPosition();
            }

            if (monsterToSpawn != null)
            {
                // Instantiate quái vật tại vị trí tương ứng
                GameObject monster = Instantiate(monsterToSpawn, spawnPosition, Quaternion.identity);

                // Spawn quái vật trên mạng (NetworkObject)
                NetworkObject networkObject = monster.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
                }
                monsterManage.AddMonsterToActiveListServerRpc(monster);
            }
        }
    }


    private Vector3 GetRandomSpawnPosition()
    {
        float xRange = 10f;
        float yRange = 10f;
        float randomX = Random.Range(-xRange, xRange);
        float randomY = Random.Range(-yRange, yRange);

        return new Vector3(randomX, randomY, 0f); 
    }
    private GameObject GetMonsterByDifficulty(int wave)
    {
        
        if (wave >= 4 && golemCount < 1)
        {
            
            golemCount++;
            return golemBossPrefab;
        }
        else if (wave >= 3 && slimeKingCount < 3)
        {
           
            slimeKingCount++;
            return slimeKingPrefab;
        }


        return Random.Range(0, 2) == 0 ? slimePrefab : werewolfPrefab;
    }

  
    
}
