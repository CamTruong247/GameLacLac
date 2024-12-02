using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject attackpoint;
    [SerializeField] private GameObject menu;
    public float radius;
    public LayerMask layer;        // Layer cho Enemy
    public LayerMask layercheck;   // Layer cho Player
    public LayerMask itemLayer;    // Layer cho Item (vật phẩm)

    private void Update()
    {
        // Kiểm tra kẻ thù (Enemy)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        RaycastHit2D[] hits = Physics2D.CircleCastAll(attackpoint.transform.position, radius, Vector2.zero, 0f, layer);
        foreach (RaycastHit2D hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;

            // Kiểm tra xem đối tượng bị trúng có phải là Enemy hay không
            if (System.Array.Exists(enemies, enemy => enemy == hitObject))
            {
                if (IsServer)
                {
                    hitObject.GetComponent<NetworkObject>().Despawn();
                }
            }
        }

        // Kiểm tra người chơi (Player)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        RaycastHit2D[] hits1 = Physics2D.CircleCastAll(attackpoint.transform.position, radius, Vector2.zero, 0f, layercheck);
        foreach (RaycastHit2D hit in hits1)
        {
            GameObject hitObject = hit.collider.gameObject;
            // Kiểm tra xem đối tượng bị trúng có phải là Player hay không
            if (System.Array.Exists(players, player => player == hitObject))
            {
                menu.SetActive(false);
            }
        }

        // Kiểm tra các đối tượng có tag "Item" và hủy chúng
        RaycastHit2D[] itemHits = Physics2D.CircleCastAll(attackpoint.transform.position, radius, Vector2.zero, 0f, itemLayer);
        foreach (RaycastHit2D hit in itemHits)
        {
            GameObject hitObject = hit.collider.gameObject;
            // Kiểm tra va chạm có phải là Item và gọi Despawn
            if (hitObject.CompareTag("Item"))
            {
                if (IsServer)
                {
                    hitObject.GetComponent<NetworkObject>().Despawn();
                }
            }
        }
    }
}
