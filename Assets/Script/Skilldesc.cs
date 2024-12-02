using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDescription : MonoBehaviour
{
    public GameObject descriptionPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI unlockStatusText;
    public Button unlockButton; // Nút mở khóa kỹ năng

    private SkillData currentSkillData; // Dữ liệu của kỹ năng hiện tại

    // Hàm để hiển thị mô tả kỹ năng và trạng thái mở khóa
    public void ShowDescription(string name, string description, string type, bool isSkillUnlocked, SkillData skillData)
    {
        descriptionPanel.SetActive(true);
        nameText.text = name;
        descriptionText.text = description;
        typeText.text = type;

        currentSkillData = skillData; // Lưu thông tin kỹ năng hiện tại để dùng khi mở khóa

        if (isSkillUnlocked)
        {
            unlockStatusText.text = "Đã mở khóa";  // Kỹ năng đã mở khóa
            unlockButton.interactable = false; // Không thể nhấn nút mở khóa nếu kỹ năng đã mở khóa
        }
        else
        {
            unlockStatusText.text = "Mở khóa";  // Kỹ năng chưa mở khóa
            unlockButton.interactable = true; // Cho phép nhấn nút nếu kỹ năng chưa mở khóa
        }
    }

    // Hàm gọi khi nhấn nút mở khóa
   public void OnUnlockClick()
    {
        Debug.Log("Unlock button clicked!");
        currentSkillData.isSkillUnlocked = true; // Mở khóa kỹ năng
        unlockStatusText.text = "Đã mở khóa"; // Cập nhật trạng thái
        unlockButton.interactable = false; // Vô hiệu hóa nút     
    }

    // Ẩn mô tả kỹ năng
    public void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }
}
