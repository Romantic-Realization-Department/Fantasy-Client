using UnityEngine;

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

    public abstract void UseSkill();
}
