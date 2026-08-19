/// <summary>
/// 던전 클리어 이벤트를 구독하여 다음 Addressable 스테이지로 자동 진행합니다.
/// </summary>
public class StageFeature : AddressableStageFeatureBase
{
    protected override void Awake()
    {
        base.Awake();

        if (BoundDungeonManager != null)
            BoundDungeonManager.OnDungeonCleared += AdvanceToNextStage;
    }

    protected override void OnDestroy()
    {
        if (BoundDungeonManager != null)
            BoundDungeonManager.OnDungeonCleared -= AdvanceToNextStage;

        base.OnDestroy();
    }
}
