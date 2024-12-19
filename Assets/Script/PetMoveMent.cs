using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PetMovement : NetworkBehaviour
{
    [SerializeField] private GameObject attackBallPrefab;
    [SerializeField] private Transform player;
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
    private bool isFollowing = true;
    [SerializeField] private float followDistance = 2f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalRotate = transform.rotation;
    }
    private void Start()
    {
        if (IsServer)
        {
            // Tìm kiếm nhân vật theo tag hoặc Netcode's ownership
            player = FindPlayerTransform();
        }
    }
    private Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            return player.transform;
        }
        return null;
    }
    private void Update()
    {
        if (IsServer)
        {
            if (player == null)
            {
                player = FindPlayerTransform(); 
            }

            if (player != null && isFollowing)
            {
                FollowPlayer(); 
            }

            FindNearestEnemy(); 

            if (targetEnemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

               
                if (distanceToEnemy <= attackRange && !isAttacking)
                {
                    StartCoroutine(AttackCoroutine());
                }
            }

            transform.rotation = originalRotate; 
            Invoke(nameof(DestroySelf), lifetime); 
        }
    }

    private void DestroySelf()
    {
        if (IsServer)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > 0.1f)
        {
            direction = (player.position - transform.position).normalized;
            MoveServerRpc(direction);
        }
        else
        {
            StopServerRpc();
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
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                targetEnemy = enemyObj.transform;
            }
        }
    }
    private void FollowPlayer()
    {
     
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > followDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;

           
            if (direction.x > 0)
                transform.localScale = new Vector3(-0.12f, 0.12f, 1);
            else if (direction.x < 0)
                transform.localScale = new Vector3(0.12f, 0.12f, 1);
        }
        else
        {
            rb.velocity = Vector2.zero; // Dừng lại nếu đã đủ gần
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
            transform.localScale = new Vector3(0.12f, 0.12f, 1); // Quay phải
        }
        else if (newDirection.x < 0)
        {
            transform.localScale = new Vector3(-0.12f, 0.12f, 1); // Quay trái
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

        // Đợi cooldown
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}
