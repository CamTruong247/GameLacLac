using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DropItem : NetworkBehaviour
{
    [SerializeField] private GameObject[] Items; // Mảng chứa các vật phẩm (bình máu, bom, v.v.)

    [ServerRpc(RequireOwnership = false)]
    public void DropItemServerRpc()
    {
        DropItemClientRpc();
    }

    [ClientRpc]
    public void DropItemClientRpc()
    {
        // Sử dụng Random.Range với giá trị từ 0 đến 8 để tạo xác suất 1/4 cho bình máu và 1/8 cho bom
        int random = Random.Range(0, 8);

        // Kiểm tra nếu giá trị random là 0 hoặc 1 thì tạo bình máu
        if (random == 0 || random == 1)
        {
            if (IsServer)
            {
                var drop = Instantiate(Items[0], gameObject.transform.position, gameObject.transform.rotation);
                drop.GetComponent<NetworkObject>().Spawn();
            }
        }
        // Kiểm tra nếu giá trị random là 2 thì tạo bom (xác suất 1/8)
        else if (random == 2)
        {
            if (IsServer)
            {
                var drop = Instantiate(Items[1], gameObject.transform.position, gameObject.transform.rotation);
                drop.GetComponent<NetworkObject>().Spawn();
            }
        }
        // Các giá trị khác sẽ không tạo ra vật phẩm nào
    }
}
