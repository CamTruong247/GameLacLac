using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Skill : NetworkBehaviour
{
    [SerializeField] private GameObject laser; // GameObject cho kỹ năng laser
    
    public Animator animator;

    // Hàm này sẽ được gọi khi người chơi sử dụng kỹ năng
    [ServerRpc(RequireOwnership = false)]
    public void UseSkillServerRpc()
    {
        if (IsServer)
        {
            Vector3 laserPosition = transform.position + new Vector3(100, 0, 0);
            Debug.Log($"Laser Position: {laserPosition}"); // In ra vị trí laser
            UseSkillClientRpc();
            var l = Instantiate(laser, laserPosition, transform.rotation);
            l.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ClientRpc]
    private void UseSkillClientRpc()
    {
        animator.SetTrigger("UseSkill"); // Gán trigger UseSkill cho hoạt ảnh kỹ năng, nếu có
    }
}
