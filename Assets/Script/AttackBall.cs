using UnityEngine;
using Unity.Netcode;
using System.Collections;
public class AttackBall : NetworkBehaviour
{
    private float lifespan = 5f; // Duration the ball will exist before being destroyed
    private float speed = 10f; // Speed of the attack ball
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Destroy the ball after the lifespan expires
        Destroy(gameObject, lifespan);
    }

    private void Update()
    {
        // Continue moving the ball forward in the direction it's facing
        rb.velocity = transform.up * speed;
    }
    private Transform targetEnemy; // Lưu trữ kẻ thù mục tiêu
  

    // Khởi tạo với kẻ thù và hướng bắn
    public void Initialize(Transform enemy, Vector2 attackDirection)
    {
        targetEnemy = enemy;
        StartCoroutine(ChaseEnemy(attackDirection));
    }

    private IEnumerator ChaseEnemy(Vector2 attackDirection)
    {
        // Đặt vận tốc viên đạn theo hướng đã cho ban đầu
        rb.velocity = attackDirection * speed;

        while (targetEnemy != null)
        {
            // Cập nhật hướng bắn để theo dõi kẻ thù
            Vector2 directionToEnemy = (targetEnemy.position - transform.position).normalized;
            rb.velocity = directionToEnemy * speed;

            // Kiểm tra khoảng cách đến kẻ thù
            if (Vector2.Distance(transform.position, targetEnemy.position) < 0.5f)
            {
              
                break;
            }

            yield return null; // Chờ đến khung hình tiếp theo
        }

       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Kiểm tra nếu đối tượng là Werewolf
            werewolfmovement werewolfMovement = collision.gameObject.GetComponent<werewolfmovement>();
            if (werewolfMovement != null)
            {
                werewolfMovement.UpdateHealthServerRpc(10);
            }

            // Kiểm tra nếu đối tượng là Slime
            SlimeMovement slimeMovement = collision.gameObject.GetComponent<SlimeMovement>();
            if (slimeMovement != null)
            {
                slimeMovement.UpdateHealthServerRpc(10);
            }

            // Kiểm tra nếu đối tượng là Golem
            GolemBoss golemMovement = collision.gameObject.GetComponent<GolemBoss>();
            if (golemMovement != null)
            {
                golemMovement.UpdateHealthServerRpc(10);
            }

            // Kiểm tra nếu đối tượng là Slime King
            SlimeKingMovement slimeKingMovement = collision.gameObject.GetComponent<SlimeKingMovement>();
            if (slimeKingMovement != null)
            {
                slimeKingMovement.UpdateHealthServerRpc(10);
            }
            PumpkinBoss pumpkinboss = collision.gameObject.GetComponent<PumpkinBoss>();
            if (pumpkinboss != null)
            {
                pumpkinboss.UpdateHealthServerRpc(10);
            }
            Phase2pumpkin phase2pumpkin = collision.gameObject.GetComponent<Phase2pumpkin>();
            if (phase2pumpkin != null)
            {
                phase2pumpkin.UpdateHealthServerRpc(10);
            }
            // Despawn the bullet after hitting an enemy if on the server
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
            //Destroy(gameObject);
        }
        /*else if (collision.CompareTag("Wall"))
        {
            // Destroy the ball upon hitting a wall
            Destroy(gameObject);
        }*/
    }
}
