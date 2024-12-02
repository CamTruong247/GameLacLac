using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class AuraEffect : NetworkBehaviour
{
    public float radius = 1f; // Phạm vi của Aura
    public float damagePerSecond = 1f; // Sát thương mỗi giây
    public SkillData auraSkillData; // Tham chiếu đến skill data của aura

    private LineRenderer lineRenderer; // Dùng để vẽ aura

    private void Start()
    {
        // Kiểm tra nếu kỹ năng aura đã mở khóa và bắt đầu hiệu ứng
        if (auraSkillData != null && auraSkillData.isSkillUnlocked)
        {
            StartCoroutine(DamageEnemies());
        }

        // Khởi tạo LineRenderer để vẽ aura
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 120; // Số điểm vẽ
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    private IEnumerator DamageEnemies()
    {
        while (true)
        {
            if (IsServer)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Enemy"))
                    {
                        // Kiểm tra loại quái và áp dụng sát thương
                        ApplyDamage(hit.gameObject);
                    }
                }
            }

            yield return new WaitForSeconds(1f); // Gây sát thương mỗi giây
        }
    }

    private void ApplyDamage(GameObject enemy)
    {
        // Áp dụng sát thương theo từng loại kẻ địch
        if (enemy.TryGetComponent(out werewolfmovement werewolfMovement))
        {
            werewolfMovement.UpdateHealthServerRpc(damagePerSecond);
        }
        else if (enemy.TryGetComponent(out SlimeMovement slimeMovement))
        {
            slimeMovement.UpdateHealthServerRpc(damagePerSecond);
        }
        else if (enemy.TryGetComponent(out GolemBoss golemMovement))
        {
            golemMovement.UpdateHealthServerRpc(damagePerSecond);
        }
        else if (enemy.TryGetComponent(out SlimeKingMovement slimeKingMovement))
        {
            slimeKingMovement.UpdateHealthServerRpc(damagePerSecond);
        }
    }

    private void Update()
    {
        if (auraSkillData != null && auraSkillData.isSkillUnlocked)
        {
            if (lineRenderer != null)
            {
                // Cập nhật vòng tròn aura với LineRenderer chỉ khi kỹ năng đã mở
                DrawAura();
            }
        }
    }

    private void DrawAura()
    {
        float angleStep = 360f / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            lineRenderer.SetPosition(i, transform.position + position);
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, lineRenderer.GetPosition(0));
    }
}
