using UnityEngine;

public enum SkillCategory { Active, Passive }

[CreateAssetMenu(fileName = "SkillStat", menuName = "Stat/Skill")]
public abstract class SkillData : ScriptableObject
{
    public string SkillName;
    public Sprite SkillIcon;

    [TextArea]
    public string SkillDescription;

    [Space(40)]
    public float Cooldown;
    public int Damage;

    [Header("스킬 트리")]
    public SkillCategory Category;
    public int SPCost;

    public abstract void UseSkill();
}
