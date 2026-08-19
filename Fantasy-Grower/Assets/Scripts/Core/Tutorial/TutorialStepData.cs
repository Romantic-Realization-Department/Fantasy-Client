using System;
using UnityEngine;

public enum TutorialActionType
{
    /// <summary>특정 UI가 클릭될 때까지 대기</summary>
    CLICK_UI,

    /// <summary>대화창만 띄우고 화면 클릭 시 바로 넘어감</summary>
    DIALOGUE_ONLY,
}

public enum PointerDirection
{
    None,
    Up,
    Down,
    Left,
    Right,
}

/// <summary>
/// 인스펙터에서 설정 가능한 단일 튜토리얼 스텝의 데이터 컨테이너
/// </summary>
[Serializable]
public class TutorialStepData
{
    [Tooltip("튜토리얼 달성 조건 타입")]
    public TutorialActionType actionType;

    [Tooltip("UIRegistry에 등록된 타겟의 ID (예: Btn_Dungeon)")]
    public UIKeyRegistry targetUI_ID;

    [TextArea(2, 5)]
    [Tooltip("패널에 출력될 가이드 텍스트")]
    public string dialogueText;

    [Tooltip("타겟 UI를 가리킬 화살표의 방향")]
    public PointerDirection arrowDir;

    [Header("Custom Layout (선택)")]
    [Tooltip("자동 배치 로직을 무시하고 기획자가 아래 값들을 직접 지정할지 여부")]
    public bool useCustomPosition = false;

    [Header("- Dialogue Panel Custom")]
    [Tooltip("타겟 위치를 기준으로 한 대화창의 AnchoredPosition (픽셀 단위)")]
    public Vector2 customDialogueAnchoredPosition = new Vector2(0, 150);

    [Tooltip("대화창 패널의 너비(Width)와 높이(Height)")]
    public Vector2 customDialogueSize = new Vector2(400, 200);

    [Header("- Pointer Arrow Custom")]
    [Tooltip("타겟 위치를 기준으로 한 화살표의 AnchoredPosition (픽셀 단위)")]
    public Vector2 customArrowAnchoredPosition = new Vector2(0, 100);

    [Tooltip("화살표 이미지의 너비(Width)와 높이(Height)")]
    public Vector2 customArrowSize = new Vector2(100, 100);
}
