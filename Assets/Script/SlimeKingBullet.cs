using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SlimeKingBullet : NetworkBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    private void Start()
    {
        if(IsServer)
        {
            StartCoroutine(Deplay());
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator Deplay()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(3);

            GetComponent<NetworkObject>().Despawn();
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.UpdateHealthServerRpc(10);
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();

            }
        }
    }
}
