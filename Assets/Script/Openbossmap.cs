using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Openbossmap : NetworkBehaviour
{

    public void onclick()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Boss", LoadSceneMode.Single);
        }
    }
}
