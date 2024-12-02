using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemBomb : NetworkBehaviour
{
    public int damageAmount = 10;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Khi người chơi chạm vào quả bom, kiểm tra và gây sát thương lên tất cả các quái vật
            if (IsServer) // Chỉ server mới có thể gây sát thương và quản lý các đối tượng
            {
                DamageAllMonsters();

                // Hủy quả bom sau khi gây sát thương
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void DamageAllMonsters()
    {
        // Tìm tất cả các đối tượng quái vật trên sân và gây sát thương
        werewolfmovement[] werewolves = FindObjectsOfType<werewolfmovement>();
        foreach (var werewolf in werewolves)
        {
            werewolf.UpdateHealthServerRpc(damageAmount);
        }

        SlimeMovement[] slimes = FindObjectsOfType<SlimeMovement>();
        foreach (var slime in slimes)
        {
            slime.UpdateHealthServerRpc(damageAmount);
        }

        GolemBoss[] golems = FindObjectsOfType<GolemBoss>();
        foreach (var golem in golems)
        {
            golem.UpdateHealthServerRpc(damageAmount);
        }

        SlimeKingMovement[] slimeKings = FindObjectsOfType<SlimeKingMovement>();
        foreach (var slimeKing in slimeKings)
        {
            slimeKing.UpdateHealthServerRpc(damageAmount);
        }
    }
}
