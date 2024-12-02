using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Openmap3 : NetworkBehaviour
{
    // Start is called before the first frame update
    public void onclick()
    {
        if (IsServer)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 4;
            NetworkManager.Singleton.SceneManager.LoadScene("Map" + nextSceneIndex, LoadSceneMode.Single);

        }
    }
}