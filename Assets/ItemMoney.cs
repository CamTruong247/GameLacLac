using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemMoney : NetworkBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        int random = UnityEngine.Random.Range(0, 10);
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.Money(random);
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
