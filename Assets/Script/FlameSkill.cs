using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FlameSkill : NetworkBehaviour
{
    
    [SerializeField] private GameObject flamePrefab;
    public SkillData auraSkillData;

    private bool isAttacking = false; 

    void Update()
    {
        if (auraSkillData != null && auraSkillData.isSkillUnlocked && !isAttacking)
        {
            StartCoroutine(AttackPattern());
        }
    }

    private IEnumerator AttackPattern()
    {
        float trapRadius = 5f;
        isAttacking = true;

        // Chờ 10 giây trước khi tạo các vị trí cháy
        yield return new WaitForSeconds(10f);

       
        GameObject[] traps = new GameObject[16]; // Lưu trữ các đám cháy
        for (int i = 0; i < traps.Length ; i++)
        {
            float angle = 360f / traps.Length * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 spawnPosition = transform.position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) * trapRadius;
            traps[i] = Instantiate(flamePrefab, spawnPosition, Quaternion.identity);
            traps[i].GetComponent<NetworkObject>().Spawn();
        }

       
        yield return new WaitForSeconds(4f);

       
        foreach (var trap in traps)
        {
            if (trap != null)
            {
                NetworkObject trapNetworkObject = trap.GetComponent<NetworkObject>();
                if (trapNetworkObject != null)
                {
                    trapNetworkObject.Despawn();
                }
            }
        }

       
        isAttacking = false;
    }




}
