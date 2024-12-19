using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private GameObject bullet; // GameObject cho đạn
    [SerializeField] private GameObject laser; // GameObject cho kỹ năng laser
    [SerializeField] private Transform shootingPoint; // Điểm bắn
    public Animator animator; // Animator để điều khiển hoạt ảnh
    public SkillData skillData; // Tham chiếu tới SkillData
    public SkillData skillDataQ;
    private float skillQCooldown = 10f;
    private float lastSkillQTime = -10f;
    private bool isAutoAttackActive = false; // Trạng thái tấn công tự động

    public Camera camera;

    void Update()
    {
        // Kiểm tra nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            AttackServerRpc();
        }

        // Kiểm tra nhấn phím R để kích hoạt tấn công tự động
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isAutoAttackActive)
            {
                isAutoAttackActive = true;
                StartCoroutine(AutoAttack());
            }
            else
            {
                isAutoAttackActive = false;
            }
        }

        // Kiểm tra nhấn phím Q để sử dụng kỹ năng
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseSkillServerRpc(); // Gọi hàm sử dụng kỹ năng
        }
    }

    /*private void FixedUpdate()
    {
        RotationWeapon(); // Xoay vũ khí theo chuột
    }*/

    private IEnumerator AutoAttack()
    {
        while (isAutoAttackActive)
        {
            AttackServerRpc();
            yield return new WaitForSeconds(0.3f); // Điều chỉnh tốc độ tấn công
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc()
    {
        if (IsServer)
        {
            AttackClientRpc();

            // Kiểm tra kỹ năng có mở khóa không
            if (skillData != null && skillData.isSkillUnlocked)
            {
                // Nếu kỹ năng mở khóa, tạo ra 2 viên đạn song song
                FireTwoBullets();
            }
            else
            {
                // Nếu kỹ năng chưa mở khóa, chỉ bắn 1 viên đạn
                FireOneBullet();
            }
        }
    }

    [ClientRpc]
    private void AttackClientRpc()
    {
        animator.SetTrigger("Attack"); // Gán trigger cho hoạt ảnh tấn công
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseSkillServerRpc()
    {
        if (IsServer)
        {
            // Kiểm tra kỹ năng Q có mở khóa không và thời gian cooldown
            if (skillDataQ != null && skillDataQ.isSkillUnlocked && Time.time - lastSkillQTime >= skillQCooldown)
            {
                // Cập nhật thời gian sử dụng kỹ năng Q
                lastSkillQTime = Time.time;

             
                Vector3 laserPosition = transform.position;
                var l = Instantiate(laser, laserPosition, Quaternion.identity);
                l.GetComponent<NetworkObject>().Spawn();

              
                StartCoroutine(DestroyLaserAfterDelay(l, 1f));
            }
            else
            {
                
                Debug.Log("Skill Q not unlocked or still on cooldown");
            }
        }
    }
    // Coroutine để hủy laser sau một khoảng thời gian nhất định
    private IEnumerator DestroyLaserAfterDelay(GameObject laser, float delay)
    {
        yield return new WaitForSeconds(delay); // Chờ trong khoảng thời gian delay
        Destroy(laser); // Hủy laser
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; // Vô hiệu hóa script nếu không phải chủ sở hữu
            return;
        }
    }
    private Transform FindClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        // Get all enemies in the scene
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    private void RotationWeapon(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = rotation;
    }

    private void FireOneBullet()
    {
        // Fire 1 bullet towards the nearest enemy
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            RotationWeapon(closestEnemy.position); // Rotate towards the target
            var bulletInstance = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
            bulletInstance.GetComponent<NetworkObject>().Spawn();
            bulletInstance.GetComponent<Rigidbody2D>().velocity = (closestEnemy.position - transform.position).normalized * 7f; // Adjust bullet speed
        }
    }

    private void FireTwoBullets()
    {
        // Fire 2 bullets with a small offset
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            RotationWeapon(closestEnemy.position); // Rotate towards the target

            var bullet1 = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
            var bullet2 = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);

            // Apply slight angle offset for the second bullet
            float angleOffset = 5f;
            float angle = shootingPoint.rotation.eulerAngles.z + angleOffset;
            bullet2.transform.rotation = Quaternion.Euler(0, 0, angle);

            bullet1.GetComponent<NetworkObject>().Spawn();
            bullet2.GetComponent<NetworkObject>().Spawn();

            bullet1.GetComponent<Rigidbody2D>().velocity = (closestEnemy.position - transform.position).normalized * 7f;
            bullet2.GetComponent<Rigidbody2D>().velocity = (closestEnemy.position - transform.position).normalized * 7f;
        }
    }
}
