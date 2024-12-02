using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class SlimeKingMovement : NetworkBehaviour
{
    [SerializeField] private Image healthbar;
    [SerializeField] private GameObject attackBallPrefab; // Prefab for the attack ball

    private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Transform targetPlayer;
    private Vector2 direction;
    public float detectionRange = 10f;
    public Animator animator;
    private Quaternion originalRotate;
    private bool isAttacking = false;
    private int attackMode = 1;
    public float health = 40f;
    private float attackCooldown = 2f;
    private MonsterManage monsterManage;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        animator = GetComponent<Animator>();
        originalRotate = transform.rotation;
        monsterManage = FindObjectOfType<MonsterManage>();
    }

    private void Update()
    {
        if (IsServer)
        {
            FindNearestPlayer();

            if (targetPlayer != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= detectionRange)
                {
                    direction = (targetPlayer.position - transform.position).normalized;
                    MoveServerRpc(direction);

                    // Trigger attack if in range
                    if (distanceToPlayer <= detectionRange)
                    {
                        TriggerAttack();
                    }
                }
                else
                {
                    StopServerRpc();
                }
            }
            HealthBarServerRpc();
        }
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        targetPlayer = null;

        foreach (GameObject playerObj in players)
        {
            if (playerObj.transform.GetChild(0).gameObject.activeSelf)
            {
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPlayer = playerObj.transform;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector2 newDirection)
    {
        MoveClientRpc(newDirection);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 newDirection)
    {
        rb.velocity = newDirection * moveSpeed;
        transform.rotation = originalRotate;

        if (newDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (newDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopServerRpc()
    {
        MoveClientRpc(Vector2.zero);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealthBarServerRpc()
    {
        HealthBarClientRpc();
    }

    [ClientRpc]
    private void HealthBarClientRpc()
    {
        healthbar.fillAmount = health / 40f;
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
            DropItem item = gameObject.GetComponent<DropItem>();
            item.DropItemServerRpc();
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

        // Choose the attack pattern based on the current mode
        if (attackMode == 1)
        {
            PerformAttackPattern(new float[] { 0, 90, 180, 270 });
            attackMode = 2;
        }
        else
        {
            PerformAttackPattern(new float[] { 45, 135, 225, 315 });
            attackMode = 1;
        }

        // Wait for the cooldown before allowing movement and attack again
        yield return new WaitForSeconds(attackCooldown);
        //transform.rotation = originalRotate;
        isAttacking = false;
    }

    private void PerformAttackPattern(float[] angles)
    {

        foreach (float angle in angles)
        {
            Vector3 spawnPosition = transform.position;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject ball = Instantiate(attackBallPrefab, spawnPosition, rotation);
            ball.GetComponent<NetworkObject>().Spawn();
            ball.GetComponent<Rigidbody2D>().velocity = rotation * Vector2.right * 5f; // Adjust speed as needed
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

    // Call this method to trigger the attack periodically
    private void TriggerAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }
}
