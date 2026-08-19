using UnityEngine;

/// <summary>
/// 튜토리얼 발동 조건을 정의하는 베이스 ScriptableObject.
/// 구체적인 조건(예: 레벨 N 도달)은 이 클래스를 상속받아 구현합니다.
/// </summary>
public abstract class TutorialConditionBase : ScriptableObject
{
    /// <summary>
    /// 조건이 충족되었는지 매 프레임 검사합니다.
    /// </summary>
    /// <returns>조건 충족 여부</returns>
    public abstract bool CheckCondition();
}
