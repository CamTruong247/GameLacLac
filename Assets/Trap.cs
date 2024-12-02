using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject attackpoint;

    private float damage = 4f;
    public float radius;
    public LayerMask layer;
    public float cooldownattack = 0;

    private void Update()
    {
        cooldownattack += Time.deltaTime;
        if (cooldownattack > 1)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(attackpoint.transform.position, radius, attackpoint.transform.position, 0f, layer);
            foreach (RaycastHit2D hit in hits)
            {
                AttackServerRpc();
                PlayerStats playerStats = hit.transform.GetComponent<PlayerStats>();
                playerStats.UpdateHealthServerRpc(damage);
                cooldownattack = 0;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc()
    {
        AttackClientRpc();
    }

    [ClientRpc]
    private void AttackClientRpc()
    {
        animator.SetTrigger("Attack"); // Gán trigger cho hoạt ảnh tấn công
    }
    //private void OnDrawGizmosSelected()
    //{
    //    Handles.color = Color.red;
    //    Handles.DrawWireDisc(attackpoint.transform.position, attackpoint.transform.forward, radius);
    //}
}
