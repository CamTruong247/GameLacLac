using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLost : NetworkBehaviour
{
    [SerializeField] public GameObject panel;
    [SerializeField] public GameObject btn;

    private void Update()
    {
        // Tìm tất cả các đối tượng có tag là "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        bool allPlayersInactive = true; // Biến để kiểm tra tất cả "Player" đã bị vô hiệu hóa chưa

        foreach (GameObject player in players)
        {
            // Nếu bất kỳ player nào đang hoạt động, set allPlayersInactive thành false
            if (player.transform.GetChild(0).gameObject.activeSelf)
            {
                allPlayersInactive = false;
                break; // Không cần kiểm tra tiếp nếu đã tìm thấy player đang hoạt động
            }
        }

        // Nếu tất cả player đều bị vô hiệu hóa, hiện panel
        if (allPlayersInactive)
        {
            panel.SetActive(true);
            if (IsServer)
            {
                btn.SetActive(true);
            }
            else
            {
                btn.SetActive(false);
            }
        }
        else
        {
            panel.SetActive(false);
        }
    }
    public void TransScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
