using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PumpkinBoss : NetworkBehaviour
{
    [SerializeField] private Image healthbar;
    [SerializeField] private GameObject projectilePrefab; // Assign your projectile prefab here
    [SerializeField] private float moveSpeed = 5f; // Speed for the dash movement
    [SerializeField] private float chargeCooldown = 3f;
     public float health = 100f;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    //private bool isCharging = false;
    private bool canAttack = true;
    private Vector2 chargeDirection;
    private Quaternion originalRotation;
    private MonsterManage monsterManage; // Reference to MonsterManager

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalRotation = transform.rotation;
        monsterManage = FindObjectOfType<MonsterManage>(); // Find MonsterManager in the scene
    }

    private void Update()
    {
        if (IsServer)
        {
            FindNearestPlayer();
            if (targetPlayer != null && canAttack)
            {
                StartCoroutine(ChargeAndShootAttack());
            }
           /* else if (targetPlayer != null) // Chase player if not charging
            {
                Vector2 direction = (targetPlayer.position - transform.position).normalized;
                Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPlayer.position, moveSpeed * Time.deltaTime);
                rb.MovePosition(newPosition); 
            }*/
        }
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        targetPlayer = null;

        foreach (GameObject playerObj in players)
        {
            float distance = Vector2.Distance(transform.position, playerObj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetPlayer = playerObj.transform;
            }
        }
    }

    private IEnumerator ChargeAndShootAttack()
    {
        canAttack = false;
        ChargeTowardsPlayer();

        yield return new WaitForSeconds(1.5f); // Adjust timing for charge

        StopCharge();
        ShootProjectiles();

        yield return new WaitForSeconds(chargeCooldown); // Cooldown before next attack
        canAttack = true;
    }

    private void ChargeTowardsPlayer()
    {
        if (targetPlayer == null) return;
        rb.bodyType = RigidbodyType2D.Kinematic;
        chargeDirection = (targetPlayer.position - transform.position).normalized;
        rb.velocity = chargeDirection * moveSpeed;
        //isCharging = true;
    }

    private void StopCharge()
    {
        rb.velocity = Vector2.zero;
        //isCharging = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

    }

    private void ShootProjectiles()
    {
        if (projectilePrefab == null || targetPlayer == null) return;

        // Calculate the angle towards the player
        Vector2 directionToPlayer = (targetPlayer.position - transform.position).normalized;
        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Spawn and aim the three projectiles
        SpawnProjectile(angleToPlayer); // Center projectile aimed at player
        SpawnProjectile(angleToPlayer + 20f); // Right offset projectile
        SpawnProjectile(angleToPlayer - 20f); // Left offset projectile
    }

    private void SpawnProjectile(float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject projectile = Instantiate(projectilePrefab, transform.position, rotation);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.GetComponent<Rigidbody2D>().velocity = rotation * Vector2.right * 5f; // Set speed as needed
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float damage)
    {
       UpdateHealthClientRpc(damage);
    }
    [ClientRpc]
    public void UpdateHealthClientRpc(float damage)
    {
        health -= damage;
        if (healthbar != null)
        {
            healthbar.fillAmount = health / 100f;
        }

        if (health <= 0)
        {
            DropAndRemoveMonster();
        }
    }
    private void DropAndRemoveMonster()
    {
        DropItem item = gameObject.GetComponent<DropItem>();
        item.DropItemServerRpc(); // Drop item if the DropItem script is attached

        RemoveMonsterServerRpc(); // Remove monster when health is zero
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveMonsterServerRpc()
    {
        // Handle monster death through MonsterManager
        if (monsterManage != null)
        {
            monsterManage.HandleMonsterDeathServerRpc(gameObject); // Notify MonsterManager
        }

        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn(); // Despawn from network
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.UpdateHealthServerRpc(10);
        }
    }
}
