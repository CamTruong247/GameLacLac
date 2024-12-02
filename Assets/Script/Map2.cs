using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Map2Manager : NetworkBehaviour
{
    public GameObject slimePrefab; // Prefab của slime
    public GameObject werewolfPrefab; // Prefab của werewolf
    public float waveDuration = 10f; // Thời gian giữa các đợt quái vật
    private int currentWave = 0;

    private MonsterManage monsterManage; // Tham chiếu đến MonsterManage

    private void Start()
    {
        // Tìm đối tượng MonsterManage trong scene
        monsterManage = FindObjectOfType<MonsterManage>();

        if (monsterManage == null)
        {
            Debug.LogError("MonsterManage not found in the scene!");
            return;
        }

        // Bắt đầu spawn các đợt quái vật
        if (IsServer) // Chỉ server mới có thể bắt đầu spawn
        {
            StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWave < 3) // Giới hạn chỉ chạy 3 đợt
        {
            currentWave++;
            monsterManage.SetCurrentWave(currentWave);
            int slimeCount = 0;
            int werewolfCount = 0;

            // Chọn số lượng slime và werewolf cho từng đợt
            if (currentWave == 1)
            {
                slimeCount = 5;
                werewolfCount = 0;
            }
            else if (currentWave == 2)
            {
                slimeCount = 4;
                werewolfCount = 2;
            }
            else if (currentWave == 3)
            {
                slimeCount = 3;
                werewolfCount = 4;
            }

            // Spawn slime
            for (int i = 0; i < slimeCount; i++)
            {
                SpawnMonsterServerRpc(0); // Gọi ServerRpc để spawn slime (0 cho slime)
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }

            // Spawn werewolf
            for (int i = 0; i < werewolfCount; i++)
            {
                SpawnMonsterServerRpc(1); // Gọi ServerRpc để spawn werewolf (1 cho werewolf)
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }
            if (currentWave == 3 && monsterManage.GetActiveEnemiesCount() == 0)
            {
                Debug.Log("Bạn thắng!");
            }
            // Chờ đợt tiếp theo
            yield return new WaitForSeconds(waveDuration);
        }

        if (monsterManage.GetActiveEnemiesCount() == 0)
        {
            Debug.Log("Bạn thắng!");
        }
      
    }

    // ServerRpc để spawn quái vật
    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonsterServerRpc(int monsterType)
    {
        SpawnMonster(monsterType); // Gọi ClientRpc để spawn quái vật trên tất cả client
    }

    // ClientRpc để spawn quái vật trên tất cả client
   /* [ClientRpc]
    private void SpawnMonsterClientRpc()
    {     
    }*/
    private void SpawnMonster(int monsterType)
    {
        GameObject monsterPrefab = null;

        // Chọn prefab dựa trên loại quái vật
        if (monsterType == 0)
        {
            monsterPrefab = slimePrefab; // Slime
        }
        else if (monsterType == 1)
        {
            monsterPrefab = werewolfPrefab; // Werewolf
        }

        // Tạo vị trí ngẫu nhiên để spawn quái vật
        Vector3 randomPosition = GetRandomSpawnPosition();

        // Spawn quái vật
        if (monsterPrefab != null)
        {
            GameObject monster = Instantiate(monsterPrefab, randomPosition, Quaternion.identity);
            NetworkObject networkObject = monster.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }

            // Thêm quái vật vào danh sách activeEnemies trong MonsterManage
            monsterManage.AddMonsterToActiveListServerRpc(monster);
        }
    }
    private Vector3 GetRandomSpawnPosition()
    {
        float xRange = 10f;
        float yRange = 10f;
        float randomX = Random.Range(-xRange, xRange);
        float randomY = Random.Range(-yRange, yRange);

        return new Vector3(randomX, randomY, 0f); // Nếu bạn dùng 2D, z sẽ là 0
    }
}
