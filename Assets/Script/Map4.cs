using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Map4Manager : NetworkBehaviour
{
    public GameObject werewolfPrefab; // Prefab của werewolf
    public GameObject slimeKingPrefab; // Prefab của slimeKing
    public GameObject golemBossPrefab; // Prefab của golemBoss
    public GameObject player; // Tham chiếu đến nhân vật người chơi
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

        // Tìm đối tượng nhân vật
        player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
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
            int werewolfCount = 0;
            int slimeKingCount = 0;
            int golemBossCount = 0;

            // Chọn số lượng werewolf, slimeKing và golemBoss cho từng đợt
            if (currentWave == 1)
            {
                werewolfCount = 5;
                slimeKingCount = 0;
                golemBossCount = 0;
            }
            else if (currentWave == 2)
            {
                werewolfCount = 2;
                slimeKingCount = 5;
                golemBossCount = 0;
            }
            else if (currentWave == 3)
            {
                werewolfCount = 0;
                slimeKingCount = 3;
                golemBossCount = 2;
            }

            // Spawn werewolf
            for (int i = 0; i < werewolfCount; i++)
            {
                SpawnMonsterServerRpc(0); // Gọi ServerRpc để spawn werewolf (0 cho werewolf)
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }

            // Spawn slimeKing
            for (int i = 0; i < slimeKingCount; i++)
            {
                SpawnMonsterServerRpc(1); // Gọi ServerRpc để spawn slimeKing (1 cho slimeKing)
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }

            // Spawn golemBoss (vị trí gần nhân vật hơn)
            for (int i = 0; i < golemBossCount; i++)
            {
                SpawnMonsterServerRpc(2); // Gọi ServerRpc để spawn golemBoss (2 cho golemBoss)
                yield return new WaitForSeconds(0.3f); // Delay giữa các lần spawn quái vật
            }
            
            // Chờ đợt tiếp theo
            yield return new WaitForSeconds(waveDuration);
        }

        // Sau khi tất cả các đợt quái vật đã spawn, kiểm tra nếu tất cả quái vật đã chết
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

    /*// ClientRpc để spawn quái vật trên tất cả client
    [ClientRpc]
    private void SpawnMonsterClientRpc() { }*/
    private void SpawnMonster (int monsterType)
    {
        GameObject monsterPrefab = null;

        // Chọn prefab dựa trên loại quái vật
        if (monsterType == 0)
        {
            monsterPrefab = werewolfPrefab; // Werewolf
        }
        else if (monsterType == 1)
        {
            monsterPrefab = slimeKingPrefab; // SlimeKing
        }
        else if (monsterType == 2)
        {
            monsterPrefab = golemBossPrefab; // GolemBoss
        }

        // Tạo vị trí ngẫu nhiên để spawn quái vật
        Vector3 randomPosition = (monsterType == 2) ? GetCloserSpawnPosition() : GetRandomSpawnPosition();

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

    // Tạo vị trí ngẫu nhiên cho quái vật (cho các loại quái vật khác ngoài GolemBoss)
    private Vector3 GetRandomSpawnPosition()
    {
        float xRange = 10f;
        float yRange = 10f;
        float randomX = Random.Range(-xRange, xRange);
        float randomY = Random.Range(-yRange, yRange);

        return new Vector3(randomX, randomY, 0f); // Nếu bạn dùng 2D, z sẽ là 0
    }

    // Tạo vị trí gần người chơi cho GolemBoss
    private Vector3 GetCloserSpawnPosition()
    {
        float spawnDistance = 5f; // Khoảng cách spawn gần người chơi

        // Lấy vị trí của nhân vật người chơi
        Vector3 playerPosition = player.transform.position;

        // Tạo một vị trí ngẫu nhiên gần người chơi trong phạm vi spawnDistance
        float randomX = Random.Range(playerPosition.x - spawnDistance, playerPosition.x + spawnDistance);
        float randomY = Random.Range(playerPosition.y - spawnDistance, playerPosition.y + spawnDistance);

        return new Vector3(randomX, randomY, 0f); // Nếu bạn dùng 2D, z sẽ là 0
    }
}
