using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // Thêm thư viện Netcode
using UnityEngine;

public class Map1Manager : NetworkBehaviour // Đổi lớp thành NetworkBehaviour để sử dụng Netcode
{
    public GameObject slimePrefab; // Prefab của slime
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
        while (currentWave<3)
        {
            currentWave++;
            monsterManage.SetCurrentWave(currentWave);
            int slimeCount = 0;
            // Chọn số lượng slime cho từng đợt
            if (currentWave == 1)
            {
                slimeCount = 5;
            }
            else if (currentWave == 2)
            {
                slimeCount = 7;
            }
            else if (currentWave == 3)
            {
                slimeCount = 10;
            }

            // Spawn quái vật cho từng đợt
            for (int i = 0; i < slimeCount; i++)
            {
                SpawnMonsterServerRpc(); // Gọi ServerRpc để spawn quái vật
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }
            if (currentWave == 3 && monsterManage.GetActiveEnemiesCount() == 0)
            {
                Debug.Log("Bạn thắng!");
            }
            // Chờ đợt tiếp theo
            yield return new WaitForSeconds(waveDuration);

            // Kiểm tra xem tất cả quái vật đã bị tiêu diệt chưa
           
        }
        if (monsterManage.GetActiveEnemiesCount() == 0)
        {
            Debug.Log("Bạn thắng!");
        }
    }

    // ServerRpc để spawn quái vật
    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonsterServerRpc()
    {
        if (IsServer)
        {
            SpawnMonster();
        }
    }

    /*// ClientRpc để spawn quái vật trên tất cả client
    [ClientRpc]
    private void SpawnMonsterClientRpc()
    {
        SpawnMonster();
    }*/
    private void SpawnMonster()
    {
        // Tạo vị trí ngẫu nhiên để spawn quái vật
        Vector3 randomPosition = GetRandomSpawnPosition();

        // Spawn slime (hoặc các loại quái vật khác nếu cần)
        GameObject monster = Instantiate(slimePrefab, randomPosition, Quaternion.identity);
        NetworkObject networkObject = monster.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        // Thêm quái vật vào danh sách activeEnemies trong MonsterManage
        monsterManage.AddMonsterToActiveListServerRpc(monster);
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
