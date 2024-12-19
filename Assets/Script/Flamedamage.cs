﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Flamedamage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Kiểm tra nếu đối tượng là Werewolf
            werewolfmovement werewolfMovement = collision.gameObject.GetComponent<werewolfmovement>();
            if (werewolfMovement != null)
            {
                werewolfMovement.UpdateHealthServerRpc(5);
            }

            // Kiểm tra nếu đối tượng là Slime
            SlimeMovement slimeMovement = collision.gameObject.GetComponent<SlimeMovement>();
            if (slimeMovement != null)
            {
                slimeMovement.UpdateHealthServerRpc(5);
            }

            // Kiểm tra nếu đối tượng là Golem
            GolemBoss golemMovement = collision.gameObject.GetComponent<GolemBoss>();
            if (golemMovement != null)
            {
                golemMovement.UpdateHealthServerRpc(5);
            }

            // Kiểm tra nếu đối tượng là Slime King
            SlimeKingMovement slimeKingMovement = collision.gameObject.GetComponent<SlimeKingMovement>();
            if (slimeKingMovement != null)
            {
                slimeKingMovement.UpdateHealthServerRpc(5);
            }
            PumpkinBoss pumpkinboss = collision.gameObject.GetComponent<PumpkinBoss>();
            if (pumpkinboss != null)
            {
                pumpkinboss.UpdateHealthServerRpc(5);
            }
            Phase2pumpkin phase2pumpkin = collision.gameObject.GetComponent<Phase2pumpkin>();
            if (phase2pumpkin != null)
            {
                phase2pumpkin.UpdateHealthServerRpc(5);
            }
            // Despawn the bullet after hitting an enemy if on the server

        }
    }
}