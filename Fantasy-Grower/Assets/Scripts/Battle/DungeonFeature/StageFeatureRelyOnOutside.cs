/// <summary>
/// UI 또는 외부 이벤트가 호출할 때 다음 Addressable 스테이지로 진행합니다.
/// </summary>
public class StageFeatureRelyOnOutside : AddressableStageFeatureBase
{
    public void NextStage()
    {
        AdvanceToNextStage();
    }
}
