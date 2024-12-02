using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class Phase2pumpkin : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab; // For Attack 1
    [SerializeField] private GameObject trapPrefab;       // For Attack 2
    [SerializeField] private GameObject pullPrefab;       // For Attack 3
    [SerializeField] private GameObject laserPrefab;      // For Attack 4
    [SerializeField] private float health = 150f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private Image healthbar;

    private bool canAttack = true;
    private Transform targetPlayer;
    private MonsterManage monsterManage;

    private void Awake()
    {
        monsterManage = FindObjectOfType<MonsterManage>();
    }

    private void Update()
    {
        if (IsServer && canAttack)
        {
            FindNearestPlayer();
            StartCoroutine(AttackSequence());
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
    private IEnumerator AttackSequence()
    {
        canAttack = false;

        int attackType = Random.Range(1, 5); // Randomly choose an attack
        switch (attackType)
        {
            case 1:
                yield return StartCoroutine(AttackPattern1());
                break;
            case 2:
                yield return StartCoroutine(AttackPattern2());
                break;
            case 3:
                yield return StartCoroutine(AttackPattern3());
                break;
            case 4:
                yield return StartCoroutine(AttackPattern4());
                break;
        }

        yield return new WaitForSeconds(attackInterval);
        canAttack = true;
    }

    // Attack 1: Shoots 5 projectiles, one by one, each aiming at the player's latest position
    private IEnumerator AttackPattern1()
    {
        for (int i = 0; i < 5; i++)
        {
            if (targetPlayer != null)
            {
                Vector2 direction = (targetPlayer.position - transform.position).normalized;
                SpawnProjectile(direction);
            }
            yield return new WaitForSeconds(0.2f); // Delay between each projectile
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
            projectile.GetComponent<Rigidbody2D>().velocity = direction * 7f; // Adjust speed as needed
        }
    }

    // Attack 2: Randomly spawn 5 trap tiles around the boss with increased radius
    private IEnumerator AttackPattern2()
    {
        float trapRadius = 5f; // Increased trap radius for wider spawn area

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * trapRadius;
            Vector3 spawnPosition = transform.position + (Vector3)randomOffset;
            GameObject trap = Instantiate(trapPrefab, spawnPosition, Quaternion.identity);
            trap.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForSeconds(0.2f); // Slight delay for effect
        }
    }

    // Attack 3: Spawn a pulling force that pulls the player towards the boss
    private IEnumerator AttackPattern3()
    {
        GameObject pullEffect = Instantiate(pullPrefab, transform.position, Quaternion.identity);
        pullEffect.GetComponent<NetworkObject>().Spawn();

        // Pull the player towards the boss for a set duration
        float pullDuration = 5f;
        float pullSpeed = 5f; // Adjust pull speed as needed
        float timer = 0f;

        while (timer < pullDuration)
        {
            timer += Time.deltaTime;
            FindNearestPlayer();
            if (targetPlayer != null)
            {
                // Move the player gradually closer to the boss position
                targetPlayer.position = Vector2.MoveTowards(
                    targetPlayer.position,
                    transform.position,
                    pullSpeed * Time.deltaTime
                );
            }
            yield return null;
        }
    }

    // Attack 4: Spawn four lasers at 90-degree intervals, then rotate them counterclockwise
    private IEnumerator AttackPattern4()
    {
        float[] angles = { 0f, 90f, 180f, 270f };

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject laser = Instantiate(laserPrefab, transform.position, rotation);
            var networkObject = laser.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // Truyền NetworkObject ID cho ClientRpc để xoay laser
            RotateLaserClientRpc(networkObject.NetworkObjectId, 30f);  // 30f là tốc độ xoay
        }

        yield return new WaitForSeconds(2f); // Delay trước khi tấn công tiếp theo
    }

    [ClientRpc]
    private void RotateLaserClientRpc(ulong networkObjectId, float rotateSpeed)
    {
        GameObject laserObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
        StartCoroutine(RotateLaser(laserObject.transform, rotateSpeed));
    }

    private IEnumerator RotateLaser(Transform laserTransform, float rotateSpeed)
    {
        float duration = 5f; // Thời gian sống của laser (5 giây)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (laserTransform != null)
            {
                laserTransform.Rotate(Vector3.forward, -rotateSpeed * Time.deltaTime);
            }
            yield return null;
        }

        // Despawn laser sau khi hết thời gian
        if (laserTransform != null && laserTransform.GetComponent<NetworkObject>().IsSpawned)
        {
            laserTransform.GetComponent<NetworkObject>().Despawn();
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
        if (healthbar != null)
        {
            healthbar.fillAmount = health / 150f;
        }

        if (health <= 0)
        {
            DropAndRemoveBoss();
        }
    }

    private void DropAndRemoveBoss()
    {
        DropItem item = GetComponent<DropItem>();
        item.DropItemServerRpc();
        RemoveBossServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveBossServerRpc()
    {
        if (monsterManage != null)
        {
            monsterManage.HandleMonsterDeathServerRpc(gameObject);
        }

        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn();
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
