using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterManage : MonoBehaviour
{
    private List<GameObject> activeEnemies = new List<GameObject>(); // Danh sách các quái vật đang sống
    private int currentWave = 0; // Biến để theo dõi số wave hiện tại
    private int totalWaves = 3; // Tổng số wave (có thể điều chỉnh theo nhu cầu)
    public GameObject winpanel;
    // Cập nhật wave hiện tại (được gọi từ Map4Manager)
    public void SetCurrentWave(int waveNumber)
    {
        currentWave = waveNumber;
    }
    public void SetTotalWaves(int waves)
    {
        totalWaves = waves;
        Debug.Log("Total waves set to: " + totalWaves); // Kiểm tra giá trị
    }
    // Trả về số lượng quái vật còn lại trong danh sách
    public int GetActiveEnemiesCount()
    {
        return activeEnemies.Count;
    }

    // Phương thức gọi khi một quái vật chết (sử dụng ServerRpc để cập nhật trên server)
    [ServerRpc(RequireOwnership = false)]
    public void HandleMonsterDeathServerRpc(GameObject monster)
    {
        if (activeEnemies.Contains(monster))
        {
            activeEnemies.Remove(monster);
        }

        Debug.Log("Quái vật còn lại trong wave: " + activeEnemies.Count);

        // Kiểm tra nếu tất cả quái vật đã chết và đây là wave cuối cùng
        if (activeEnemies.Count == 0 && currentWave == totalWaves)
        {
            Debug.Log("Bạn đã thắng tất cả các wave!");
            winpanel.SetActive(true);
            
        }
        else if (activeEnemies.Count == 0)
        {
            Debug.Log("Tất cả quái vật trong wave " + currentWave + " đã bị tiêu diệt!");
        }
    }
    

    // Thêm quái vật vào danh sách activeEnemies (có thể là ClientRpc hoặc ServerRpc)
    [ServerRpc(RequireOwnership = false)]
    public void AddMonsterToActiveListServerRpc(GameObject monster)
    {
        if (!activeEnemies.Contains(monster))
        {
            activeEnemies.Add(monster);
        }
    }

    
    
}
