using UnityEngine;

/// <summary>테스트용 액티브 스킬. UseSkill()은 콘솔 출력만 수행한다.</summary>
[CreateAssetMenu(fileName = "TestActiveSkill", menuName = "ScriptableObjects/SkillTree/Test/ActiveSkill")]
public class TestActiveSkill : ActiveSkillData
{
    public override void UseSkill()
    {
        Debug.Log($"[{SkillName}] 발동!");
    }
}
