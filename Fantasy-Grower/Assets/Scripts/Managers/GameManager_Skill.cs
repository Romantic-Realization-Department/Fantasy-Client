using System.Collections.Generic;

public partial class GameManager
{
    private readonly Dictionary<Career, Dictionary<SkillNodeData, bool>> careerUnlockedStates =
        new();
    private readonly Dictionary<Career, List<ActiveSkillData>> careerEquippedActives = new();
    private readonly Dictionary<Career, List<PassiveSkillData>> careerEquippedPassives = new();

    /// <summary>
    /// 지정된 직업의 스킬 해금 상태 딕셔너리를 반환한다.
    /// 없으면 새로 생성하여 반환한다. (딕셔너리에 없는 노드는 잠김으로 간주)
    /// </summary>
    public Dictionary<SkillNodeData, bool> GetUnlockedState(Career career)
    {
        if (!careerUnlockedStates.TryGetValue(career, out var state))
        {
            state = new Dictionary<SkillNodeData, bool>();
            careerUnlockedStates[career] = state;
        }
        return state;
    }

    /// <summary>
    /// 지정된 직업의 액티브 스킬 장착 슬롯 리스트를 반환한다.
    /// </summary>
    public List<ActiveSkillData> GetEquippedActives(Career career, int requiredSlots)
    {
        if (!careerEquippedActives.TryGetValue(career, out var actives))
        {
            actives = new List<ActiveSkillData>(new ActiveSkillData[requiredSlots]);
            careerEquippedActives[career] = actives;
        }
        else if (actives.Count < requiredSlots)
        {
            // 스킬 슬롯이 확장되었을 경우를 대비한 보정
            while (actives.Count < requiredSlots)
            {
                actives.Add(null);
            }
        }
        return actives;
    }

    /// <summary>
    /// 지정된 직업의 패시브 스킬 장착 슬롯 리스트를 반환한다.
    /// </summary>
    public List<PassiveSkillData> GetEquippedPassives(Career career, int requiredSlots)
    {
        if (!careerEquippedPassives.TryGetValue(career, out var passives))
        {
            passives = new List<PassiveSkillData>(new PassiveSkillData[requiredSlots]);
            careerEquippedPassives[career] = passives;
        }
        else if (passives.Count < requiredSlots)
        {
            // 스킬 슬롯이 확장되었을 경우를 대비한 보정
            while (passives.Count < requiredSlots)
            {
                passives.Add(null);
            }
        }
        return passives;
    }
}
