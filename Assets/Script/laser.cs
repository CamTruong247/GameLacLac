using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    private Rigidbody2D rb;

    // Tốc độ của laser
    public float speed = 10f;
    // Thời gian tồn tại của laser
    public float lifespan = 1.5f; // Thay đổi thành 1.5 giây

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Bắt đầu coroutine để tự động tiêu diệt laser sau một khoảng thời gian
        if (IsServer)
        {
            StartCoroutine(DelayDespawn());
        }
    }

    private void FixedUpdate()
    {
        // Di chuyển laser về phía trước
        rb.velocity = transform.up * speed;
    }

    private IEnumerator DelayDespawn()
    {
        // Đợi trong một khoảng thời gian rồi tiêu diệt laser
        yield return new WaitForSeconds(lifespan);
        GetComponent<NetworkObject>().Despawn(); // Tiêu diệt laser
    }
}
