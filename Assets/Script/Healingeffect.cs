using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Healingeffect : NetworkBehaviour
{
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(Deplay());
        }

    }

    private IEnumerator Deplay()
    {
        yield return new WaitForSeconds(1);
        GetComponent<NetworkObject>().Despawn();
    }
}