using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class werewolfmovement : NetworkBehaviour
{
    [SerializeField] private Image healthbar;

    private float moveSpeed = 2f;
    private float dashSpeed = 5f;
    private float dashDuration = 0.2f;
    private float dashCooldown = 5f;
    private float lastDashTime;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private Vector2 direction;
    public float detectionRange = 10f;
    public float dashRange = 5f;
    public Animator animator;
    private Quaternion originalRotate;
    public  float health = 20f;

    private bool isDashing = false;
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

                    // Kiểm tra nếu đủ thời gian hồi và trong khoảng dash
                    if (distanceToPlayer <= dashRange && !isDashing && Time.time >= lastDashTime + dashCooldown)
                    {
                        StartCoroutine(DashTowardsPlayer());
                    }
                    else if (!isDashing)
                    {
                        MoveServerRpc(direction); // Di chuyển bình thường khi không dash
                    }
                }
                else
                {
                    StopServerRpc(); // Ngừng di chuyển khi ra ngoài tầm phát hiện
                }

                transform.rotation = originalRotate;
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

    private IEnumerator DashTowardsPlayer()
    {
        isDashing = true;
        lastDashTime = Time.time; // Ghi nhận thời gian dash

        // Tăng tốc độ và dash về phía người chơi
        MoveServerRpc(direction, dashSpeed);

        yield return new WaitForSeconds(dashDuration); // Thời gian dash

        // Kết thúc dash và trở lại tốc độ bình thường
        isDashing = false;
        MoveServerRpc(direction, moveSpeed);
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector2 newDirection, float speed=2f)
    {
        // Gọi ClientRpc để di chuyển slime
        MoveClientRpc(newDirection,speed);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 newDirection, float speed)
    {
        // Cập nhật vận tốc
        rb.velocity = newDirection * speed;

        transform.rotation = originalRotate;
        // Điều chỉnh hướng hiển thị slime
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
        // Dừng di chuyển khi ra ngoài tầm phát hiện
        MoveClientRpc(Vector2.zero,0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.UpdateHealthServerRpc(10);
        }
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
            RemoveMonsterServerRpc(); 
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveMonsterServerRpc()
    {
        monsterManage.HandleMonsterDeathServerRpc(gameObject);
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn(); 
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
        healthbar.fillAmount = health / 20f;
    }
}
