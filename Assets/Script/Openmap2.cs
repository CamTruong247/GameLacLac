using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Openmap2 : NetworkBehaviour
{
    public void onclick()
    {
        if (IsServer)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 3;
            NetworkManager.Singleton.SceneManager.LoadScene("Map" + nextSceneIndex, LoadSceneMode.Single);

        }
    }
}
