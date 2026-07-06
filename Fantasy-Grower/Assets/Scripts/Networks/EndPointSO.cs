using UnityEngine;

[CreateAssetMenu]
public class EndPointSO : ScriptableObject
{
    [field: SerializeField]
    public string BaseUrl { get; set; } = "https://fantasy.https.gsmsv.site/v1/";

    [Header("Account")]
    [field: SerializeField]
    public string AccountEndPoint { get; set; } = "account";

    [field: SerializeField]
    public string SignUpEndPoint { get; set; } = "account/signup";

    [Header("Auth")]
    [field: SerializeField]
    public string LoginEndPoint { get; set; } = "auth/login";

    [field: SerializeField]
    public string LogoutEndPoint { get; set; } = "auth/logout";

    [field: SerializeField]
    public string RefreshTokenEndPoint { get; set; } = "auth/refresh";

    [Header("Dungeon")]
    [field: SerializeField]
    public string BasicDungeonStateEndPoint { get; set; } = "dungeons/basic/state";

    [field: SerializeField]
    public string BasicDungeonClaimEndPoint { get; set; } = "dungeons/basic/claim";

    [field: SerializeField]
    public string WeaponDungeonEndPoint { get; set; } = "dungeons/weapon";

    [field: SerializeField]
    public string BossDungeonEndPoint { get; set; } = "dungeons/boss";

    [field: SerializeField]
    public string GoldDungeonStartEndPoint { get; set; } = "dungeons/gold-runs";

    [field: SerializeField]
    public string GoldDungeonClaimEndPoint { get; set; } = "dungeons/gold-runs/{runId}/claim";

    [field: SerializeField]
    public string GoldDungeonAdRewardEndPoint { get; set; } = "dungeons/gold-tickets/ad-reward";

    [field: SerializeField]
    public string DungeonTicketsEndPoint { get; set; } = "dungeons/tickets";

    [Header("Game Data")]
    [field: SerializeField]
    public string JobSkillsEndPoint { get; set; } = "jobs/{jobType}/skills";

    [field: SerializeField]
    public string JobWeaponsEndPoint { get; set; } = "jobs/{jobType}/weapons";

    [field: SerializeField]
    public string LevelsEndPoint { get; set; } = "levels";

    [field: SerializeField]
    public string StagesEndPoint { get; set; } = "stages";

    [Header("Player")]
    [field: SerializeField]
    public string PlayerEndPoint { get; set; } = "player";

    [field: SerializeField]
    public string PlayerLoadoutEndPoint { get; set; } = "player/loadout";

    [field: SerializeField]
    public string PlayerSkillUnlockEndPoint { get; set; } = "player/skill/unlock";

    [Header("Tutorial")]
    [field: SerializeField]
    public string TutorialsEndPoint { get; set; } = "tutorials";

    [field: SerializeField]
    public string TutorialCompleteEndPoint { get; set; } = "tutorials/{tutorialId}/complete";

    [Header("Health")]
    [field: SerializeField]
    public string HealthEndPoint { get; set; } = "health";
}
