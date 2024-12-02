using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill Tree/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public string skillType;
    public bool isSkillUnlocked = false; 

    
    public void UnlockSkill()
    {
        isSkillUnlocked = true;
    }

    public void LockSkill()
    {
        isSkillUnlocked = false;
    }
}