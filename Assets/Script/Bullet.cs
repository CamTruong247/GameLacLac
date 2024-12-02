using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
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
        yield return new WaitForSeconds(5);
        // Despawn the bullet after 5 seconds if it's spawned on the network
        if (IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.up * 10;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Get the NetworkObject from the collided enemy
            NetworkObject enemyNetworkObject = collision.gameObject.GetComponent<NetworkObject>();

            // Ensure the enemy NetworkObject is spawned
            if (enemyNetworkObject != null && enemyNetworkObject.IsSpawned)
            {
                // Kiểm tra nếu đối tượng là Werewolf
                werewolfmovement werewolfMovement = collision.gameObject.GetComponent<werewolfmovement>();
                if (werewolfMovement != null)
                {
                    werewolfMovement.UpdateHealthServerRpc(3);
                }

                // Kiểm tra nếu đối tượng là Slime
                SlimeMovement slimeMovement = collision.gameObject.GetComponent<SlimeMovement>();
                if (slimeMovement != null)
                {
                    slimeMovement.UpdateHealthServerRpc(3);
                }

                // Kiểm tra nếu đối tượng là Golem
                GolemBoss golemMovement = collision.gameObject.GetComponent<GolemBoss>();
                if (golemMovement != null)
                {
                    golemMovement.UpdateHealthServerRpc(3);
                }

                // Kiểm tra nếu đối tượng là Slime King
                SlimeKingMovement slimeKingMovement = collision.gameObject.GetComponent<SlimeKingMovement>();
                if (slimeKingMovement != null)
                {
                    slimeKingMovement.UpdateHealthServerRpc(3);
                }
                PumpkinBoss pumpkinboss =collision.gameObject.GetComponent<PumpkinBoss>();
                if(pumpkinboss != null)
                {
                    pumpkinboss.UpdateHealthServerRpc(3);
                }
                Phase2pumpkin phase2pumpkin=collision.gameObject.GetComponent<Phase2pumpkin>();
                if(phase2pumpkin != null)
                {
                    phase2pumpkin.UpdateHealthServerRpc(3);
                }
            }

            // Despawn the bullet after hitting an enemy if on the server
            if (IsServer && GetComponent<NetworkObject>().IsSpawned)
            {
                // Despawn the bullet after hitting an enemy if on the server
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
