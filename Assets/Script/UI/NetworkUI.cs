using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private GameObject failScene;

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetIpAddress(string input)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = input;
    }

    public void StartGame()
    {
        NetworkManager.Singleton.StartHost();
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void JoinGame()
    {
        SetIpAddress(input.text);
        //if (NetworkManager.Singleton.ConnectedClientsIds.Count < 6)
        //{
            NetworkManager.Singleton.StartClient();
            //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        //}
        //else
        //{
        //    failScene.SetActive(true);
        //    failScene.transform.GetChild(0).gameObject.SetActive(true);
        //    failScene.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Server Is Full";
        //    NetworkManager.Singleton.Shutdown();
        //}
    }

}
