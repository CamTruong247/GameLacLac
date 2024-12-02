using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Openmap1 : NetworkBehaviour
{
    public void onclick()
    {
        if (IsServer)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 2;
            NetworkManager.Singleton.SceneManager.LoadScene("Map" + nextSceneIndex, LoadSceneMode.Single);

        }
    }
}
