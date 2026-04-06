using System.Collections.Generic;

/// <summary>
/// 직업별 스킬 트리 해금 규칙을 추상화하는 전략 인터페이스.
/// 검사(분기형), 궁수(행-택1), 법사(선형) 각각 구현한다.
/// </summary>
public interface ISkillTreeStrategy
{
    /// <summary>
    /// 현재 해금 상태와 트리 규칙을 기반으로 노드 해금 가능 여부를 판별한다.
    /// SP 조건 및 선행 노드 조건은 SkillTreeValidator에서 별도 검증한다.
    /// </summary>
    bool CanUnlock(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state);

    /// <summary>
    /// 노드 해금 직후 호출된다. 전략별 추가 상태 처리에 사용한다.
    /// </summary>
    void OnNodeUnlocked(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state);
}
