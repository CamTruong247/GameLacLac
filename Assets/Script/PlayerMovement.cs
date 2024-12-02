using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] public SpriteRenderer avatar;

    private float baseSpeed = 3f; 
    private float moveSpeed;
    private Rigidbody2D rb;
    private Vector2 v;

    public Animator animator;
    public SkillData speedSkillData;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = baseSpeed;
    }
    private void Start()
    {
       
    }
    private void Update()
    {
        MoveServerRpc();
        if (speedSkillData.isSkillUnlocked)
        {
            SetMoveSpeedServerRpc(baseSpeed * 2);
        }
        else
        {
            SetMoveSpeedServerRpc(baseSpeed);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetMoveSpeedServerRpc(float newSpeed)
    {
        moveSpeed = newSpeed;
        SetMoveSpeedClientRpc(newSpeed); // Đồng bộ lên tất cả các client
    }

    // ClientRpc để cập nhật moveSpeed trên client
    [ClientRpc]
    private void SetMoveSpeedClientRpc(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    [ServerRpc(RequireOwnership = false)]
    private void AnimatorServerRpc(float speed)
    {
        AnimatorClientRpc(speed);
    }

    [ClientRpc]
    private void AnimatorClientRpc(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    [ClientRpc]
    private void MoveClientRpc()
    {
        if (IsOwner)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            float speed = new Vector2(x, y).magnitude;

            AnimatorServerRpc(speed);
            v = new Vector2(x, y).normalized;

            if (x > 0)
            {
                gameObject.transform.GetChild(0).localScale = new Vector3(1, 1, 0);
            }
            else if (x < 0)
            {
                gameObject.transform.GetChild(0).localScale = new Vector3(-1, 1, 0);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc()
    {
        MoveClientRpc();
    }

    [ClientRpc]
    private void FUMoveClientRpc()
    {
        rb.velocity = new Vector2(v.x * moveSpeed, v.y * moveSpeed);
    }


    [ServerRpc(RequireOwnership = false)]
    public void FUMoveServerRpc()
    {
        FUMoveClientRpc();
    }

    private void FixedUpdate()
    {
        FUMoveServerRpc();
    }
}
