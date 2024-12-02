using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public SkillData skillData; // Gán SkillData cho mỗi nút kỹ năng
    public SkillDescription skillDescriptionPanel; // Tham chiếu đến panel mô tả kỹ năng
    
    // Hàm này được gọi khi nhấn vào kỹ năng
    public void OnImageClick()
    {
        skillDescriptionPanel.ShowDescription(skillData.skillName,
                                              skillData.skillDescription,
                                              skillData.skillType,
                                              skillData.isSkillUnlocked,skillData);
    }
}
