using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test : NetworkBehaviour
{
    public GameObject mapbutton;
   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        mapbutton.SetActive(false);
        if (collision.gameObject.CompareTag("Player"))
        {
            if (IsServer)
            {
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                NetworkManager.Singleton.SceneManager.LoadScene("Map"+ nextSceneIndex, LoadSceneMode.Single);

            }

        }

    }

}
