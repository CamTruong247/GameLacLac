using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrapShoot : NetworkBehaviour
{
    [SerializeField] private GameObject bullet; // GameObject cho đạn
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private Transform shootingPoint1;
    [SerializeField] private Transform shootingPoint2;
    [SerializeField] private Transform shootingPoint3;

    private float time = 0;

    void Update()
    {
        if (IsServer) // Chỉ chạy logic này trên server
        {
            time += Time.deltaTime;
            if (time >= 2)
            {
                AttackServerRpc();
                time = 0;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc()
    {
        SpawnBullets(); // Gọi để các client khác có thể thấy hiệu ứng
    }


    private void SpawnBullets()
    {
        // Tạo và spawn đạn từ các điểm khác nhau
        var b = Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
        var b1 = Instantiate(bullet, shootingPoint1.position, shootingPoint1.rotation);
        var b2 = Instantiate(bullet, shootingPoint2.position, shootingPoint2.rotation);
        var b3 = Instantiate(bullet, shootingPoint3.position, shootingPoint3.rotation);

        // Spawn đạn trên server để tất cả client đều thấy
        if (IsServer)
        {
            b.GetComponent<NetworkObject>().Spawn();
            b1.GetComponent<NetworkObject>().Spawn();
            b2.GetComponent<NetworkObject>().Spawn();
            b3.GetComponent<NetworkObject>().Spawn();
        }

        // Set velocity cho đạn
        b.GetComponent<Rigidbody2D>().velocity = shootingPoint.rotation * Vector2.right * 5f;
        b1.GetComponent<Rigidbody2D>().velocity = shootingPoint1.rotation * Vector2.right * 5f;
        b2.GetComponent<Rigidbody2D>().velocity = shootingPoint2.rotation * Vector2.right * 5f;
        b3.GetComponent<Rigidbody2D>().velocity = shootingPoint3.rotation * Vector2.right * 5f;
    }
}
