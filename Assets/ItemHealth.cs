using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemHealth : NetworkBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.HealthServerRpc(10);
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
