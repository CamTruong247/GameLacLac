using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PetMovement : NetworkBehaviour
{
    [SerializeField] private GameObject attackBallPrefab;
    public Animator animator; // Animator for pet animations
    [SerializeField] private float lifetime = 15.0f;
    private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Transform targetEnemy;
    private Vector2 direction;
    public float detectionRange = 5f;
    private float attackRange = 5f;
    private float attackCooldown = 2f;
    private bool isAttacking = false;
    private Quaternion originalRotate;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Get the Animator component
        originalRotate = transform.rotation;
    }

    private void Update()
    {
        if (IsServer)
        {
            FindNearestEnemy();

            if (targetEnemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

                if (distanceToEnemy <= detectionRange && !isAttacking)
                {
                    // Move towards the enemy
                    direction = (targetEnemy.position - transform.position).normalized;
                    animator.SetBool("isRunning", true); // Start moving animation
                    MoveServerRpc(direction);

                    // Attack if within range
                    if (distanceToEnemy <= attackRange)
                    {
                        StartCoroutine(AttackCoroutine());
                    }
                }
                else
                {
                    animator.SetBool("isRunning", false); // Stop moving animation
                    StopServerRpc();
                }

                transform.rotation = originalRotate;
            }
            else
            {
                animator.SetBool("isRunning", false); // Stop moving if no target
            }
            Invoke(nameof(DestroySelf), lifetime);
        }
    }
    private void DestroySelf()
    {
        // Tự hủy slime sau thời gian tồn tại
        if (IsServer)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
        }
    }
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = float.MaxValue;
        targetEnemy = null;

        foreach (GameObject enemyObj in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetEnemy = enemyObj.transform;
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
            transform.localScale = new Vector3(1, 1, 1); // Facing right
        }
        else if (newDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Facing left
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopServerRpc()
    {
        MoveClientRpc(Vector2.zero);
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        animator.SetBool("isRunning", false); // Dừng animation di chuyển khi tấn công

        // Tìm quái vật gần nhất
        FindNearestEnemy();

        if (targetEnemy != null)
        {
            // Tạo viên đạn và điều chỉnh vị trí khởi tạo
            GameObject ball = Instantiate(attackBallPrefab, transform.position, Quaternion.identity);
            ball.GetComponent<NetworkObject>().Spawn();

            // Tính toán hướng bắn về phía quái vật
            Vector2 attackDirection = (targetEnemy.position - transform.position).normalized;

            // Gọi phương thức khởi tạo của AttackBall
            ball.GetComponent<AttackBall>().Initialize(targetEnemy, attackDirection);
        }

        // Đợi thời gian làm mới (cooldown) trước khi cho phép tấn công tiếp theo
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}
