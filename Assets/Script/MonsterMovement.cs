using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SlimeMovement : NetworkBehaviour
{
    //public SpriteRenderer avatar;
    [SerializeField] private Image healthbar;

    private float moveSpeed = 2f; // Tốc độ di chuyển của slime
    private Rigidbody2D rb;
    private Transform targetPlayer; // Tham chiếu đến player gần nhất
    private Vector2 direction; // Hướng di chuyển
    public float detectionRange = 10f; // Tầm phát hiện người chơi
    public Animator animator;
    private Quaternion originalRotate;
    private float chaseDirection;

    public float health = 20f;
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

                    // Tính hướng di chuyển về phía player gần nhất
                    direction = (targetPlayer.position - transform.position).normalized;

                    // Gọi hàm di chuyển trên server và đồng bộ với client
                    MoveServerRpc(direction);
                }
                else
                {
                    // Ngừng di chuyển khi ra ngoài tầm phát hiện
                    StopServerRpc();
                }
                transform.rotation = originalRotate;
            }
            HealthBarServerRpc();
        }
        
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue; // Khởi tạo khoảng cách gần nhất
        targetPlayer = null; // Đặt lại tham chiếu đến player gần nhất

        // Tìm player gần nhất
        foreach (GameObject playerObj in players)
        {
            if (playerObj.transform.GetChild(0).gameObject.activeSelf)
            {
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPlayer = playerObj.transform; // Cập nhật player gần nhất
                }
            }
            
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector2 newDirection)
    {
        // Gọi ClientRpc để di chuyển slime
        MoveClientRpc(newDirection);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 newDirection)
    {
        // Cập nhật vận tốc
        rb.velocity = newDirection * moveSpeed;

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
        MoveClientRpc(Vector2.zero);
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
        health -= damage;
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
