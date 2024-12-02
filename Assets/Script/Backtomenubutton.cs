using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Backtomenubutton : NetworkBehaviour
{
    public GameObject hidemenu;
    public void onclick()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            hidemenu.SetActive(false);
        }
    }
}
