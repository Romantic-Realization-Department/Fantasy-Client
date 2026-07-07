using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 스텝 목록을 저장하는 ScriptableObject 에셋.
/// 외부 시스템에서 기능 해금 시, 이 데이터를 TutorialManager에 전달하여 튜토리얼을 트리거합니다.
/// </summary>
[CreateAssetMenu(fileName = "TutorialSequenceData", menuName = "Tutorial/TutorialSequenceData")]
public class TutorialSequenceData : ScriptableObject
{
    [Header("Tutorial Sequence")]
    [Tooltip("위에서부터 아래로 순차적으로 실행될 튜토리얼 스텝 목록")]
    public List<TutorialStepData> steps = new List<TutorialStepData>();
}
