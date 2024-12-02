using UnityEngine;
using Unity.Netcode;

public class SummonPet : NetworkBehaviour
{
    [SerializeField] private GameObject Pet;
    [SerializeField] private float summonDistance = 2.0f;
    [SerializeField] private SkillData summonPetSkillData;
    private float summonPetCooldown = 20f;
    private float lastSummonPetTime = -20f;
    void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.E))
        {
           
            SummonPetServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SummonPetServerRpc()
    {
        if (IsServer)
        {
            // Kiểm tra kỹ năng Summon Pet có mở khóa và cooldown đã hết
            if (summonPetSkillData != null && summonPetSkillData.isSkillUnlocked && Time.time - lastSummonPetTime >= summonPetCooldown)
            {
                // Cập nhật thời gian sử dụng kỹ năng Summon Pet
                lastSummonPetTime = Time.time;

                // Vị trí triệu hồi pet
                Vector3 summonPosition = transform.position + new Vector3(Random.Range(-summonDistance, summonDistance), Random.Range(-summonDistance, summonDistance), 0);
                var pet = Instantiate(Pet, summonPosition, Quaternion.identity);
                pet.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                // Nếu kỹ năng chưa mở khóa hoặc cooldown chưa hết, không làm gì
                Debug.Log("Summon Pet skill not unlocked or still on cooldown");
            }
        }
    }
}
