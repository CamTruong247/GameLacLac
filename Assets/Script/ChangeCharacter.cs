using Unity.Netcode;
using UnityEngine;

public class CharacterImageChanger : NetworkBehaviour
{
    [SerializeField] private GameObject avatar; // GameObject con chứa Avatar
    [SerializeField] private Sprite[] spriteList; // Mảng chứa các sprite để thay đổi
    private SpriteRenderer avatarSpriteRenderer;
    private Animator avatarAnimator;

    private int currentSpriteIndex = 0; // Chỉ số của sprite hiện tại (được truyền qua mạng)
    private int originalSpriteIndex = 0; // Chỉ số của sprite ban đầu

    private bool isSpriteChanged = false; // Kiểm tra trạng thái sprite

    private void Start()
    {
        // Lấy SpriteRenderer và Animator của Avatar
        avatarSpriteRenderer = avatar.GetComponent<SpriteRenderer>();
        avatarAnimator = avatar.GetComponent<Animator>();

        // Kiểm tra nếu SpriteRenderer và Animator không tồn tại
        if (avatarSpriteRenderer == null)
        {
            Debug.LogError("Không tìm thấy SpriteRenderer trên Avatar.");
        }

        if (avatarAnimator == null)
        {
            Debug.LogError("Không tìm thấy Animator trên Avatar.");
        }

        // Kiểm tra nếu spriteList có ít nhất một sprite
        if (spriteList != null && spriteList.Length > 0)
        {
            originalSpriteIndex = 0; // Lưu chỉ số sprite ban đầu
            currentSpriteIndex = originalSpriteIndex; // Đặt sprite ban đầu là sprite hiện tại

            // Cập nhật sprite ban đầu
            avatarSpriteRenderer.sprite = spriteList[originalSpriteIndex];
        }
        else
        {
            Debug.LogError("spriteList không có phần tử nào. Vui lòng thêm sprite vào mảng.");
        }
    }

    private void Update()
    {
        if (IsOwner) // Chỉ người chơi sở hữu mới thay đổi sprite
        {
            if (Input.GetKeyDown(KeyCode.Z)) // Khi nhấn phím Z
            {
                // Gọi ServerRpc để thay đổi sprite trên server
                ChangeSpriteServerRpc();
                isSpriteChanged = !isSpriteChanged; // Đảo trạng thái sprite
            }
        }
    }

    // ServerRpc sẽ thay đổi sprite trên server
    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc()
    {
        // Chuyển đến sprite tiếp theo trong spriteList
        currentSpriteIndex = (currentSpriteIndex + 1) % spriteList.Length;

        // Gọi ClientRpc để thay đổi sprite trên tất cả client
        ChangeSpriteClientRpc(currentSpriteIndex);
    }

    // ClientRpc sẽ thay đổi sprite trên tất cả các client
    [ClientRpc]
    private void ChangeSpriteClientRpc(int spriteIndex)
    {
        // Cập nhật sprite trên client dựa trên chỉ số nhận được
        if (spriteIndex >= 0 && spriteIndex < spriteList.Length)
        {
            avatarSpriteRenderer.sprite = spriteList[spriteIndex];
        }

        // Tắt/bật Animator nếu cần thiết
        if (avatarAnimator != null)
        {
            avatarAnimator.enabled = spriteIndex != originalSpriteIndex; // Tắt Animator nếu sprite là sprite ban đầu
        }
    }
}
