using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class GolemBoss : NetworkBehaviour
{
    [SerializeField] private Image healthbar;
    [SerializeField] private GameObject attackBallPrefab; // Prefab for the attack ball
    [SerializeField] private GameObject slimePrefab; // Prefab for the slime to summon

    //private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Transform targetPlayer;
    public float detectionRange = 10f;
    public Animator animator;
    private bool isAttacking = false;
    private int attackMode = 1; // 1: Fireball, 2: Spread, 3: Summon
    public float health = 100f; // Updated health
    private float attackCooldown = 2f;

    private int fireballAttackCount = 0; // Count for fireball attacks
    private float summonCooldown = 20f; // Cooldown for summoning slimes
    private float summonTimer = 20f; // Timer for summoning slimes
    private Quaternion originalRotate;
    private MonsterManage monsterManage;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        monsterManage = FindObjectOfType<MonsterManage>();
    }

    private void Update()
    {
        if (IsServer)
        {
            FindNearestPlayer();
            //rb.velocity = Vector2.zero;
            if (targetPlayer != null && !isAttacking)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= detectionRange)
                {
                    TriggerAttack();
                }
            }
            HealthBarServerRpc();

            // Update summon cooldown timer
            if (summonTimer > 0)
            {
                summonTimer -= Time.deltaTime;
            }
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

    [ServerRpc(RequireOwnership = false)]
    private void HealthBarServerRpc()
    {
        HealthBarClientRpc();
    }

    [ClientRpc]
    private void HealthBarClientRpc()
    {
        healthbar.fillAmount = health / 100f; // Update to 100
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float damage)
    {
        UpdateHealthClientRpc(damage);
    }

    [ClientRpc]
    public void UpdateHealthClientRpc(float damage)
    {
        this.health -= damage;
        if (health <= 0)
        {
            RemoveMonsterServerRpc(); // Gọi Remove trên server khi máu về 0
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveMonsterServerRpc()
    {
        monsterManage.HandleMonsterDeathServerRpc(gameObject); // Xử lý cái chết của quái vật
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn(); // Xóa khỏi mạng thay vì dùng Destroy trực tiếp
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        if (summonTimer <= 0 && attackMode != 3)
        {
            attackMode = 3; // Chuyển sang chế độ summon
        }
        switch (attackMode)
        {
            case 1:
                if (fireballAttackCount < 3)
                {
                    PerformFireballAttack();
                    fireballAttackCount++;
                }
                else
                {
                    fireballAttackCount = 0; // Reset count after 3 attacks
                    attackMode = 2; // Switch to spread attack
                }
                break;

            case 2:
                PerformSpreadAttack();
                attackMode = 1; // Switch back to fireball attack
                break;

            case 3:
                // Only summon if cooldown has expired
                {
                    SummonSlimes();
                    summonTimer = summonCooldown;
                    attackMode = 1; 
                }
                break;
        }

        // Wait for the cooldown before allowing the next attack
        yield return new WaitForSeconds(attackCooldown);
        //transform.rotation = originalRotate;
        isAttacking = false;
    }

    private void PerformFireballAttack()
    {
        Vector3 spawnPosition = transform.position;
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        GameObject ball = Instantiate(attackBallPrefab, spawnPosition, Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn();
        ball.GetComponent<Rigidbody2D>().velocity = direction * 5f; // Adjust speed as needed
    }

    private void PerformSpreadAttack()
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = 360f / 12 * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject ball = Instantiate(attackBallPrefab, transform.position, rotation);
            ball.GetComponent<NetworkObject>().Spawn();
            ball.GetComponent<Rigidbody2D>().velocity = rotation * Vector2.right * 5f; // Adjust speed as needed
        }
    }

    private void SummonSlimes()
    {
        Debug.Log("Summoning slimes...");
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 1.5f), 0); // Random spawn position around the boss
            GameObject slime = Instantiate(slimePrefab, spawnPosition, Quaternion.identity);
            slime.GetComponent<NetworkObject>().Spawn();
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

    private void TriggerAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }
}
